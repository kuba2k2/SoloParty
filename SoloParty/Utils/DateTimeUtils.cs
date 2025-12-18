using System;

namespace SoloParty.Utils;

public static class DateTimeUtils
{
	public static long ToUnixTimeMilliseconds(this DateTime dateTime)
	{
		return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();
	}
}
