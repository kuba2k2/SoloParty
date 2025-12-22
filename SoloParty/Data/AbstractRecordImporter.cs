using System.Collections.Generic;
using SoloParty.Data.Models;

namespace SoloParty.Data;

public abstract class AbstractRecordImporter : AbstractRecordProvider
{
	public abstract Dictionary<string, List<SoloRecord>> GetAllRecords();
}
