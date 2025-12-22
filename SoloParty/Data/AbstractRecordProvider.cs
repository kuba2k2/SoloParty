using System;
using System.Collections.Generic;
using System.Linq;
using SoloParty.Data.Models;

namespace SoloParty.Data;

public abstract class AbstractRecordProvider
{
	public abstract string ProviderName { get; }
	public abstract List<SoloRecord> GetRecords(BeatmapKey beatmapKey);

	public event Action<BeatmapKey>? RecordsUpdatedEvent;

	protected void InvokeRecordsUpdated(BeatmapKey beatmapKey)
	{
		RecordsUpdatedEvent?.Invoke(beatmapKey);
	}

	public SoloRecord? GetRecordPlayerBest(BeatmapKey beatmapKey, string playerName)
	{
		return GetRecords(beatmapKey)
			.Where(record => record.PlayerName == playerName)
			.OrderByDescending(record => record.ModifiedScore)
			.FirstOrDefault();
	}

	public SoloRecord? GetRecordMatching(BeatmapKey beatmapKey, long date, int modifiedScore)
	{
		return GetRecords(beatmapKey)
			.Where(record => record.ModifiedScore == modifiedScore)
			.FirstOrDefault(record => Math.Abs(record.Date - date) <= 10000);
	}
}
