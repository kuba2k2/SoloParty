using System.Collections.Generic;
using System.Linq;
using SoloParty.Data.Models;
using Zenject;

namespace SoloParty.Data.Manager;

public class ExternalRecordManager(
	SoloRecordManager recordManager
) : AbstractRecordProvider
{
	public override string ProviderName => "External";

	private readonly IList<AbstractRecordProvider> _providers = [];

	[Inject] private readonly PluginConfig _config = null!;

	public void Register(AbstractRecordProvider provider)
	{
		if (_providers.Contains(provider))
			return;
		_providers.Add(provider);
	}

	public void Unregister(AbstractRecordProvider provider)
	{
		if (!_providers.Contains(provider))
			return;
		_providers.Remove(provider);
	}

	public AbstractRecordProvider? GetByName(string providerName)
	{
		return _providers.FirstOrDefault(provider => provider.ProviderName == providerName);
	}

	public override List<SoloRecord> GetRecords(BeatmapKey beatmapKey)
	{
		var soloRecords = recordManager.GetRecords(beatmapKey);
		var allRecords = soloRecords.ToList();
		foreach (var provider in _providers)
		{
			switch (provider.ProviderName)
			{
				case "PartyLeaderboard" when !_config.LeaderboardPartyModeRecords:
				case "SongPlayHistory" when !_config.LeaderboardSongPlayHistoryRecords:
					continue;
				default:
					allRecords.AddRange(provider.GetRecords(beatmapKey));
					break;
			}
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
