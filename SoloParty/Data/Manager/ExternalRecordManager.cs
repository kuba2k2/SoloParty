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
		var allRecords = recordManager.GetRecords(beatmapKey);
		foreach (var provider in _providers)
		{
			allRecords.AddRange(provider.GetRecords(beatmapKey));
		}

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
