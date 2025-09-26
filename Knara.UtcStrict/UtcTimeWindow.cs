﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Knara.UtcStrict
{
	public class UtcTimeWindow
	{
		public TimeSpan StartOn { get; }
		public TimeSpan StopOn { get; }
		public string TimeZone { get; }
		public DayOfWeek[] DaysActive { get; }

		public static UtcTimeWindow AlwaysOpen => new(
			startOn: TimeSpan.Zero,
			stopOn: new TimeSpan(23, 59, 59));

		public UtcTimeWindow(
					TimeSpan startOn,
					TimeSpan stopOn,
					string timeZone = "UTC",
					DayOfWeek[]? daysActive = null)
		{
			if (startOn < TimeSpan.Zero || startOn >= TimeSpan.FromDays(1))
			{
				throw new ArgumentException("Start time must be between 00:00:00 and 23:59:59.", nameof(startOn));
			}

			// Validate end time
			if (stopOn < TimeSpan.Zero || stopOn >= TimeSpan.FromDays(1))
			{
				throw new ArgumentException("End time must be between 00:00:00 and 23:59:59.", nameof(stopOn));
			}

			// Note: We allow start time >= end time for overnight windows (e.g., 22:00 - 06:00)

			// Validate and normalize timezone
			var validatedTimeZone = ValidateAndNormalizeTimeZone(timeZone);

			// Validate allowed days
			var validatedDays = ValidateAllowedDays(daysActive);

			StartOn = startOn;
			StopOn = stopOn;
			TimeZone = validatedTimeZone;
			DaysActive = validatedDays;
		}

		public bool HasWindow()
		{
			// No window restrictions only if it covers full time range AND all days
			bool isFullTimeRange = StartOn == TimeSpan.Zero // start at midnight
				&& StopOn == new TimeSpan(23, 59, 59);		// end at one second before midnight
			bool isAllDays = DaysActive.Length == 7; // All 7 days of the week

			return !(isFullTimeRange && isAllDays);
		}

		public (bool, string) IsActiveAt(UtcDateTime time)
		{
			var (localTime, localDay) = TimezoneUtilities.GetTimeAndDayInTimezone(time, TimeZone);

			// Check if current day is in allowed days
			if (!IsAllowedDay(localDay))
			{
				return (false, "Outside allowed days");
			}

			// Check if current time is within window
			if (!IsWithinTimeRange(localTime))
			{
				return (false, "Outside time window");
			}

			return (true, "Within time window");

		}

		private bool IsAllowedDay(DayOfWeek dayOfWeek)
		{
			return DaysActive.Contains(dayOfWeek);
		}

		private bool IsWithinTimeRange(TimeSpan currentTime)
		{
			if (StartOn <= StopOn)
			{
				// Same day window (e.g., 9:00 - 17:00)
				return currentTime >= StartOn && currentTime <= StopOn;
			}
			else
			{
				// Overnight window (e.g., 22:00 - 06:00)
				return currentTime >= StartOn || currentTime <= StopOn;
			}
		}

		private static string ValidateAndNormalizeTimeZone(string? timeZone)
		{
			if (string.IsNullOrWhiteSpace(timeZone))
			{
				return "UTC";
			}

			try
			{
				// Validate that the timezone exists
				TimeZoneInfo.FindSystemTimeZoneById(timeZone);
				return timeZone!;
			}
			catch (TimeZoneNotFoundException)
			{
				throw new ArgumentException($"Invalid timezone identifier: {timeZone}", nameof(timeZone));
			}
			catch (InvalidTimeZoneException)
			{
				throw new ArgumentException($"Invalid timezone identifier: {timeZone}", nameof(timeZone));
			}
		}

		private static DayOfWeek[] ValidateAllowedDays(IEnumerable<DayOfWeek>? allowedDays)
		{
			if (allowedDays == null || !allowedDays.Any())
			{
				// Default to all days if none specified
				return [DayOfWeek.Monday,
				DayOfWeek.Tuesday,
				DayOfWeek.Wednesday,
				DayOfWeek.Thursday,
				DayOfWeek.Friday,
				DayOfWeek.Saturday,
				DayOfWeek.Sunday];
			}

			// Remove duplicates and validate enum values
			var distinctDays = allowedDays.Distinct().ToArray();
			foreach (var day in distinctDays)
			{
				if (!Enum.IsDefined(typeof(DayOfWeek), day))
				{
					throw new ArgumentException($"Invalid day of week: {day}", nameof(allowedDays));
				}
			}

			return distinctDays;
		}

		public static bool operator ==(UtcTimeWindow? left, UtcTimeWindow? right)
		{
			if (ReferenceEquals(left, right))
				return true;

			if (left is null || right is null)
				return false;

			return left.StartOn == right.StartOn &&
				   left.StopOn == right.StopOn &&
				   string.Equals(left.TimeZone, right.TimeZone, StringComparison.OrdinalIgnoreCase) &&
				   left.DaysActive.OrderBy(d => d).SequenceEqual(right.DaysActive.OrderBy(d => d));
		}

		public static bool operator !=(UtcTimeWindow? left, UtcTimeWindow? right)
		{
			return !(left == right);
		}

		public override bool Equals(object? obj)
		{
			return obj is UtcTimeWindow other && this == other;
		}

		public override int GetHashCode()
		{
			var hash = new HashCode();
			hash.Add(StartOn);
			hash.Add(StopOn);
			hash.Add(TimeZone, StringComparer.OrdinalIgnoreCase);

			// Add sorted days to ensure consistent hash regardless of order
			foreach (var day in DaysActive.OrderBy(d => d))
			{
				hash.Add(day);
			}

			return hash.ToHashCode();
		}
	}
}
