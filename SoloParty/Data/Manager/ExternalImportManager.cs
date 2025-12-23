using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoloParty.Data.Manager;

public class ExternalImportManager
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
	}

	public Task<ImportResult> ImportRecords(AbstractRecordImporter importer, string? playerName) =>
		Task.Run(() => new ImportResult());
}
