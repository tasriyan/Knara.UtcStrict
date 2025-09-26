using System;

namespace Knara.UtcStrict
{
	public static class TimezoneUtilities
	{
		public static (TimeSpan, DayOfWeek) GetTimeAndDayInTimezone(UtcDateTime time, string timezone)
		{
			var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);

			if (timeZoneInfo.StandardName == "UTC" || timeZoneInfo.Id == "UTC")
			{
				// If the timezone is UTC, return the time and day directly
				return (time.DateTime.TimeOfDay, time.DateTime.DayOfWeek);
			}

			var localTime = TimeZoneInfo.ConvertTimeFromUtc(time, timeZoneInfo);
			var currentTime = localTime.TimeOfDay;
			var currentDay = localTime.DayOfWeek;

			return (currentTime, currentDay);
		}
	}
}
