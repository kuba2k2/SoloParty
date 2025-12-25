using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SoloParty.Utils;
using UnityEngine;

namespace SoloParty.Data.Models;

[JsonObject(MemberSerialization.OptIn)]
public class SoloRecord : IComparable<SoloRecord>
{
	[JsonProperty("Date")] public long Date { get; internal set; }
	[JsonProperty("ModifiedScore")] public int ModifiedScore { get; internal set; } = -1;
	[JsonProperty("MultipliedScore")] public int MultipliedScore { get; internal set; } = -1;
	[JsonProperty("MaxModifiedScore")] public int MaxModifiedScore { get; internal set; } = -1;
	[JsonProperty("MaxMultipliedScore")] public int MaxMultipliedScore { get; internal set; } = -1;
	[JsonProperty("GoodCutsCount")] public int GoodCutsCount { get; internal set; } = -1;
	[JsonProperty("BadCutsCount")] public int BadCutsCount { get; internal set; } = -1;
	[JsonProperty("MissedCount")] public int MissedCount { get; internal set; } = -1;
	[JsonProperty("MaxCombo")] public int MaxCombo { get; internal set; } = -1;
	[JsonProperty("NotesPassed")] public int NotesPassed { get; internal set; } = -1;
	[JsonProperty("NotesCount")] public int NotesCount { get; internal set; } = -1;
	[JsonProperty("EndState")] public EndState EndState { get; internal set; } = EndState.Unknown;
	[JsonProperty("Modifiers")] public Modifier Modifiers { get; internal set; } = Modifier.None;
	[JsonProperty("PlayerName")] public string? PlayerName { get; internal set; }

	public bool IsExternal;
	public bool IsLatest;

	public float Accuracy
	{
		get
		{
			if (MaxModifiedScore <= 0)
				return 0f;
			if (MaxMultipliedScore == 0)
				return 1f;
			return ModifiedScore / (float)Mathf.Max(MaxMultipliedScore, MaxModifiedScore);
		}
	}

	public RankModel.Rank Rank =>
		RankModel.GetRankForScore(MultipliedScore, ModifiedScore, MaxMultipliedScore, MaxModifiedScore);

	public override string ToString()
	{
		return $"PlayerRecord {{ " +
		       $"Date = {Date}, " +
		       $"ModifiedScore = {ModifiedScore}, " +
		       $"MultipliedScore = {MultipliedScore}, " +
		       $"MaxModifiedScore = {MaxModifiedScore}, " +
		       $"MaxMultipliedScore = {MaxMultipliedScore}, " +
		       $"GoodCutsCount = {GoodCutsCount}, " +
		       $"BadCutsCount = {BadCutsCount}, " +
		       $"MissedCount = {MissedCount}, " +
		       $"MaxCombo = {MaxCombo}, " +
		       $"NotesPassed = {NotesPassed}, " +
		       $"NotesCount = {NotesCount}, " +
		       $"EndState = {EndState}, " +
		       $"Modifiers = {Modifiers.ToModifierString()}, " +
		       $"PlayerName = {PlayerName}, " +
		       $"IsExternal = {IsExternal}, " +
		       $"IsLatest = {IsLatest} " +
		       $"}}";
	}

	public void FillMaxScore(int maxMultipliedScore)
	{
		// if the modified score is less than half the maximum multiplied score, NoFail was probably used
		// (this should only apply to PartyLeaderboard records anyway)
		if (ModifiedScore <= maxMultipliedScore / 2 && Modifiers == Modifier.None && IsExternal &&
		    EndState == EndState.Cleared)
		{
			Modifiers = Modifier.NoFail;
			EndState = EndState.SoftFailed;
		}

		if (MaxMultipliedScore == -1)
			MaxMultipliedScore = maxMultipliedScore;
		if (MaxModifiedScore != -1)
			return;
		var totalMultiplier = Modifiers.GetTotalMultiplier(softFailed: EndState == EndState.SoftFailed);
		MaxModifiedScore = Mathf.FloorToInt(MaxMultipliedScore * totalMultiplier);
	}

