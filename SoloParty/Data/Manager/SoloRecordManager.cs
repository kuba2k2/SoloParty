using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IPA.Utilities;
using Newtonsoft.Json;
using SiraUtil.Logging;
using SoloParty.Data.Models;
using SoloParty.Utils;
using Zenject;

namespace SoloParty.Data.Manager;

public class SoloRecordManager(
	SiraLog log
) : AbstractRecordProvider, IInitializable, IDisposable
{
	public override string ProviderName => "SoloParty";

	private readonly string _dataFilePath = Path.Combine(UnityGame.UserDataPath, "SoloPartyData.json");
	private readonly string _backupFilePath = Path.Combine(UnityGame.UserDataPath, "SoloPartyData.json.bak");
	private ConcurrentDictionary<string, IList<SoloRecord>> _records = new();
	private bool _recordsModified;

	public void Initialize()
	{
		if (!LoadRecords(_dataFilePath) || _records.IsEmpty)
		{
			log.Warn("No records loaded, trying to restore from backup");
			if (LoadRecords(_backupFilePath) && !_records.IsEmpty)
				log.Notice("Backup file restore successful");
			_recordsModified = true;
		}
		else
		{
			log.Info("Records file loaded, creating a backup");
			BackupRecords(_backupFilePath);
			_recordsModified = false;
		}

		log.Info(
			$"Initialized with {_records.Select(pair => pair.Value.Count).Sum()} " +
			$"records from {_records.Count} levels"
		);
	}

	public void Dispose()
	{
		SaveRecords();
	}

	private bool LoadRecords(string filePath)
	{
		log.Info($"Loading records from {filePath}");
		_records = new ConcurrentDictionary<string, IList<SoloRecord>>();
		if (!File.Exists(filePath))
		{
			log.Warn($"File {filePath} does not exist");
			return false;
		}

		try
		{
			var text = File.ReadAllText(filePath);
			var deserialized = JsonConvert.DeserializeObject<ConcurrentDictionary<string, IList<SoloRecord>>>(text);
			_records = deserialized ?? throw new ArgumentNullException(nameof(deserialized));
			return true;
		}
		catch (Exception e)
		{
			log.Error($"Unable to load {filePath}");
			log.Error(e);
			return false;
		}
	}

	public void SaveRecords(bool force = false)
	{
		if (!_recordsModified && !force)
			return;

		log.Info($"Saving records to {_dataFilePath}");
		try
		{
			var serialized = JsonConvert.SerializeObject(_records, Formatting.Indented);
			File.WriteAllText(_dataFilePath, serialized);
			_recordsModified = false;
		}
		catch (Exception e)
		{
			log.Error($"Unable to save {_dataFilePath}");
			log.Error(e);
		}
	}

	private void BackupRecords(string backupPath)
	{
		if (!File.Exists(_dataFilePath))
			return;
		try
		{
			File.Copy(_dataFilePath, backupPath, true);
		}
		catch (Exception e)
		{
			log.Error($"Unable to create backup {_dataFilePath} -> {backupPath}");
			log.Error(e);
		}
	}

	public void CreateBackup(string backupName)
	{
		var dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
		var backupPath = Path.Combine(UnityGame.UserDataPath, $"SoloPartyData_{dateTime}_{backupName}.json.bak");
		BackupRecords(backupPath);
	}

	public void AddRecord(BeatmapKey beatmapKey, SoloRecord record)
	{
		AddRecord(beatmapKey.ToBeatmapKeyString(), record);
		// notify listeners
		InvokeRecordsUpdated(beatmapKey);
	}

	public void AddRecord(string beatmapKey, SoloRecord record)
	{
		var records = _records.GetOrAdd(beatmapKey, new List<SoloRecord>());
		// clear IsLatest flag of all previous records
		foreach (var r in records)
		{
			r.IsLatest = false;
		}

		// add the new record
		records.Add(record);
		_recordsModified = true;
	}

	public override List<SoloRecord> GetRecords(BeatmapKey beatmapKey) =>
		GetRecords(beatmapKey.ToBeatmapKeyString());

	public List<SoloRecord> GetRecords(string beatmapKey)
	{
		return _records.TryGetValue(beatmapKey, out var records)
			? records.ToList()
			: [];
	}
}
