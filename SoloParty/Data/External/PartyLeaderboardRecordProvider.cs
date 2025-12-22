using System;
using System.Collections.Generic;
using System.Linq;
using SoloParty.Data.Manager;
using SoloParty.Data.Models;
using Zenject;

namespace SoloParty.Data.External;

internal class PartyLeaderboardRecordProvider(
	ExternalRecordManager externalManager,
	ExternalImportManager importManager,
	LocalLeaderboardsModel localLeaderboardsModel
) : AbstractRecordImporter, IInitializable, IDisposable
{
	public override string ProviderName => "PartyLeaderboard";

	public void Initialize()
	{
		externalManager.Register(this);
		importManager.Register(this);
	}

	public void Dispose()
	{
		importManager.Unregister(this);
		externalManager.Unregister(this);
	}

	public override List<SoloRecord> GetRecords(BeatmapKey beatmapKey)
	{
		var leaderboardId = LocalLeaderboardsIdModel.GetLocalLeaderboardID(beatmapKey);
		return localLeaderboardsModel
			.GetScores(leaderboardId, LocalLeaderboardsModel.LeaderboardType.AllTime)
			?.Select(ConvertRecord)
			?.ToList() ?? [];
	}

	public override Dictionary<string, List<SoloRecord>> GetAllRecords()
	{
		return localLeaderboardsModel._leaderboardsData
			.Select(data => (
				ConvertLeaderboardId(data._leaderboardId),
				data._scores.Select(ConvertRecord).ToList()
			))
			.Where(tuple => tuple.Item1 != "")
			.ToDictionary(
				keySelector: tuple => tuple.Item1,
				elementSelector: tuple => tuple.Item2
			);
	}

	private static SoloRecord ConvertRecord(LocalLeaderboardsModel.ScoreData score)
	{
		var endState = score._fullCombo ? EndState.FullCombo : EndState.Cleared;
		return new SoloRecord
		{
			Date = score._timestamp * 1000,
			ModifiedScore = score._score,
			EndState = endState,
			PlayerName = score._playerName,
			IsExternal = true
		};
	}

	private static string ConvertLeaderboardId(string leaderboardId)
	{
		List<string> difficultyList = ["Easy", "Normal", "Hard", "Expert", "ExpertPlus"];
		List<string> characteristicList = ["Standard", "OneSaber", "NoArrows", "90Degree", "360Degree", "Lawless"];

		var difficulty = difficultyList.FirstOrDefault(leaderboardId.EndsWith);
		if (difficulty == null)
			return "";
		leaderboardId = leaderboardId[..^difficulty.Length];

		var characteristic = characteristicList.FirstOrDefault(leaderboardId.EndsWith);
		if (characteristic != null)
			leaderboardId = leaderboardId[..^difficulty.Length];
		else
			characteristic = "Standard";

		return $"{leaderboardId}___{difficultyList.IndexOf(difficulty)}___{characteristic}";
	}
}
