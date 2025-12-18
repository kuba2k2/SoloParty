using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SoloParty.Utils;

namespace SoloParty.Data;

[JsonObject(MemberSerialization.OptIn)]
public class SoloRecord : IComparable<SoloRecord>
{
	[JsonProperty("Date")] public long Date { get; internal set; }
	[JsonProperty("ModifiedScore")] public int ModifiedScore { get; internal set; } = -1;
	[JsonProperty("MultipliedScore")] public int MultipliedScore { get; internal set; } = -1;
	[JsonProperty("MaxModifiedScore")] public int MaxModifiedScore { get; internal set; } = -1;
	[JsonProperty("MaxMultipliedScore")] public int MaxMultipliedScore { get; internal set; } = -1;
	[JsonProperty("FullCombo")] public bool FullCombo { get; internal set; }
	[JsonProperty("GoodCutsCount")] public int GoodCutsCount { get; internal set; } = -1;
	[JsonProperty("BadCutsCount")] public int BadCutsCount { get; internal set; } = -1;
	[JsonProperty("MissedCount")] public int MissedCount { get; internal set; } = -1;
	[JsonProperty("MaxCombo")] public int MaxCombo { get; internal set; } = -1;
	[JsonProperty("NotesLeft")] public int NotesLeft { get; internal set; } = -1;
	[JsonProperty("SoftFailed")] public bool SoftFailed { get; internal set; }
	[JsonProperty("Modifiers")] public Modifier Modifiers { get; internal set; } = Modifier.None;
	[JsonProperty("PlayerName")] public string? PlayerName { get; internal set; }

	public override string ToString()
	{
		return $"PlayerRecord {{ " +
		       $"Date = {Date}, " +
		       $"ModifiedScore = {ModifiedScore}, " +
		       $"MultipliedScore = {MultipliedScore}, " +
		       $"MaxModifiedScore = {MaxModifiedScore}, " +
		       $"MaxMultipliedScore = {MaxMultipliedScore}, " +
		       $"FullCombo = {FullCombo}, " +
		       $"GoodCutsCount = {GoodCutsCount}, " +
		       $"BadCutsCount = {BadCutsCount}, " +
		       $"MissedCount = {MissedCount}, " +
		       $"MaxCombo = {MaxCombo}, " +
		       $"NotesLeft = {NotesLeft}, " +
		       $"SoftFailed = {SoftFailed}, " +
		       $"Modifiers = {Modifiers.ToModifierString()}, " +
		       $"PlayerName = {PlayerName} " +
		       $"}}";
	}

	public int CompareTo(SoloRecord other)
	{
		return ModifiedScore.CompareTo(other.ModifiedScore);
	}

	public bool Matches(SoloRecord other)
	{
		return Math.Abs(Date - other.Date) < 10000 && ModifiedScore == other.ModifiedScore;
	}

	public void MergeFrom(SoloRecord other)
	{
		if (!Matches(other))
			throw new ArgumentException("Records don't match");
		if (Date % 1000 == 0)
			Date = other.Date;
		if (MultipliedScore == -1 && other.MultipliedScore != -1)
			MultipliedScore = other.MultipliedScore;
		if (MaxModifiedScore == -1 && other.MaxModifiedScore != -1)
			MaxModifiedScore = other.MaxModifiedScore;
		if (MaxMultipliedScore == -1 && other.MaxMultipliedScore != -1)
			MaxMultipliedScore = other.MaxMultipliedScore;
		if (other.FullCombo)
			FullCombo = other.FullCombo;
		if (GoodCutsCount == -1 && other.GoodCutsCount != -1)
			GoodCutsCount = other.GoodCutsCount;
		if (BadCutsCount == -1 && other.BadCutsCount != -1)
			BadCutsCount = other.BadCutsCount;
		if (MissedCount == -1 && other.MissedCount != -1)
			MissedCount = other.MissedCount;
		if (MaxCombo == -1 && other.MaxCombo != -1)
			MaxCombo = other.MaxCombo;
		if (NotesLeft == -1 && other.NotesLeft != -1)
			NotesLeft = other.NotesLeft;
		if (other.SoftFailed)
			SoftFailed = other.SoftFailed;
		if (Modifiers == Modifier.None && other.Modifiers != Modifier.None)
			Modifiers = other.Modifiers;
		if (PlayerName == null && other.PlayerName != null)
			PlayerName = other.PlayerName;
	}

	public static SoloRecord MergeAll(List<SoloRecord> records)
	{
		if (records.Count == 0)
			throw new IndexOutOfRangeException("Records list is empty");
		var mergedRecord = records[0];
		records
			.GetRange(1, records.Count - 1)
			.ForEach(record => mergedRecord.MergeFrom(record));
		return mergedRecord;
	}

	public sealed class Comparer : IEqualityComparer<SoloRecord>
	{
		public bool Equals(SoloRecord? x, SoloRecord? y)
		{
			if (ReferenceEquals(x, y)) return true;
			if (x is null) return false;
			if (y is null) return false;
			return x.GetType() == y.GetType() && x.Matches(y);
		}

		public int GetHashCode(SoloRecord obj)
		{
			return HashCode.Combine(obj.Date / 10000, obj.ModifiedScore);
		}
	}
}
