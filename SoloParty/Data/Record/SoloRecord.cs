using System;
using Newtonsoft.Json;

namespace SoloParty.Data.Record;

[JsonObject(MemberSerialization.OptIn)]
public class SoloRecord : IComparable<SoloRecord>
{
	[JsonProperty("Date")] public long Date { get; internal set; }
	[JsonProperty("ModifiedScore")] public int ModifiedScore { get; internal set; }
	[JsonProperty("PlayerName")] public string? PlayerName { get; internal set; }

	public int CompareTo(SoloRecord other)
	{
		return ModifiedScore.CompareTo(other.ModifiedScore);
	}

	public override string ToString()
	{
		return $"PlayerRecord{{ Date = {Date}; ModifiedScore = {ModifiedScore}; PlayerName = {PlayerName} }}";
	}
}
