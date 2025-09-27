using System;

namespace Knara.UtcStrict
{
	/// <summary>
	/// Represents a schedule for enabling and disabling features or services within a specific UTC time range.
	/// </summary>
	/// <remarks>
	/// This class encapsulates the business logic for temporal feature control, such as:
	/// - Feature rollouts with scheduled start and end times
	/// - Temporary service availability windows  
	/// - Maintenance schedules with automatic enable/disable
	/// 
	/// All times are handled in UTC to prevent timezone-related scheduling bugs.
	/// </remarks>
	/// <example>
	/// <code>
	/// // Create a new feature schedule (must be in the future)
	/// var schedule = UtcSchedule.CreateSchedule(
	///     UtcDateTime.UtcNow.AddHours(1),
	///     UtcDateTime.UtcNow.AddDays(7)
	/// );
	/// 
	/// // Load existing schedule from database (any dates allowed)
	/// var existing = new UtcSchedule(enableDate, disableDate);
	/// 
	/// // Check if feature should be active now
	/// var (isActive, reason) = schedule.IsActiveAt(UtcDateTime.UtcNow);
	/// </code>
	/// </example>
	public class UtcSchedule
	{
		/// <summary>
		/// Gets the UTC date and time when the schedule becomes active.
		/// </summary>
		/// <value>The enable time in UTC timezone.</value>
		public UtcDateTime EnableOn { get; }

		/// <summary>
		/// Gets the UTC date and time when the schedule becomes inactive.
		/// </summary>
		/// <value>The disable time in UTC timezone.</value>
		public UtcDateTime DisableOn { get; }

		/// <summary>
		/// Initializes a new instance of UtcSchedule with the specified enable and disable times.
		/// </summary>
		/// <param name="enableOn">The UTC time when the schedule becomes active.</param>
		/// <param name="disableOn">The UTC time when the schedule becomes inactive.</param>
		/// <remarks>
		/// This constructor is typically used when loading schedules from a database where 
		/// historical dates are valid. Use <see cref="CreateSchedule"/> for creating new schedules
		/// with future validation.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Thrown when disableOn is less than or equal to enableOn.
		/// </exception>
		/// <example>
		/// <code>
		/// // Load schedule from database (can be historical dates)
		/// var schedule = new UtcSchedule(
		///     new UtcDateTime(DateTime.Parse("2024-01-15T10:00:00Z")),
		///     new UtcDateTime(DateTime.Parse("2024-01-15T18:00:00Z"))
		/// );
		/// </code>
		/// </example>
		public UtcSchedule(UtcDateTime enableOn, UtcDateTime disableOn)
		{
			if (disableOn <= enableOn)
			{
				throw new ArgumentException("Scheduled disable date must be after the scheduled enable date.");
			}

			EnableOn = enableOn;
			DisableOn = disableOn;
		}

		/// <summary>
		/// Gets a schedule that represents no active scheduling (always inactive).
		/// </summary>
		/// <value>
		/// A UtcSchedule with EnableOn set to MinValue and DisableOn set to MaxValue,
		/// which will always return false for <see cref="HasSchedule"/> and <see cref="IsActiveAt"/>.
		/// </value>
		/// <remarks>
		/// Use this for features that should remain disabled until explicitly scheduled.
		/// </remarks>
		/// <example>
		/// <code>
		/// var feature = new Feature 
		/// { 
		///     Name = "NewFeature",
		///     Schedule = UtcSchedule.Unscheduled  // Feature disabled
		/// };
		/// </code>
		/// </example>
		public static UtcSchedule Unscheduled => new UtcSchedule(UtcDateTime.MinValue, UtcDateTime.MaxValue);

		/// <summary>
		/// Creates a new schedule with validation that the enable time is in the future.
		/// </summary>
		/// <param name="utcEnableOn">The UTC time when the schedule should become active.</param>
		/// <param name="utcDisableOn">The UTC time when the schedule should become inactive.</param>
		/// <returns>A new UtcSchedule instance with the specified times.</returns>
		/// <remarks>
		/// This method enforces business rules for new schedule creation by requiring the
		/// enable time to be in the future. Use the constructor for loading historical schedules.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Thrown when utcEnableOn is not in the future (less than or equal to current UTC time),
		/// or when utcDisableOn is not after utcEnableOn.
		/// </exception>
		/// <example>
		/// <code>
		/// // Create a schedule starting in 1 hour, ending in 7 days
		/// var schedule = UtcSchedule.CreateSchedule(
		///     UtcDateTime.UtcNow.AddHours(1),
		///     UtcDateTime.UtcNow.AddDays(7)
		/// );
		/// </code>
		/// </example>
		public static UtcSchedule CreateSchedule(UtcDateTime utcEnableOn, UtcDateTime utcDisableOn)
		{
			if (utcEnableOn <= UtcDateTime.UtcNow)
			{
				throw new ArgumentException("Scheduled enable date must be in the future.");
			}

			return new UtcSchedule(utcEnableOn, utcDisableOn);
		}

		/// <summary>
		/// Determines whether this schedule has meaningful enable/disable times or represents an unscheduled state.
		/// </summary>
		/// <returns>
		/// True if the schedule has a meaningful time range (not unscheduled); otherwise, false.
		/// </returns>
		/// <remarks>
		/// Returns false for <see cref="Unscheduled"/> instances and true for any schedule with
		/// actual enable/disable boundaries. This method checks if either EnableOn is greater than
		/// MinValue OR DisableOn is less than MaxValue.
		/// </remarks>
		public bool HasSchedule()
		{
			return EnableOn > UtcDateTime.MinValue || DisableOn < UtcDateTime.MaxValue;
		}

		/// <summary>
		/// Determines whether the schedule is active at the specified UTC time.
		/// </summary>
		/// <param name="time">The UTC time to check against the schedule.</param>
		/// <returns>
		/// A tuple containing:
		/// - bool: True if the schedule is active at the specified time; otherwise, false.
		/// - string: A descriptive reason for the active/inactive state.
		/// </returns>
		/// <remarks>
		/// The schedule is considered active when:
		/// 1. The schedule has meaningful boundaries (HasSchedule returns true)
		/// 2. The specified time is greater than or equal to EnableOn
		/// 3. The specified time is less than DisableOn
		/// </remarks>
		public (bool, string) IsActiveAt(UtcDateTime time)
		{
			if (!HasSchedule())
			{
				return (false, "No active schedule set");
			}

			if (time < EnableOn)
			{
				return (false, "Scheduled enable date not reached");
			}

			if (time >= DisableOn)
			{
				return (false, "Scheduled disable date passed");
			}

			return (true, "Schedule is active");
		}

		public static bool operator ==(UtcSchedule? left, UtcSchedule? right)
		{
			if (left is null && right is null)
				return true;
			if (left is null || right is null)
				return false;

			return left.EnableOn == right.EnableOn
				&& left.DisableOn == right.DisableOn;
		}

		public static bool operator !=(UtcSchedule? left, UtcSchedule? right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			return obj is UtcSchedule other && this == other;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(EnableOn, DisableOn);
		}
	}
}
