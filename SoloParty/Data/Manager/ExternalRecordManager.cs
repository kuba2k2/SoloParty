using System.Collections.Generic;
using System.Linq;
using SoloParty.Data.Models;

namespace SoloParty.Data.Manager;

public class ExternalRecordManager(
	SoloRecordManager recordManager
) : ISoloRecordProvider
{
	public string ProviderName => "External";

	private readonly IList<ISoloRecordProvider> _providers = [];

	public void Register(ISoloRecordProvider provider)
	{
		if (_providers.Contains(provider))
			return;
		_providers.Add(provider);
	}

	public void Unregister(ISoloRecordProvider provider)
	{
		if (!_providers.Contains(provider))
			return;
		_providers.Remove(provider);
	}

	public ISoloRecordProvider? GetByName(string providerName)
	{
		return _providers.FirstOrDefault(provider => provider.ProviderName == providerName);
	}

	public List<SoloRecord> GetRecords(BeatmapKey beatmapKey)
	{
		var soloRecords = recordManager.GetRecords(beatmapKey);
		var allRecords = soloRecords.ToList();
		foreach (var provider in _providers)
		{
			allRecords.AddRange(provider.GetRecords(beatmapKey));
		}

		// find records with MaxMultipliedScore and NotesCount populated, fill other records based on that
		// (fill allRecords, before grouping, to ensure that all instances of SoloRecord get the values)
		var scoredRecord = allRecords.FirstOrDefault(record => record.MaxMultipliedScore != -1);
		if (scoredRecord != null)
			allRecords.ForEach(record => record.FillMaxScore(scoredRecord.MaxMultipliedScore));
		var countedRecord = allRecords.FirstOrDefault(record => record.NotesCount != -1);
		if (countedRecord != null)
			allRecords.ForEach(record => record.FillNotesCount(countedRecord.NotesCount));

		return allRecords
			.GroupBy(
				keySelector: record => record,
				resultSelector: (_, records) => SoloRecord.MergeAll(records.ToList()),
				comparer: new SoloRecord.Comparer()
			)
			.Where(record => record.ModifiedScore != -1)
			.ToList();
	}
}
