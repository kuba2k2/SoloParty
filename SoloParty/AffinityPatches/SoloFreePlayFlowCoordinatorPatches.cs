using System;
using BS_Utils.Gameplay;
using HMUI;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using SoloParty.Data.HighScore;
using SoloParty.Data.Manager;
using SoloParty.Data.Models;
using SoloParty.Utils;

namespace SoloParty.AffinityPatches;

internal sealed class SoloFreePlayFlowCoordinatorPatches(
	PluginConfig config,
	SoloRecordManager soloRecordManager,
	ExternalRecordManager externalRecordManager,
	SiraLog log,
	EnterPlayerGuestNameViewController enterNameViewController,
	IHighScoreSetter highScoreSetter
) : IAffinity
{
	private LevelCompletionResults? _lastLevelCompletionResults;
	private bool? _isNewHighScore;
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
		if (_isNewHighScore != null)
			__result = _isNewHighScore.Value;
		_isNewHighScore = null;
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
		if (!config.ModEnabled || !config.SoloTrackingEnabled)
		{
			log.Info("Skipping result handling: mod disabled or score tracking disabled");
			return true;
		}

		// call base class to handle completion results, skip patch if requested
		if (__instance.HandleBasicLevelCompletionResults(levelCompletionResults, practice))
		{
			log.Info("Skipping result handling: results handled");
			return true;
		}

		// skip practice mode and zen mode
		if (practice || levelCompletionResults.gameplayModifiers.zenMode)
		{
			log.Info("Skipping result handling: practice or zen mode");
			return true;
		}

		// skip if score submission was disabled
		if (ScoreSubmission.WasDisabled)
		{
			log.Info($"Skipping result handling: score submission disabled by {ScoreSubmission.LastDisabledModString}");
			return true;
		}

		// skip if level wasn't cleared
		if (levelCompletionResults.levelEndStateType != LevelCompletionResults.LevelEndStateType.Cleared)
		{
			log.Info("Skipping result handling: level not cleared");
			return true;
		}

		// remember which particular results are being processed right now
		_lastLevelCompletionResults = levelCompletionResults;

		var date = DateTimeOffset.Now.ToUnixTimeMilliseconds();

		// player name chooser disabled, save results and call the original method
		if (!config.SoloChooserEnabled)
		{
			SaveResults(__instance, beatmapKey, levelCompletionResults, transformedBeatmapData, date, null);
			return true;
		}

		// player name chooser enabled, redirect view controller
		log.Info("Redirecting to player name chooser");
		enterNameViewController.Init((_, playerName) =>
		{
			log.Info($"Got player name '{playerName}'");
			SaveResults(__instance, beatmapKey, levelCompletionResults, transformedBeatmapData, date, playerName);

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

	private void SaveResults(
		SoloFreePlayFlowCoordinator coordinator,
		BeatmapKey beatmapKey,
		LevelCompletionResults levelCompletionResults,
		IReadonlyBeatmapData beatmapData,
		long date,
		string? playerName
	)
	{
		var modifiedScore = levelCompletionResults.modifiedScore;
		var multipliedScore = levelCompletionResults.multipliedScore;
		var maxModifiedScore = -1;
		var maxMultipliedScore = -1;
		var notesPassed = -1;
		var notesCount = beatmapData.cuttableNotesCount;
		var endState = levelCompletionResults.fullCombo ? EndState.FullCombo : EndState.Cleared;

		log.Info(
			$"Beatmap '{beatmapKey.ToBeatmapKeyString()}' finished at {date} " +
			$"with score {modifiedScore}, player name is '{playerName}'"
		);

		// fetch the previous high score for this player (if name set; otherwise don't change the high score at all)
		_isNewHighScore = null;
		if (playerName != null)
		{
			var previousHighScore = externalRecordManager
				.GetRecordPlayerBest(beatmapKey, playerName)
				?.ModifiedScore ?? 0;
			log.Info($"- previous high score was {previousHighScore}");
			_isNewHighScore = modifiedScore > previousHighScore;

			// update previous high score in other mods
			if (config.ResultsHighScoreSetter)
				highScoreSetter.UpdateHighScore(levelCompletionResults, previousHighScore);
		}

		// get max modified/multiplied scores if RankModelPatches saved them
		if (RankModelPatches.ModifiedScore == modifiedScore && RankModelPatches.MultipliedScore == multipliedScore)
		{
			maxModifiedScore = RankModelPatches.MaxModifiedScore;
			maxMultipliedScore = RankModelPatches.MaxMultipliedScore;
		}
		else
		{
			log.Warn($"RankModelPatches didn't save the max scores! " +
			         $"modified = {modifiedScore}/{RankModelPatches.ModifiedScore}, " +
			         $"multiplied = {multipliedScore}/{RankModelPatches.MultipliedScore}");
		}

		// get notes passed (before soft-failing, optionally)
		if (GameEnergyCounterPatches.GameStarted)
		{
			notesPassed = GameEnergyCounterPatches.NotesPassed;
			// cuttableNotesCount doesn't include burst slider elements, so take total count from GameEnergyCounter
			notesCount = Math.Max(notesCount, GameEnergyCounterPatches.NotesCount);
			if (levelCompletionResults.gameplayModifiers.noFailOn0Energy && GameEnergyCounterPatches.EnergyDidReach0)
				endState = EndState.SoftFailed;
			GameEnergyCounterPatches.GameStarted = false;
		}
		else
		{
			log.Warn($"GameEnergyCounterPatches didn't save passed notes! " +
			         $"notesPassed = {GameEnergyCounterPatches.NotesPassed}");
		}

		// check again if soft-failed
		// (don't ask, that's how the base game does this)
		if (levelCompletionResults.gameplayModifiers.noFailOn0Energy &&
		    levelCompletionResults.energy <= 9.999999747378752E-06)
			endState = EndState.SoftFailed;

		// save a new record
		var record = new SoloRecord
		{
			Date = date,
			ModifiedScore = modifiedScore,
			MultipliedScore = multipliedScore,
			MaxModifiedScore = maxModifiedScore,
			MaxMultipliedScore = maxMultipliedScore,
			GoodCutsCount = levelCompletionResults.goodCutsCount,
			BadCutsCount = levelCompletionResults.badCutsCount,
			MissedCount = levelCompletionResults.missedCount,
			MaxCombo = levelCompletionResults.maxCombo,
			NotesPassed = notesPassed,
			NotesCount = notesCount,
			Pauses = GamePausePatches.Pauses,
			NoteJumpOffset = (int)(coordinator.playerSettings.noteJumpStartBeatOffset * 100),
			EndState = endState,
			Modifiers = levelCompletionResults.gameplayModifiers.ToSoloModifier(),
			PlayerName = playerName,
			IsLatest = true
		};
		log.Info($"Saving record: {record}");
		soloRecordManager.AddRecord(beatmapKey, record);
		soloRecordManager.SaveRecords();
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
