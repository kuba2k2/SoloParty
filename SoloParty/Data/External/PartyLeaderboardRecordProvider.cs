using System;
using System.Collections.Generic;
using System.Linq;
using SoloParty.Data.Manager;
using SoloParty.Data.Models;
using Zenject;

namespace SoloParty.Data.External;

internal class PartyLeaderboardRecordProvider(
	ExternalRecordManager externalManager,
	LocalLeaderboardsModel localLeaderboardsModel
) : AbstractRecordProvider, IInitializable, IDisposable
{
	public override string ProviderName => "PartyLeaderboard";

	public void Initialize()
	{
		externalManager.Register(this);
	}

	public void Dispose()
	{
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
}
