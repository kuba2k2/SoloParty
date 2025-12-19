using System;
using System.Collections.Generic;
using System.Linq;
using SoloParty.Data;
using SoloParty.Data.Models;

namespace SoloParty.Utils;

public static class ModifierExtensions
{
	private static readonly List<Tuple<Modifier, string>> Modifiers =
	[
		new(Modifier.Multiplayer, "MULTI"),
		new(Modifier.ZenMode, "ZEN"),
		new(Modifier.BatteryEnergy, "BE"),
		new(Modifier.NoFail, "NF"),
		new(Modifier.InstaFail, "IF"),
		new(Modifier.NoObstacles, "NO"),
		new(Modifier.NoBombs, "NB"),
		new(Modifier.FastNotes, "FN"),
		new(Modifier.StrictAngles, "SA"),
		new(Modifier.DisappearingArrows, "DA"),
		new(Modifier.SuperFastSong, "SFS"),
		new(Modifier.FasterSong, "FS"),
		new(Modifier.SlowerSong, "SS"),
		new(Modifier.NoArrows, "NA"),
		new(Modifier.GhostNotes, "GN"),
		new(Modifier.SmallCubes, "SN"),
		new(Modifier.ProMode, "PRO")
	];

	public static Modifier ToSoloModifier(this GameplayModifiers modifiers)
	{
		return (Modifier)(int)modifiers.ToGameplayModifierMask();
	}

	public static string ToModifierString(this Modifier modifier)
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
}
