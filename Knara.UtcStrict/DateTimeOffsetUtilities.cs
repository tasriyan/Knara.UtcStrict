using System;

namespace Knara.UtcStrict
{
	public static class DateTimeOffsetUtilities
	{
		public static DateTime NormalizeToUtc(DateTimeOffset? dateTimeOffset, DateTime utcReplacementDt)
		{
			if (!dateTimeOffset.HasValue)
				return utcReplacementDt;

			if (dateTimeOffset.Value == DateTimeOffset.MinValue)
				return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

			if (dateTimeOffset.Value == DateTimeOffset.MaxValue)
				return DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);

			var dateTime = dateTimeOffset.Value.DateTime;
			if (dateTimeOffset.Value.Offset == TimeSpan.Zero)
			{
				dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
				return dateTime;
			}

			dateTime = dateTimeOffset.Value.UtcDateTime;
			dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
			return dateTime;
		}
	}
}