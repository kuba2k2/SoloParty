using System;
using HMUI;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using SoloParty.Data.Record;
using SoloParty.Data.HighScore;
using SoloParty.Utils;

namespace SoloParty.AffinityPatches;

internal sealed class SoloFreePlayFlowCoordinatorPatches(
	PluginConfig config,
	SoloRecordManager recordManager,
	SiraLog log,
	EnterPlayerGuestNameViewController enterNameViewController,
	IHighScoreSetter highScoreSetter
) : IAffinity
{
	private LevelCompletionResults? _lastLevelCompletionResults;
	private bool _isNewHighScore;
	private bool _replaceNextViewController;

	// NOTE: somehow, order matters - moving this below LevelDidFinishPatch() makes the patch invisible!
	[AffinityPostfix]
	[AffinityPatch(
		typeof(SoloFreePlayFlowCoordinator),
		nameof(SoloFreePlayFlowCoordinator.IsNewHighScore)
	)]
	private void IsNewHighScorePatch(
		ref bool __result,
		LevelCompletionResults levelCompletionResults
	)
	{
		if (!config.ModEnabled || !config.ResultsHighScoreFireworks)
			return;
		if (!ReferenceEquals(levelCompletionResults, _lastLevelCompletionResults))
			return;

		// replace the IsNewHighScore() method's result according to the player name's high score
		__result = _isNewHighScore;
	}

	[AffinityPrefix]
	[AffinityPatch(
		typeof(SoloFreePlayFlowCoordinator),
		nameof(SoloFreePlayFlowCoordinator.ProcessLevelCompletionResultsAfterLevelDidFinish)
	)]
	private bool LevelDidFinishPatch(
		SoloFreePlayFlowCoordinator __instance,
		LevelCompletionResults levelCompletionResults,
		IReadonlyBeatmapData transformedBeatmapData,
		BeatmapKey beatmapKey,
		BeatmapLevel beatmapLevel,
		GameplayModifiers modifiers,
		bool practice
	)
	{
		// return immediately if these results were already handled
		if (ReferenceEquals(levelCompletionResults, _lastLevelCompletionResults))
			return true;

		// check if mod is enabled
		if (!config.ModEnabled || !config.SoloChooserEnabled)
		{
			log.Info("Skipping name input: mod or name chooser disabled");
			return true;
		}

		// call base class to handle completion results, skip patch if requested
		if (__instance.HandleBasicLevelCompletionResults(levelCompletionResults, practice))
		{
			log.Info("Skipping name input: results handled");
			return true;
		}

		// skip practice mode
		if (practice)
		{
			log.Info("Skipping name input: practice mode");
			return true;
		}

		// skip if level wasn't cleared
		if (levelCompletionResults.levelEndStateType != LevelCompletionResults.LevelEndStateType.Cleared)
		{
			log.Info("Skipping name input: level not cleared");
			return true;
		}

		// remember which particular results are being processed right now
		_lastLevelCompletionResults = levelCompletionResults;

		var beatmapKeyString = beatmapKey.ToBeatmapKeyString();
		var date = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		var modifiedScore = levelCompletionResults.modifiedScore;
		log.Info(
			$"Beatmap '{beatmapKeyString}' finished at {date} " +
			$"with score {modifiedScore}, redirecting to player name view"
		);

		enterNameViewController.Init((_, playerName) =>
		{
			log.Info($"Got player name '{playerName}'");

			// fetch the previous high score for this player
			var previousHighScore = recordManager
				.GetRecordPlayerBest(beatmapKeyString, playerName)
				?.ModifiedScore ?? 0;
			log.Info($"- previous high score is {previousHighScore}");
			_isNewHighScore = modifiedScore > previousHighScore;

			// update previous high score in other mods
			if (config.ResultsHighScoreSetter)
				highScoreSetter.UpdateHighScore(levelCompletionResults, previousHighScore);

			// save a new record
			var record = new SoloRecord { Date = date, ModifiedScore = modifiedScore, PlayerName = playerName };
			recordManager.AddRecord(beatmapKeyString, record);

			// redirect the PresentViewController() call to ReplaceTopViewController()
			_replaceNextViewController = true;
			// call the original method to show results view
			__instance.ProcessLevelCompletionResultsAfterLevelDidFinish(
				levelCompletionResults,
				transformedBeatmapData,
				beatmapKey,
				beatmapLevel,
				modifiers,
				practice
			);
		});

		// present the view controller and skip the original method
		__instance.PresentViewController(enterNameViewController, immediately: true);
		return false;
	}

	[AffinityPrefix]
	[AffinityPatch(
		typeof(FlowCoordinator),
		nameof(FlowCoordinator.PresentViewController)
	)]
	private bool PresentViewControllerPatch(
		FlowCoordinator __instance,
		ViewController viewController
	)
	{
		if (!config.ModEnabled)
			return true;
		if (__instance is not SoloFreePlayFlowCoordinator coordinator)
			return true;
		if (!ReferenceEquals(viewController, coordinator._resultsViewController))
			return true;
		if (!_replaceNextViewController)
			return true;

		// allow to redirect the call once per request
		_replaceNextViewController = false;
		coordinator.ReplaceTopViewController(viewController);
		// do not call the original method anymore
		return false;
	}
}
