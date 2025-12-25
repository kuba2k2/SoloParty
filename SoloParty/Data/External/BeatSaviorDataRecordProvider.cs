using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using SiraUtil.Logging;
using SoloParty.Data.Manager;
using SoloParty.Data.Models;
using SoloParty.Utils;
using Zenject;

namespace SoloParty.Data.External;

internal class BeatSaviorDataRecordProvider(
	ExternalImportManager importManager,
	SiraLog log
) : AbstractRecordImporter, IInitializable, IDisposable
{
	public override string ProviderName => "BeatSaviorData";
	public override bool IsDatePrecise => false;

	private readonly List<string> _difficultyList = ["easy", "normal", "hard", "expert", "expertplus"];
	private const string FileNameFormat = "yyyy-MM-dd";

	private readonly string _dataPath =
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Beat Savior Data\");

	public void Initialize()
	{
		importManager.Register(this);
	}

	public void Dispose()
	{
		importManager.Unregister(this);
	}

	public override List<SoloRecord> GetRecords(BeatmapKey beatmapKey) => [];

	public override Dictionary<string, List<SoloRecord>> GetAllRecords()
	{
		var result = new Dictionary<string, List<SoloRecord>>();
		if (!Directory.Exists(_dataPath))
			return result;

		foreach (var filePath in Directory.EnumerateFiles(_dataPath, "20*.bsd"))
		{
			var fileName = Path.GetFileNameWithoutExtension(filePath);
			var parsed = DateTime.TryParseExact(
				fileName,
				FileNameFormat,
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out var date
			);
			if (!parsed)
				continue;

			log.Info($"Reading BeatSaviorData file: {filePath}");
			int loadCount = 0, skipCount = 0, failCount = 0;
			foreach (var (line, index) in File.ReadLines(filePath).Select((s, i) => (s, i)))
			{
				if (index == 0)
					continue;
				try
				{
					if (ParseLine(date, index, line, result))
						loadCount++;
					else
						skipCount++;
				}
				catch (Exception e)
				{
					log.Warn($"Cannot parse JSON line: {e}");
					failCount++;
				}
			}

			log.Info($"- loaded {loadCount} records, skipped {skipCount}, failed {failCount}");
		}

		return result;
	}

	private bool ParseLine(DateTime date, int index, string line, Dictionary<string, List<SoloRecord>> result)
	{
		var json = JObject.Parse(line);
		var songDataType = (int)(json["songDataType"] ?? -1);
		if (songDataType != 1)
			return false;

		var songId = (string?)json["songID"];
		var songDifficulty = (string?)json["songDifficulty"];
		var gameMode = (string?)json["gameMode"];
		var trackers = (JObject?)json["trackers"];
		if (songId == null || songDifficulty == null || trackers == null || !_difficultyList.Contains(songDifficulty))
			return false;

		var winTracker = (JObject)trackers["winTracker"]!;
		var won = (bool)winTracker["won"]!;
		if (!won)
			// ignore failed records (though BSD doesn't save these for some reason)
			return false;

		var scoreTracker = (JObject)trackers["scoreTracker"]!;
		var rawScore = (int)scoreTracker["rawScore"]!;
		var score = (int)scoreTracker["score"]!;
		if (score == 0 || rawScore == 0)
			// ignore records with no score at all
			return false;

		var distanceTracker = (JObject)trackers["distanceTracker"]!;
		var leftSaber = (float)distanceTracker["leftSaber"]!;
		var rightSaber = (float)distanceTracker["rightSaber"]!;

		var hitTracker = (JObject)trackers["hitTracker"]!;
		var leftNoteHit = (int)hitTracker["leftNoteHit"]!;
		var rightNoteHit = (int)hitTracker["rightNoteHit"]!;
		var leftMiss = (int)hitTracker["leftMiss"]!;
		var rightMiss = (int)hitTracker["rightMiss"]!;
		var leftBadCuts = (int)hitTracker["leftBadCuts"]!;
		var rightBadCuts = (int)hitTracker["rightBadCuts"]!;
		var maxCombo = (int)hitTracker["maxCombo"]!;
		var leftTotal = leftNoteHit + leftMiss + leftBadCuts;
		var rightTotal = rightNoteHit + rightMiss + rightBadCuts;
		if ((rightTotal != 0 && leftTotal == 0 && leftSaber == 0f) ||
		    (leftTotal != 0 && rightTotal == 0 && rightSaber == 0f))
			// certain old versions don't save 'gameMode' at all :(
			gameMode = "OneSaber";

		var modifiers = ((JArray)scoreTracker["modifiers"]!)
			.Select(m => (string)m!)
			.ToList()
			.ToSoloModifier();

		var endState = EndState.Cleared;
		if (maxCombo == leftNoteHit + rightNoteHit && (leftMiss + rightMiss + leftBadCuts + rightBadCuts) == 0)
			// maxCombo is all notes, and there's no missed - assume full combo
			// (though the total note count is sometimes wrong in BSD)
			endState = EndState.FullCombo;
		else if (modifiers.HasFlag(Modifier.NoFail) && score - 2 <= rawScore / 2)
			// modified score less than half the multiplied score - assume soft-failed
			endState = EndState.SoftFailed;

		if (songId.Length == 40)
			songId = $"custom_level_{songId}";
		gameMode ??= "Standard";

		var beatmapKey = $"{songId}___{_difficultyList.IndexOf(songDifficulty)}___{gameMode}";
		var record = new SoloRecord
		{
			Date = date.ToUnixTimeMilliseconds() + index,
			ModifiedScore = score,
			MultipliedScore = rawScore,
			GoodCutsCount = leftNoteHit + rightNoteHit,
			BadCutsCount = leftBadCuts + rightBadCuts,
			MissedCount = leftMiss + rightMiss,
			MaxCombo = maxCombo,
			EndState = endState,
			Modifiers = modifiers,
			IsExternal = true
		};

		if (!result.TryGetValue(beatmapKey, out var records))
			result[beatmapKey] = records = [];
		records.Add(record);
		return true;
	}
}
