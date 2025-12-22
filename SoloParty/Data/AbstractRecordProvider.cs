using System.Collections.Generic;
using SoloParty.Data.Models;

namespace SoloParty.Data;

public abstract class AbstractRecordProvider
{
	public abstract string ProviderName { get; }
	public abstract List<SoloRecord> GetRecords(BeatmapKey beatmapKey);
}
