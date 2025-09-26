using System;

namespace Knara.UtcStrict
{
	public class UtcSchedule
	{
		public UtcDateTime EnableOn { get; }
		public UtcDateTime DisableOn { get; }

		// load from the database in a valid state
		public UtcSchedule(UtcDateTime enableOn, UtcDateTime disableOn)
		{
			if (disableOn <= enableOn)
			{
				throw new ArgumentException("Scheduled disable date must be after the scheduled enable date.");
			}

			EnableOn = enableOn;
			DisableOn = disableOn;
		}

		public static UtcSchedule Unscheduled => new UtcSchedule(UtcDateTime.MinValue, UtcDateTime.MaxValue);

		// This method is used to create a new schedule in a valid state
		public static UtcSchedule CreateSchedule(UtcDateTime utcEnableOn, UtcDateTime utcDisableOn)
		{
			if (utcEnableOn <= UtcDateTime.UtcNow)
			{
				throw new ArgumentException("Scheduled enable date must be in the future.");
			}

			return new UtcSchedule(utcEnableOn, utcDisableOn);
		}

		public bool HasSchedule()
		{
			return EnableOn > UtcDateTime.MinValue || DisableOn < UtcDateTime.MaxValue;
		}

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
