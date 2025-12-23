using System;
using System.Collections.Generic;
using System.Linq;
using SoloParty.Data.Models;
using SongPlayHistory.Model;

namespace SoloParty.Utils;

public static class ModifierExtensions
{
	private static readonly List<Tuple<Modifier, string, string, float>> Modifiers =
	[
		new(Modifier.Multiplayer, "MULTI", "Multiplayer", 0),
		new(Modifier.ZenMode, "ZEN", "Zen Mode", -1.00f),
		new(Modifier.BatteryEnergy, "BE", "4 Lives", 0),
		new(Modifier.NoFail, "NF", "No Fail", -0.50f),
		new(Modifier.InstaFail, "IF", "1 Life", 0),
		new(Modifier.NoObstacles, "NO", "No Walls", -0.05f),
		new(Modifier.NoBombs, "NB", "No Bombs", -0.10f),
		new(Modifier.FastNotes, "FN", "Fast Notes", 0),
		new(Modifier.StrictAngles, "SA", "Strict Angles", 0),
		new(Modifier.DisappearingArrows, "DA", "Disappearing Arrows", 0.07f),
		new(Modifier.SuperFastSong, "SFS", "Super Fast Song", 0.10f),
		new(Modifier.FasterSong, "FS", "Faster Song", 0.08f),
		new(Modifier.SlowerSong, "SS", "Slower Song", -0.30f),
		new(Modifier.NoArrows, "NA", "No Arrows", -0.30f),
		new(Modifier.GhostNotes, "GN", "Ghost Notes", 0.11f),
		new(Modifier.SmallCubes, "SN", "Small Notes", 0),
		new(Modifier.ProMode, "PRO", "Pro Mode", 0)
	];

	public static Modifier ToSoloModifier(this GameplayModifiers modifiers)
	{
		return (Modifier)(int)modifiers.ToGameplayModifierMask();
	}

	public static Modifier ToSoloModifier(this SongPlayParam modifiers)
	{
		var modifier = Modifier.None;
		if (modifiers.HasFlag(SongPlayParam.BatteryEnergy)) modifier |= Modifier.BatteryEnergy;
		if (modifiers.HasFlag(SongPlayParam.NoFail)) modifier |= Modifier.NoFail;
		if (modifiers.HasFlag(SongPlayParam.InstaFail)) modifier |= Modifier.InstaFail;
		if (modifiers.HasFlag(SongPlayParam.NoObstacles)) modifier |= Modifier.NoObstacles;
		if (modifiers.HasFlag(SongPlayParam.NoBombs)) modifier |= Modifier.NoBombs;
		if (modifiers.HasFlag(SongPlayParam.FastNotes)) modifier |= Modifier.FastNotes;
		if (modifiers.HasFlag(SongPlayParam.StrictAngles)) modifier |= Modifier.StrictAngles;
		if (modifiers.HasFlag(SongPlayParam.DisappearingArrows)) modifier |= Modifier.DisappearingArrows;
		if (modifiers.HasFlag(SongPlayParam.FasterSong)) modifier |= Modifier.FasterSong;
		if (modifiers.HasFlag(SongPlayParam.SlowerSong)) modifier |= Modifier.SlowerSong;
		if (modifiers.HasFlag(SongPlayParam.NoArrows)) modifier |= Modifier.NoArrows;
		if (modifiers.HasFlag(SongPlayParam.GhostNotes)) modifier |= Modifier.GhostNotes;
		if (modifiers.HasFlag(SongPlayParam.SuperFastSong)) modifier |= Modifier.SuperFastSong;
		if (modifiers.HasFlag(SongPlayParam.SmallCubes)) modifier |= Modifier.SmallCubes;
		if (modifiers.HasFlag(SongPlayParam.ProMode)) modifier |= Modifier.ProMode;
		if (modifiers.HasFlag(SongPlayParam.Multiplayer)) modifier |= Modifier.Multiplayer;
		return modifier;
	}

	public static Modifier ToSoloModifier(this List<string> modifiers)
	{
		return Modifiers
			.Where(tuple => modifiers.Contains(tuple.Item2))
			.Aggregate(Modifier.None, (modifier, tuple) => modifier | tuple.Item1);
	}

	extension(Modifier modifier)
	{
		public string ToModifierString(bool withNoFail = true)
		{
			if (modifier == Modifier.None)
				return "";

			var modifiers = Modifiers
				.Where(tuple => modifier.HasFlag(tuple.Item1))
				.Where(tuple => tuple.Item1 != Modifier.NoFail || withNoFail)
				.Select(tuple => tuple.Item2)
				.ToList();

			if (modifiers.Count > 4)
				return string.Join(",", modifiers.Take(3)) + "...";
			return string.Join(",", modifiers);
		}

		public string ToModifierDescription()
		{
			if (modifier == Modifier.None)
				return "No Modifiers";

			var modifiers = Modifiers
				.Where(tuple => modifier.HasFlag(tuple.Item1))
				.Select(tuple => tuple.Item3)
				.ToList();
			return string.Join(", ", modifiers);
		}

		public float GetTotalMultiplier(bool softFailed)
		{
			if (modifier == Modifier.None)
				return 1.0f;

			return 1.0f + Modifiers
				.Where(tuple => modifier.HasFlag(tuple.Item1))
				.Where(tuple => tuple.Item1 != Modifier.NoFail || softFailed)
				.Sum(tuple => tuple.Item4);
		}
	}
}
