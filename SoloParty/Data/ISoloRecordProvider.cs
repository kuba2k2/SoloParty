using System.Collections.Generic;

namespace SoloParty.Data;

public interface ISoloRecordProvider
{
	public string ProviderName { get; }
	public IList<SoloRecord> GetRecords(BeatmapKey beatmapKey);
}
