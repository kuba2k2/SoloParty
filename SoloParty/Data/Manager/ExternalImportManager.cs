using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiraUtil.Logging;
using SoloParty.Data.Models;
using SoloParty.Utils;
using UnityEngine;

namespace SoloParty.Data.Manager;

public class ExternalImportManager(
	SoloRecordManager recordManager,
	SiraLog log
)
{
	private readonly IList<AbstractRecordImporter> _importers = [];

	public void Register(AbstractRecordImporter importer)
	{
		if (_importers.Contains(importer))
			return;
		_importers.Add(importer);
	}

	public void Unregister(AbstractRecordImporter importer)
	{
		if (!_importers.Contains(importer))
			return;
		_importers.Remove(importer);
	}

	public AbstractRecordImporter? GetByName(string providerName)
	{
		return _importers.FirstOrDefault(provider => provider.ProviderName == providerName);
	}

	public class ImportResult
	{
		public int AddCount;
		public int MergeCount;
		public int SameCount;
	}

	public Task<ImportResult> ImportRecords(AbstractRecordImporter importer, string? playerName) =>
		Task.Run(() => ImportRecordsImpl(importer, playerName));

	private ImportResult ImportRecordsImpl(AbstractRecordImporter importer, string? playerName)
	{
		var result = new ImportResult();
		log.Info($"Starting import from {importer.ProviderName} with player name '{playerName}'");
		recordManager.CreateBackup($"import-{importer.ProviderName}");

		var allRecords = importer.GetAllRecords();
		log.Info($"Loaded records for {allRecords.Count} beatmaps");

		foreach (var (beatmapKey, records) in allRecords)
		{
			log.Info($"- processing beatmap {beatmapKey}, {records.Count} records");
			// fill in player name if requested
			records.ForEach(record => record.PlayerName ??= playerName);
			// get SoloRecords for this map
			var soloRecords = recordManager.GetRecords(beatmapKey);
			// run the import function
			ImportBeatmap(
				result,
				beatmapKey,
				records
					.GroupBy(record => record.Date.ToLocalDateTime().ToShortDateString())
					.ToDictionary(record => record.Key, record => record.ToList()),
				soloRecords
					.GroupBy(record => record.Date.ToLocalDateTime().ToShortDateString())
					.ToDictionary(record => record.Key, record => record.ToList()),
				importer.IsDatePrecise
			);
		}

		recordManager.SaveRecords(force: true);
		return result;
	}

	private void ImportBeatmap(
		ImportResult result,
		string beatmapKey,
		Dictionary<string, List<SoloRecord>> extAllRecords,
		Dictionary<string, List<SoloRecord>> soloAllRecords,
		bool isDatePrecise
	)
	{
		foreach (var (date, extRecords) in extAllRecords)
		{
			if (!soloAllRecords.TryGetValue(date, out var soloRecords))
			{
				// there are no SoloRecords at all for this date, import everything
				extRecords.ForEach(extRecord => recordManager.AddRecord(beatmapKey, extRecord));
				result.AddCount += extRecords.Count;
				continue;
			}

			foreach (var extRecord in extRecords)
			{
				// find a matching SoloRecord
				SoloRecord? soloRecord;
				if (isDatePrecise)
					// use full match (score + date) if the external date is precise
					soloRecord = soloRecords
						.FirstOrDefault(record =>
							record.Matches(extRecord)
						);
				else
					// otherwise just check if the score matches
					soloRecord = soloRecords
						.FirstOrDefault(record =>
							DistanceMatches(record.ModifiedScore, extRecord.ModifiedScore, 0.001) ||
							DistanceMatches(record.MultipliedScore, extRecord.MultipliedScore, 0.001)
						);

				if (soloRecord != null)
				{
					// SoloRecord found by precise match, merge it
					var hashCode = soloRecord.GetHashCode();
					soloRecord.MergeFrom(extRecord, mustMatch: false);
					if (hashCode == soloRecord.GetHashCode())
						result.SameCount += 1;
					else
						result.MergeCount += 1;
					continue;
				}

				if (isDatePrecise)
				{
					// SoloRecord not found and date should be precise, add a new one
					recordManager.AddRecord(beatmapKey, extRecord);
					result.AddCount += 1;
					continue;
				}

				// external date is not precise, and no "close enough" score match was found
				// ignore for now
				continue;
			}
		}
	}

	private static bool DistanceMatches(int a, int b, double threshold)
	{
		if (a == b)
			return true;
		if (a <= 0 || b <= 0)
			return false;
		var distance = Mathf.Abs(a - b) / (double)Mathf.Max(a, b);
		return distance < threshold;
	}
}
