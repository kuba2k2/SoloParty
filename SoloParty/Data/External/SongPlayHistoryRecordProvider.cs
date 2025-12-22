using System;
using System.Collections.Concurrent;
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

	public readonly ConcurrentDictionary<BeatmapKey, Tuple<int, int>> ScoringCache = new();

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
			.Select(record => ConvertRecord(beatmapKey, record))
			.ToList();
	}

	private SoloRecord ConvertRecord(BeatmapKey beatmapKey, ISongPlayRecord record)
	{
		// fetch scoring info from cache
		var (notesCount, maxMultipliedScore) = ScoringCache
			.GetValueOrDefault(beatmapKey, new Tuple<int, int>(-1, -1));

		var endState = record.LevelEnd == LevelEndType.Cleared ? EndState.Cleared : EndState.SoftFailed;
		return new SoloRecord
		{
			Date = record.LocalTime.ToUnixTimeMilliseconds(),
			// SPH saves score at the time of soft-fail, but we don't want that here, so set it as "unknown"
			ModifiedScore = endState == EndState.Cleared ? record.ModifiedScore : -1,
			MultipliedScore = endState == EndState.Cleared ? record.RawScore : -1,
			MaxModifiedScore = -1,
			MaxMultipliedScore = maxMultipliedScore,
			NotesPassed = record.LastNote,
			NotesCount = notesCount,
			EndState = endState,
			Modifiers = record.Params.ToSoloModifier(),
			IsExternal = true
		};
	}
}
