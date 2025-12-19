using System.Collections.Generic;
using SoloParty.Data.Models;

namespace SoloParty.Data;

public interface ISoloRecordProvider
{
	public string ProviderName { get; }
	public List<SoloRecord> GetRecords(BeatmapKey beatmapKey);
}
