using System;
using System.Collections.Generic;
using System.Linq;
using SoloParty.Data.Models;
using SongPlayHistory.Model;

namespace SoloParty.Utils;

public static class ModifierExtensions
{
	private static readonly List<Tuple<Modifier, string, string>> Modifiers =
	[
		new(Modifier.Multiplayer, "MULTI", "Multiplayer"),
		new(Modifier.ZenMode, "ZEN", "Zen Mode"),
		new(Modifier.BatteryEnergy, "BE", "4 Lives"),
		new(Modifier.NoFail, "NF", "No Fail"),
		new(Modifier.InstaFail, "IF", "1 Life"),
		new(Modifier.NoObstacles, "NO", "No Walls"),
		new(Modifier.NoBombs, "NB", "No Bombs"),
		new(Modifier.FastNotes, "FN", "Fast Notes"),
		new(Modifier.StrictAngles, "SA", "Strict Angles"),
		new(Modifier.DisappearingArrows, "DA", "Disappearing Arrows"),
		new(Modifier.SuperFastSong, "SFS", "Super Fast Song"),
		new(Modifier.FasterSong, "FS", "Faster Song"),
		new(Modifier.SlowerSong, "SS", "Slower Song"),
		new(Modifier.NoArrows, "NA", "No Arrows"),
		new(Modifier.GhostNotes, "GN", "Ghost Notes"),
		new(Modifier.SmallCubes, "SN", "Small Notes"),
		new(Modifier.ProMode, "PRO", "Pro Mode")
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

	extension(Modifier modifier)
	{
		public string ToModifierString()
		{
			if (modifier == Modifier.None)
				return "";

			var modifiers = Modifiers
				.Where(tuple => modifier.HasFlag(tuple.Item1))
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
	}
}
