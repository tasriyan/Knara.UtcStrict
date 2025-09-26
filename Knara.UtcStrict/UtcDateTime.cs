using System;

namespace Knara.UtcStrict
{
	public struct UtcDateTime
	{
		public DateTime DateTime { get; }

		public UtcDateTime(DateTime dateTime, string? sourceTimeZone = null)
		{
			DateTime = sourceTimeZone == null
				? DateTimeUtilities.NormalizeToUtc(dateTime, DateTime.UtcNow)
				: DateTimeUtilities.NormalizeToUtc(dateTime, sourceTimeZone, DateTime.UtcNow);
		}

		public UtcDateTime(DateTimeOffset dateTimeOffset)
		{
			DateTime = DateTimeOffsetUtilities.NormalizeToUtc(dateTimeOffset, utcReplacementDt: DateTime.UtcNow);
		}

		public static UtcDateTime UtcNow => new UtcDateTime(DateTime.UtcNow);
		public static readonly UtcDateTime MinValue = (UtcDateTime)DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
		public static readonly UtcDateTime MaxValue = (UtcDateTime)DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);

		public static implicit operator DateTime(UtcDateTime normalizedUtcDateTime) => normalizedUtcDateTime.DateTime;
		public static explicit operator UtcDateTime(DateTime dateTime) => new UtcDateTime(dateTime);

		public static implicit operator UtcDateTime(DateTimeOffset dateTimeOffset) => new UtcDateTime(dateTimeOffset);
		public static implicit operator DateTimeOffset(UtcDateTime normalizedUtcDateTime) => new DateTimeOffset(normalizedUtcDateTime.DateTime);

		public static bool operator ==(UtcDateTime left, UtcDateTime right) => left.DateTime == right.DateTime;
		public static bool operator !=(UtcDateTime left, UtcDateTime right) => left.DateTime != right.DateTime;
		public static bool operator <=(UtcDateTime left, UtcDateTime right) => left.DateTime <= right.DateTime;
		public static bool operator >=(UtcDateTime left, UtcDateTime right) => left.DateTime >= right.DateTime;
		public static bool operator <(UtcDateTime left, UtcDateTime right) => left.DateTime < right.DateTime;
		public static bool operator >(UtcDateTime left, UtcDateTime right) => left.DateTime > right.DateTime;

		public override bool Equals(object obj)
		{
			if (obj is UtcDateTime normalizedUtcDateTime)
			{
				return DateTime.Equals(normalizedUtcDateTime.DateTime);
			}
			return false;
		}

		public override int GetHashCode() => DateTime.GetHashCode();
	}
}
