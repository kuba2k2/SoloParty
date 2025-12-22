using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SoloParty.Data.Manager;
using SoloParty.Data.Models;
using SoloParty.Utils;
using SongPlayHistory.Model;
using SongPlayHistory.SongPlayData;
using Zenject;

namespace SoloParty.Data.External;

internal class SongPlayHistoryRecordProvider(
	ExternalRecordManager externalManager,
	ExternalImportManager importManager,
	IRecordManager songPlayManager
) : AbstractRecordImporter, IInitializable, IDisposable
{
	public override string ProviderName => "SongPlayHistory";

	private readonly ConcurrentDictionary<BeatmapKey, Tuple<int, int>> _scoringCache = new();

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

	public void AddScoringInfo(BeatmapKey beatmapKey, int notesCount, int maxMultipliedScore)
	{
		if (_scoringCache.ContainsKey(beatmapKey))
			return;
		_scoringCache[beatmapKey] = new Tuple<int, int>(notesCount, maxMultipliedScore);
		InvokeRecordsUpdated(beatmapKey);
	}

	public override List<SoloRecord> GetRecords(BeatmapKey beatmapKey)
	{
		return songPlayManager
			.GetRecords(beatmapKey)
			// take only cleared or soft-failed records
			.Where(record => record.LevelEnd == LevelEndType.Cleared || record.Params.HasFlag(SongPlayParam.NoFail))
			// ignore practice mode records
			.Where(record => !record.Params.HasFlag(SongPlayParam.SubmissionDisabled))
			.Select(record => ConvertRecord(beatmapKey, record))
			.Where(record => record.ModifiedScore != -1)
			.ToList();
	}

	public override Dictionary<string, List<SoloRecord>> GetAllRecords()
	{
		var records =
			songPlayManager
				.GetType()
				.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
				.FirstOrDefault(property => property.Name == "Records")
				?.GetGetMethod(true)
				?.Invoke(songPlayManager, []);

		var result = new Dictionary<string, List<SoloRecord>>();
		if (records == null)
			return result;

		foreach (DictionaryEntry entry in (IDictionary)records!)
		{
			var list = (
				from object? record in (IEnumerable)entry.Value
				select ConvertRecord(default, (ISongPlayRecord)record!)
			).ToList();
			result.Add((string)entry.Key, list);
		}

		return result;
	}

	private SoloRecord ConvertRecord(BeatmapKey beatmapKey, ISongPlayRecord record)
	{
		// fetch scoring info from cache
		var (notesCount, maxMultipliedScore) = _scoringCache
			.GetValueOrDefault(beatmapKey, new Tuple<int, int>(-1, -1));

		var endState = record.LevelEnd == LevelEndType.Cleared ? EndState.Cleared : EndState.SoftFailed;

		// apparently SPH doesn't always set LastNote at the time of soft-fail, so try to detect one
		if (record.Params.HasFlag(SongPlayParam.NoFail) && record.ModifiedScore <= record.RawScore / 2)
			endState = EndState.SoftFailed;

		return new SoloRecord
		{
			Date = record.LocalTime.ToUnixTimeMilliseconds(),
			// SPH saves score at the time of soft-fail, but we don't want that here, so set it as "unknown"
			// some versions didn't track soft-fails, so the LevelEndType was set to Cleared
			ModifiedScore = record.LevelEnd == LevelEndType.Cleared ? record.ModifiedScore : -1,
			MultipliedScore = record.LevelEnd == LevelEndType.Cleared ? record.RawScore : -1,
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
