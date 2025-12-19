using System;
using System.Collections.Generic;
using System.Linq;
using SoloParty.Data.Models;
using SongPlayHistory.Model;

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
