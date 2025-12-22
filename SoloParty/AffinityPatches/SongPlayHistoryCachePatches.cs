using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using SiraUtil.Logging;
using SoloParty.Data.External;
using SongPlayHistory.SongPlayData;
using Zenject;

namespace SoloParty.AffinityPatches;

internal sealed class SongPlayHistoryCachePatches(
	Harmony harmony,
	PluginConfig config,
	SiraLog log,
	SongPlayHistoryRecordProvider recordProvider
) : IInitializable, IDisposable
{
	private static SongPlayHistoryCachePatches? _instance;
	private readonly SiraLog _log = log;
	private readonly SongPlayHistoryRecordProvider _recordProvider = recordProvider;
	private MethodInfo? _moveNextMethod;

	public void Initialize()
	{
		_instance = this;
		Patch();
	}

	public void Dispose()
	{
		Unpatch();
		_instance = null;
	}

	public void Patch()
	{
		// unpatch if not enabled in config
		if (!config.ModEnabled)
		{
			Unpatch();
			return;
		}

		// ScoringCacheManager class is internal, use reflection to find the GetRecordsText() method
		var sphui = typeof(ISongPlayRecord).Assembly.GetType("SongPlayHistory.SongPlayData.ScoringCacheManager");
		var loadScoringInfo = sphui.GetRuntimeMethods().First(m => m.Name == "LoadScoringInfo");

		// LoadScoringInfo() is async, so extract the state machine implementation
		var stateMachine = loadScoringInfo.GetCustomAttribute<AsyncStateMachineAttribute>();
		_moveNextMethod =
			stateMachine.StateMachineType.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance);

		if (_moveNextMethod == null)
		{
			_log.Error("SongPlayHistory LoadScoringInfo() MoveNext() method not found");
			return;
		}

		if (harmony.GetPatchedMethods().Contains(_moveNextMethod))
		{
			_log.Warn("SongPlayHistory ScoringCacheManager already patched");
			return;
		}

		_log.Info("Patching SongPlayHistory ScoringCacheManager");
		harmony.Patch(
			original: _moveNextMethod,
			transpiler: new HarmonyMethod(typeof(SongPlayHistoryCachePatches), "LoadScoringInfoTranspiler")
		);
	}

	public void Unpatch()
	{
		if (_moveNextMethod == null)
			return;
		_log.Info("Unpatching SongPlayHistory ScoringCacheManager");
		harmony.Unpatch(_moveNextMethod, HarmonyPatchType.Transpiler);
		_moveNextMethod = null;
	}

	private static IEnumerable<CodeInstruction> LoadScoringInfoTranspiler(IEnumerable<CodeInstruction> instructions)
	{
		CodeInstruction? ldargThis = null;
		CodeInstruction? ldfldBeatmapKey = null;
		CodeInstruction? callCompute = null;
		CodeInstruction? stlocScore = null;
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
			else if (insn.Contains("ComputeMaxMultipliedScoreForBeatmap"))
				callCompute = instruction;
			else if (callCompute != null && instruction.IsStloc() && insn.Contains("Int32"))
				stlocScore = instruction;

			// check if all instructions were found
			// inject right after ScoreModel.ComputeMaxMultipliedScoreForBeatmap() call
			if (ldargThis == null || ldfldBeatmapKey == null || callCompute == null || stlocScore == null)
				continue;

			_instance?._log.Info("Inserting UpdateBeatmapMaxScore() call");
			yield return new CodeInstruction(OpCodes.Dup); // duplicate 'beatmapData'
			yield return ldargThis; // load 'this'
			yield return ldfldBeatmapKey; // load 'this.beatmapKey'
			yield return stlocScore.Clone(OpCodes.Ldloc_S); // load 'fullMaxScore'
			// call UpdateBeatmapMaxScore(beatmapData, this.beatmapKey, fullMaxScore)
			yield return CodeInstruction.Call(() => UpdateBeatmapMaxScore(null!, default, 0));

			// patch only once
			patched = true;
		}
	}

	private static void UpdateBeatmapMaxScore(
		IReadonlyBeatmapData beatmapData,
		BeatmapKey beatmapKey,
		int fullMaxScore
	)
	{
		// insert scoring info into SongPlayHistoryRecordProvider's cache
		_instance?._recordProvider.ScoringCache[beatmapKey] =
			new Tuple<int, int>(beatmapData.cuttableNotesCount, fullMaxScore);
	}
}
