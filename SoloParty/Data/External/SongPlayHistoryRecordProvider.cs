using System;
using System.Collections.Generic;
using System.Linq;
using SoloParty.Data.Manager;
using SoloParty.Data.Models;
using SoloParty.Utils;
using SongPlayHistory.Model;
using SongPlayHistory.SongPlayData;
using Zenject;

namespace SoloParty.Data.External;

internal class SongPlayHistoryRecordProvider(
	ExternalRecordManager externalManager,
	IRecordManager songPlayManager
) : AbstractRecordProvider, IInitializable, IDisposable
{
	public override string ProviderName => "SongPlayHistory";

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
		return songPlayManager
			.GetRecords(beatmapKey)
			// take only cleared or soft-failed records
			.Where(record => record.LevelEnd == LevelEndType.Cleared || record.Params.HasFlag(SongPlayParam.NoFail))
			.Select(ConvertRecord)
			.ToList();
	}

	private static SoloRecord ConvertRecord(ISongPlayRecord record)
	{
		var endState = record.LevelEnd == LevelEndType.Cleared ? EndState.Cleared : EndState.SoftFailed;
		return new SoloRecord
		{
			Date = record.LocalTime.ToUnixTimeMilliseconds(),
			// SPH saves score at the time of soft-fail, but we don't want that here, so set it as "unknown"
			ModifiedScore = endState == EndState.Cleared ? record.ModifiedScore : -1,
			MultipliedScore = endState == EndState.Cleared ? record.RawScore : -1,
			NotesPassed = record.LastNote,
			EndState = endState,
			Modifiers = record.Params.ToSoloModifier(),
			IsExternal = true
		};
	}
}
