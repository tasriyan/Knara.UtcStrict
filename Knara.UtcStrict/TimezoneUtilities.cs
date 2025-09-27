using System;

namespace Knara.UtcStrict
{
	public static class TimezoneUtilities
	{
		/// <summary>
		/// Converts a UTC time to the specified timezone and returns the local time-of-day and day-of-week.
		/// </summary>
		/// <param name="time">The UtcDateTime to convert (must be in UTC).</param>
		/// <param name="timezone">
		/// The target timezone identifier for conversion.
		/// Accepts both Windows timezone IDs (e.g., "Eastern Standard Time") and 
		/// IANA timezone IDs (e.g., "America/New_York").
		/// </param>
		/// <returns>
		/// A tuple containing:
		/// - TimeSpan: The time-of-day in the target timezone (00:00:00 to 23:59:59.9999999)
		/// - DayOfWeek: The day of the week in the target timezone
		/// </returns>
		/// <remarks>
		/// This method handles:
		/// - Daylight saving time transitions
		/// - Different timezone formats (Windows and IANA)
		/// - UTC timezone as a special case for performance
		/// - Date changes across timezone boundaries
		/// 
		/// The conversion accounts for the timezone's current offset at the specified time,
		/// including automatic daylight saving time adjustments.
		/// </remarks>
		/// <exception cref="TimeZoneNotFoundException">
		/// Thrown when the timezone identifier is not found on the system.
		/// </exception>
		/// <exception cref="InvalidTimeZoneException">
		/// Thrown when the timezone represents corrupted timezone data.
		/// </exception>
		/// <example>
		/// <code>
		/// var utcTime = new UtcDateTime(new DateTime(2024, 6, 15, 18, 30, 0, DateTimeKind.Utc));
		/// 
		/// // Convert to Eastern time
		/// var (easternTime, easternDay) = TimezoneUtilities.GetTimeAndDayInTimezone(
		///     utcTime, 
		///     "Eastern Standard Time"
		/// );
		/// // Result: easternTime = 14:30:00 (2:30 PM), easternDay = Saturday (assuming EDT)
		/// 
		/// // Convert to Pacific time
		/// var (pacificTime, pacificDay) = TimezoneUtilities.GetTimeAndDayInTimezone(
		///     utcTime, 
		///     "America/Los_Angeles"
		/// );
		/// // Result: pacificTime = 11:30:00 (11:30 AM), pacificDay = Saturday (assuming PDT)
		/// 
		/// // UTC timezone (optimized path)
		/// var (utcTimeOfDay, utcDayOfWeek) = TimezoneUtilities.GetTimeAndDayInTimezone(
		///     utcTime, 
		///     "UTC"
		/// );
		/// // Result: utcTimeOfDay = 18:30:00 (6:30 PM), utcDayOfWeek = Saturday
		/// </code>
		/// </example>
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
