using System.Collections.Generic;

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
}
