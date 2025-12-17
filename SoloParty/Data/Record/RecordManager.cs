using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IPA.Utilities;
using Newtonsoft.Json;
using SiraUtil.Logging;
using Zenject;

namespace SoloParty.Data.Record;

public class RecordManager(
	SiraLog log
) : IInitializable, IDisposable
{
	private readonly string _dataFilePath = Path.Combine(UnityGame.UserDataPath, "SoloPartyData.json");
	private readonly string _backupFilePath = Path.Combine(UnityGame.UserDataPath, "SoloPartyData.json.bak");
	private ConcurrentDictionary<string, IList<PlayerRecord>> _records = new();
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
			BackupRecords(_dataFilePath, _backupFilePath);
			_recordsModified = false;
		}

		log.Info(
			$"Initialized with {_records.Select(pair => pair.Value.Count).Sum()} " +
			$"records from {_records.Count} levels"
		);
	}

	public void Dispose()
	{
		SaveRecords(_dataFilePath);
	}

	private bool LoadRecords(string filePath)
	{
		log.Info($"Loading records from {filePath}");
		_records = new ConcurrentDictionary<string, IList<PlayerRecord>>();
		if (!File.Exists(filePath))
		{
			log.Warn($"File {filePath} does not exist");
			return false;
		}

		try
		{
			var text = File.ReadAllText(filePath);
			var deserialized = JsonConvert.DeserializeObject<ConcurrentDictionary<string, IList<PlayerRecord>>>(text);
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

	private void SaveRecords(string filePath)
	{
		if (!_recordsModified)
			return;

		log.Info($"Saving records to {filePath}");
		try
		{
			var serialized = JsonConvert.SerializeObject(_records, Formatting.Indented);
			File.WriteAllText(filePath, serialized);
			_recordsModified = false;
		}
		catch (Exception e)
		{
			log.Error($"Unable to save {filePath}");
			log.Error(e);
		}
	}

	private void BackupRecords(string sourcePath, string backupPath)
	{
		if (!File.Exists(sourcePath))
			return;
		try
		{
			File.Copy(sourcePath, backupPath, true);
		}
		catch (Exception e)
		{
			log.Error($"Unable to create backup {sourcePath} -> {backupPath}");
			log.Error(e);
		}
	}

	public void AddRecord(string beatmapKey, PlayerRecord record)
	{
		_records.GetOrAdd(beatmapKey, new List<PlayerRecord>()).Add(record);
		_recordsModified = true;
		SaveRecords(_dataFilePath);
	}

	public IList<PlayerRecord> GetRecords(string beatmapKey)
	{
		return _records.TryGetValue(beatmapKey, out var records)
			? records.ToList()
			: [];
	}

	public PlayerRecord? GetRecordPlayerBest(string beatmapKey, string playerName)
	{
		PlayerRecord? best = null;
		foreach (var record in GetRecords(beatmapKey))
		{
			if (record.PlayerName != playerName)
				continue;
			if (best == null || record.ModifiedScore > best.ModifiedScore)
				best = record;
		}

		return best;
	}

	public PlayerRecord? GetRecordMatching(string beatmapKey, long date, int modifiedScore)
	{
		return GetRecords(beatmapKey)
			.Where(record => record.ModifiedScore == modifiedScore)
			.FirstOrDefault(record => Math.Abs(record.Date - date) <= 10000);
	}
}
