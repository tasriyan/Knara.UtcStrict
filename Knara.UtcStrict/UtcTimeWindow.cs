using System;
using System.Collections.Generic;
using System.Linq;

namespace Knara.UtcStrict
{
	/// <summary>
	/// Represents a time-of-day availability window with timezone awareness and day-of-week restrictions.
	/// </summary>
	/// <remarks>
	/// This class handles common business scenarios for service availability, such as:
	/// - Business hours in specific timezones
	/// - Maintenance windows with day restrictions  
	/// - Feature availability during certain times
	/// - Multi-timezone service scheduling
	/// 
	/// All time evaluations are performed in UTC and converted to the specified timezone
	/// for accurate time-of-day calculations regardless of server location.
	/// </remarks>
	/// <example>
	/// <code>
	/// // Business hours in Eastern timezone, weekdays only
	/// var businessHours = new UtcTimeWindow(
	///     new TimeSpan(9, 0, 0),    // 9 AM
	///     new TimeSpan(17, 0, 0),   // 5 PM  
	///     "Eastern Standard Time",
	///     new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, 
	///            DayOfWeek.Thursday, DayOfWeek.Friday }
	/// );
	/// 
	/// var (isOpen, reason) = businessHours.IsActiveAt(UtcDateTime.UtcNow);
	/// </code>
	/// </example>
	public class UtcTimeWindow
	{
		/// <summary>
		/// Gets the start time of the availability window.
		/// </summary>
		/// <value>A TimeSpan representing the time of day when the window opens.</value>
		public TimeSpan StartOn { get; }

		/// <summary>
		/// Gets the stop time of the availability window.
		/// </summary>
		/// <value>A TimeSpan representing the time of day when the window closes.</value>
		/// <remarks>
		/// For overnight windows (e.g., 22:00 - 06:00), this value will be less than StartOn.
		/// </remarks>
		public TimeSpan StopOn { get; }

		/// <summary>
		/// Gets the timezone identifier used for time-of-day calculations.
		/// </summary>
		/// <value>A timezone identifier string (e.g., "UTC", "Eastern Standard Time", "America/New_York").</value>
		public string TimeZone { get; }

		/// <summary>
		/// Gets the array of days when this window is active.
		/// </summary>
		/// <value>An array of DayOfWeek values representing the active days.</value>
		public DayOfWeek[] DaysActive { get; }

		/// <summary>
		/// Gets a time window that represents 24/7 availability with no restrictions.
		/// </summary>
		/// <value>
		/// A UtcTimeWindow covering 00:00:00 to 23:59:59 on all days in UTC timezone.
		/// </value>
		/// <remarks>
		/// This window has no restrictions and will always return false for <see cref="HasWindow"/>.
		/// </remarks>
		public static UtcTimeWindow AlwaysOpen => new(
			startOn: TimeSpan.Zero,
			stopOn: new TimeSpan(23, 59, 59));

		/// <summary>
		/// Initializes a new instance of UtcTimeWindow with the specified parameters.
		/// </summary>
		/// <param name="startOn">The time of day when the window opens (00:00:00 to 23:59:59).</param>
		/// <param name="stopOn">The time of day when the window closes (00:00:00 to 23:59:59).</param>
		/// <param name="timeZone">
		/// The timezone identifier for time calculations. Defaults to "UTC".
		/// Accepts both Windows ("Eastern Standard Time") and IANA ("America/New_York") formats.
		/// </param>
		/// <param name="daysActive">
		/// The days of the week when this window is active. If null or empty, defaults to all days.
		/// </param>
		/// <exception cref="ArgumentException">
		/// Thrown when:
		/// - startOn or stopOn are outside the valid 24-hour range
		/// - timeZone is not a valid timezone identifier
		/// - daysActive contains invalid DayOfWeek values
		/// </exception>
		/// <example>
		/// <code>
		/// // Overnight maintenance window (10 PM to 6 AM, all days)
		/// var maintenance = new UtcTimeWindow(
		///     new TimeSpan(22, 0, 0),   // 10 PM
		///     new TimeSpan(6, 0, 0),    // 6 AM
		///     "UTC"
		/// );
		/// 
		/// // Weekend-only service (all day Saturday and Sunday)
		/// var weekend = new UtcTimeWindow(
		///     TimeSpan.Zero,
		///     new TimeSpan(23, 59, 59),
		///     "UTC",
		///     new[] { DayOfWeek.Saturday, DayOfWeek.Sunday }
		/// );
		/// </code>
		/// </example>
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

		/// <summary>
		/// Determines whether this window has meaningful time or day restrictions.
		/// </summary>
		/// <returns>
		/// True if the window has restrictions (limited time range or limited days); 
		/// False if it represents unrestricted availability (24/7 on all days).
		/// </returns>
		/// <remarks>
		/// Returns false only when the window covers the full time range (00:00:00 to 23:59:59)
		/// AND includes all seven days of the week, indicating no restrictions.
		/// </remarks>
		public bool HasWindow()
		{
			// No window restrictions only if it covers full time range AND all days
			bool isFullTimeRange = StartOn == TimeSpan.Zero // start at midnight
				&& StopOn == new TimeSpan(23, 59, 59);		// end at one second before midnight
			bool isAllDays = DaysActive.Length == 7; // All 7 days of the week

			return !(isFullTimeRange && isAllDays);
		}

		/// <summary>
		/// Determines whether the window is active at the specified UTC time.
		/// </summary>
		/// <param name="time">The UTC time to evaluate against this window.</param>
		/// <returns>
		/// A tuple containing:
		/// - bool: True if the window is active at the specified time; otherwise, false.
		/// - string: A descriptive reason for the active/inactive state.
		/// </returns>
		/// <remarks>
		/// The evaluation process:
		/// 1. Converts the UTC time to the window's timezone
		/// 2. Checks if the resulting day is in the DaysActive array
		/// 3. Checks if the resulting time-of-day falls within StartOn and StopOn
		/// 
		/// Handles overnight windows correctly (e.g., StartOn=22:00, StopOn=06:00).
		/// </remarks>
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
