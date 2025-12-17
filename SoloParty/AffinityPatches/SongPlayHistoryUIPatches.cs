using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using HarmonyLib;
using IPA.Loader;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using SoloParty.Data.Record;
using SoloParty.Utils;
using SongPlayHistory.SongPlayData;
using Zenject;

namespace SoloParty.AffinityPatches;

internal sealed class SongPlayHistoryUIPatches(
	Harmony harmony,
	PluginConfig config,
	SiraLog log,
	RecordManager recordManager
) : IInitializable, IDisposable
{
	private static SongPlayHistoryUIPatches? _instance;
	private readonly SiraLog _log = log;
	private readonly RecordManager _recordManager = recordManager;
	private MethodInfo? _moveNextMethod;

	public void Initialize()
	{
		_instance = this;
		Patch();
		config.LeaderboardSongPlayHistoryChanged += Patch;
	}

	public void Dispose()
	{
		config.LeaderboardSongPlayHistoryChanged -= Patch;
		Unpatch();
		_instance = null;
	}

	public void Patch()
	{
		// unpatch if not enabled in config
		if (!config.ModEnabled || !config.LeaderboardSongPlayHistory)
		{
			Unpatch();
			return;
		}

		// SPHUI class is internal, use reflection to find the GetRecordsText() method
		var sphui = typeof(ISongPlayRecord).Assembly.GetType("SongPlayHistory.UI.SPHUI");
		var getRecordsText = sphui.GetRuntimeMethods().First(m => m.Name == "GetRecordsText");

		// GetRecordsText() is async, so extract the state machine implementation
		var stateMachine = getRecordsText.GetCustomAttribute<AsyncStateMachineAttribute>();
		_moveNextMethod =
			stateMachine.StateMachineType.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance);

		if (_moveNextMethod == null)
		{
			_log.Error("SongPlayHistory GetRecordsText() MoveNext() method not found");
			return;
		}

		if (harmony.GetPatchedMethods().Contains(_moveNextMethod))
		{
			_log.Warn("SongPlayHistory UI already patched");
			return;
		}

		_log.Info("Patching SongPlayHistory UI");
		harmony.Patch(
			original: _moveNextMethod,
			transpiler: new HarmonyMethod(typeof(SongPlayHistoryUIPatches), "GetRecordsTextTranspiler")
		);
	}

	public void Unpatch()
	{
		if (_moveNextMethod == null)
			return;
		_log.Info("Unpatching SongPlayHistory UI");
		harmony.Unpatch(_moveNextMethod, HarmonyPatchType.Transpiler);
		_moveNextMethod = null;
	}

	private static IEnumerable<CodeInstruction> GetRecordsTextTranspiler(IEnumerable<CodeInstruction> instructions)
	{
		CodeInstruction? ldargThis = null;
		CodeInstruction? ldfldBeatmapKey = null;
		CodeInstruction? ldlocBuilder = null;
		CodeInstruction? ldlocRecord = null;
		var patched = false;

		foreach (var instruction in instructions)
		{
			// do not change any existing instructions
			yield return instruction;
			if (patched)
				continue;

			// find instructions that will be reused
			var insn = instruction.ToString();
			if (instruction.IsLdarg(0))
				ldargThis = instruction;
			else if (insn.Contains("ldfld BeatmapKey"))
				ldfldBeatmapKey = instruction;
			else if (instruction.IsLdloc() && insn.Contains("System.Text.StringBuilder"))
				ldlocBuilder = instruction;
			else if (instruction.IsLdloc() && insn.Contains("SongPlayHistory.SongPlayData.ISongPlayRecord"))
				ldlocRecord = instruction;

			// called right before appending score date
			if (!insn.Contains("SongPlayHistory.Utils.Utils::TMPSpace"))
				continue;
			if (ldlocBuilder == null || ldargThis == null || ldfldBeatmapKey == null || ldlocRecord == null)
				continue;

			_instance?._log.Info("Inserting PrependPlayerName() call");
			yield return new CodeInstruction(OpCodes.Pop);
			yield return ldlocBuilder; // load 'builder'
			yield return ldargThis; // load 'this'
			yield return ldfldBeatmapKey; // load 'this.beatmapKey'
			yield return ldlocRecord; // load 'record'
			// call PrependPlayerName(builder, this.beatmapKey, record)
			yield return CodeInstruction.Call(() => PrependPlayerName(null!, default, null!));

			// patch only once
			patched = true;
		}
	}

	private static StringBuilder PrependPlayerName(
		StringBuilder builder,
		BeatmapKey beatmapKey,
		ISongPlayRecord songPlayRecord
	)
	{
		if (_instance == null)
			return builder;

		var beatmapKeyString = beatmapKey.ToBeatmapKeyString();
		// format parameters as used by PlayerRecord
		var date = new DateTimeOffset(songPlayRecord.LocalTime.ToUniversalTime()).ToUnixTimeMilliseconds();
		var modifiedScore = songPlayRecord.ModifiedScore;
		var record = _instance._recordManager.GetRecordMatching(beatmapKeyString, date, modifiedScore);
		// for logging
		var infoString = $"beatmapKey: {beatmapKeyString}, date: {date}, modifiedScore: {modifiedScore}";

		if (record != null)
		{
			_instance._log.Debug($"Inserting player name '{record.PlayerName}' for {infoString}");
			builder.Append($"<size=2.5><color=#1a252bff> {record.PlayerName} - </color></size>");
		}
		else
		{
			_instance._log.Warn($"Record not found for {infoString}");
		}

		return builder;
	}
}
