using System;

namespace Knara.UtcStrict
{
	public static class DateTimeUtilities
	{
		public static DateTime NormalizeToUtc(DateTime? dateTime, DateTime utcReplacementDt)
		{
			if (!dateTime.HasValue)
				return utcReplacementDt;

			if (dateTime.Value == DateTime.MinValue)
				return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

			if (dateTime.Value == DateTime.MinValue.ToUniversalTime())
				return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

			if (dateTime.Value == DateTime.MaxValue)
				return DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);

			if (dateTime.Value == DateTime.MaxValue.ToUniversalTime())
				return DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);

			if (dateTime.Value.Kind == DateTimeKind.Utc)
			{
				return dateTime.Value;
			}

			//return dateTime.Value.ToUniversalTime();
			return DateTime.SpecifyKind(dateTime.Value.ToUniversalTime(), DateTimeKind.Utc);
		}

		public static DateTime NormalizeToUtc(DateTime? dateTime, string sourceTimeZone, DateTime utcReplacementDt)
		{
			if (!dateTime.HasValue)
				return utcReplacementDt;

			if (dateTime.Value == DateTime.MinValue)
				return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

			if (dateTime.Value == DateTime.MinValue.ToUniversalTime())
				return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

			if (dateTime.Value == DateTime.MaxValue)
				return DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);

			if (dateTime.Value == DateTime.MaxValue.ToUniversalTime())
				return DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);

			if (dateTime.Value.Kind == DateTimeKind.Utc)
			{
				return dateTime.Value;
			}

			if (dateTime.Value.Kind == DateTimeKind.Local)
				return dateTime.Value.ToUniversalTime();

			// For Unspecified, use the provided timezone
			var sourceTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(sourceTimeZone);
			var withSourceTz = DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Unspecified);
			return TimeZoneInfo.ConvertTimeToUtc(withSourceTz, sourceTimeZoneInfo);
		}
	}
}