	public void FillNotesCount(int notesCount)
	{
		if (NotesCount != -1)
			return;
		NotesCount = notesCount;
		switch (EndState)
		{
			case EndState.FullCombo:
				NotesPassed = NotesCount;
				GoodCutsCount = NotesCount;
				MaxCombo = NotesCount;
				BadCutsCount = 0;
				MissedCount = 0;
				break;
			case EndState.Cleared:
				NotesPassed = NotesCount;
				break;
			case EndState.Unknown:
			case EndState.SoftFailed:
			case EndState.Failed:
			default:
				break;
		}
	}

	public int CompareTo(SoloRecord other)
	{
		return ModifiedScore.CompareTo(other.ModifiedScore);
	}

	public bool Matches(SoloRecord other)
	{
		if (Math.Abs(Date - other.Date) >= 10000)
			return false;
		if (ModifiedScore == -1)
			return true;
		if (other.ModifiedScore == -1)
			return true;
		return ModifiedScore == other.ModifiedScore;
	}

	public void MergeFrom(SoloRecord other, bool mustMatch = true, bool mergeDate = true)
	{
		if (mustMatch && !Matches(other))
			throw new ArgumentException("Records don't match");
		if (mergeDate && Date % 1000 == 0)
			Date = other.Date;
		if (ModifiedScore == -1 && other.ModifiedScore != -1)
			ModifiedScore = other.ModifiedScore;
		if (MultipliedScore == -1 && other.MultipliedScore != -1)
			MultipliedScore = other.MultipliedScore;
		if (MaxModifiedScore == -1 && other.MaxModifiedScore != -1)
			MaxModifiedScore = other.MaxModifiedScore;
		if (MaxMultipliedScore == -1 && other.MaxMultipliedScore != -1)
			MaxMultipliedScore = other.MaxMultipliedScore;
		if (GoodCutsCount == -1 && other.GoodCutsCount != -1)
			GoodCutsCount = other.GoodCutsCount;
		if (BadCutsCount == -1 && other.BadCutsCount != -1)
			BadCutsCount = other.BadCutsCount;
		if (MissedCount == -1 && other.MissedCount != -1)
			MissedCount = other.MissedCount;
		if (MaxCombo == -1 && other.MaxCombo != -1)
			MaxCombo = other.MaxCombo;
		if (NotesPassed == -1 && other.NotesPassed != -1)
			NotesPassed = other.NotesPassed;
		if (NotesCount == -1 && other.NotesCount != -1)
			NotesCount = other.NotesCount;
		if (EndState == EndState.Unknown && other.EndState != EndState.Unknown)
			EndState = other.EndState;
		if (Modifiers == Modifier.None && other.Modifiers != Modifier.None)
			Modifiers = other.Modifiers;
		if (PlayerName == null && other.PlayerName != null)
			PlayerName = other.PlayerName;
		if (!other.IsExternal)
			IsExternal = false;
		if (other.IsLatest)
			IsLatest = true;
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
			// in GroupBy() *both* Equals() and HashCode() have to match
			// since only the absolute difference of two Date fields counts, HashCode can't be used :(
			return 0;
		}
	}

	public override int GetHashCode()
	{
		var hashCode = new HashCode();
		// ReSharper disable NonReadonlyMemberInGetHashCode
		hashCode.Add(Date);
		hashCode.Add(ModifiedScore);
		hashCode.Add(MultipliedScore);
		hashCode.Add(MaxModifiedScore);
		hashCode.Add(MaxMultipliedScore);
		hashCode.Add(GoodCutsCount);
		hashCode.Add(BadCutsCount);
		hashCode.Add(MissedCount);
		hashCode.Add(MaxCombo);
		hashCode.Add(NotesPassed);
		hashCode.Add(NotesCount);
		hashCode.Add((int)EndState);
		hashCode.Add((int)Modifiers);
		hashCode.Add(PlayerName);
		// ReSharper restore NonReadonlyMemberInGetHashCode
		return hashCode.ToHashCode();
	}
}
