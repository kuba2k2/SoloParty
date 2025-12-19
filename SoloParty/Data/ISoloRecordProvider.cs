using System.Collections.Generic;
using SoloParty.Data.Models;

namespace SoloParty.Data;

public interface ISoloRecordProvider
{
	public string ProviderName { get; }
	public IList<SoloRecord> GetRecords(BeatmapKey beatmapKey);
}
