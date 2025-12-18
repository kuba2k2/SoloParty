using System;
using Newtonsoft.Json;

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
	[JsonProperty("PlayerName")] public string? PlayerName { get; internal set; }

	public int CompareTo(SoloRecord other)
	{
		return ModifiedScore.CompareTo(other.ModifiedScore);
	}

	public override string ToString()
	{
		return $"PlayerRecord {{ " +
		       $"Date = {Date}, " +
		       $"MultipliedScore = {MultipliedScore}, " +
		       $"ModifiedScore = {ModifiedScore}, " +
		       $"MaxMultipliedScore = {MaxMultipliedScore}, " +
		       $"MaxModifiedScore = {MaxModifiedScore}, " +
		       $"FullCombo = {FullCombo}, " +
		       $"GoodCutsCount = {GoodCutsCount}, " +
		       $"BadCutsCount = {BadCutsCount}, " +
		       $"MissedCount = {MissedCount}, " +
		       $"MaxCombo = {MaxCombo}, " +
		       $"PlayerName = {PlayerName} " +
		       $"}}";
	}
}
