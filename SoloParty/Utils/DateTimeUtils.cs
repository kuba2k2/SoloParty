using System;

namespace SoloParty.Utils;

public static class DateTimeUtils
{
	public static long ToUnixTimeMilliseconds(this DateTime dateTime)
	{
		return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();
	}

	public static DateTime ToLocalDateTime(this long unixTime)
	{
		return DateTimeOffset.FromUnixTimeMilliseconds(unixTime).LocalDateTime;
	}

	public static string FormatTimeAgo(this TimeSpan span)
	{
		if (span.TotalMinutes < 1)
			return "now";
		if (span.TotalHours < 1)
			return $"{span.Minutes} minute{(span.Minutes > 1 ? "s" : "")} ago";
		var months = span.Days / 30;
		var years = span.Days / 365;
		return span.TotalDays switch
		{
			< 1 => $"{span.Hours} hour{(span.Hours > 1 ? "s" : "")} ago",
			< 30 => $"{span.Days} day{(span.Days > 1 ? "s" : "")} ago",
			< 365 => $"{months} month{(months > 1 ? "s" : "")} ago",
			_ => $"{years} year{(years > 1 ? "s" : "")} ago"
		};
	}
}
