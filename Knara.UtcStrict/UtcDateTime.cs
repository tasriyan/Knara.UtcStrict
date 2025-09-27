using System;
using System.Text.Json.Serialization;

namespace Knara.UtcStrict
{
	/// <summary>
	/// A DateTime wrapper that enforces UTC timezone handling, preventing timezone-related bugs 
	/// by normalizing all input to UTC automatically.
	/// </summary>
	/// <remarks>
	/// This struct is designed to solve DateTime chaos in mixed environments where:
	/// - Database stores some timestamps as UTC, others as local time
	/// - APIs return DateTime vs DateTimeOffset inconsistently  
	/// - Frontend sends local or UTC time depending on implementation
	/// 
	/// All DateTime values are automatically normalized to UTC upon construction.
	/// </remarks>
	[JsonConverter(typeof(UtcDateTimeJsonConverter))]
	public struct UtcDateTime
	{
		/// <summary>
		/// Gets the normalized DateTime value in UTC timezone.
		/// </summary>
		/// <remarks>
		/// This DateTime will always have DateTimeKind.Utc regardless of the input source.
		/// </remarks>
		public DateTime DateTime { get; }

		/// <summary>
		/// Initializes a new instance of UtcDateTime from a DateTime value.
		/// </summary>
		/// <param name="dateTime">The DateTime to normalize to UTC.</param>
		/// <param name="sourceTimeZone">
		/// Optional timezone identifier for converting DateTimeKind.Unspecified values.
		/// If null, assumes server's local timezone for Kind.Local and Kind.Unspecified values.
		/// </param>
		/// <remarks>
		/// Conversion logic:
		/// - DateTimeKind.Utc: Preserved as-is
		/// - DateTimeKind.Local: Converted from system local time to UTC
		/// - DateTimeKind.Unspecified: Treated as sourceTimeZone (or local if null) and converted to UTC
		/// </remarks>
		/// <example>
		/// <code>
		/// // From local time
		/// var utc1 = new UtcDateTime(DateTime.Now);
		/// 
		/// // From specific timezone
		/// var easternTime = new DateTime(2024, 6, 15, 14, 30, 0);
		/// var utc2 = new UtcDateTime(easternTime, "Eastern Standard Time");
		/// 
		/// // Already UTC (preserved)
		/// var utc3 = new UtcDateTime(DateTime.UtcNow);
		/// </code>
		/// </example>
		/// <exception cref="TimeZoneNotFoundException">Thrown when sourceTimeZone is not a valid timezone identifier.</exception>
		public UtcDateTime(DateTime dateTime, string? sourceTimeZone = null)
		{
			DateTime = sourceTimeZone == null
				? DateTimeUtilities.NormalizeToUtc(dateTime, DateTime.UtcNow)
				: DateTimeUtilities.NormalizeToUtc(dateTime, sourceTimeZone, DateTime.UtcNow);
		}

		/// <summary>
		/// Initializes a new instance of UtcDateTime from a DateTimeOffset value.
		/// </summary>
		/// <param name="dateTimeOffset">The DateTimeOffset to convert to UTC.</param>
		/// <remarks>
		/// Extracts the UTC portion of the DateTimeOffset, discarding the original offset information.
		/// </remarks>
		/// <example>
		/// <code>
		/// var easternTime = new DateTimeOffset(2024, 6, 15, 10, 0, 0, TimeSpan.FromHours(-4));
		/// var utc = new UtcDateTime(easternTime); // Converts to UTC automatically
		/// </code>
		/// </example>
		public UtcDateTime(DateTimeOffset dateTimeOffset)
		{
			DateTime = DateTimeOffsetUtilities.NormalizeToUtc(dateTimeOffset, utcReplacementDt: DateTime.UtcNow);
		}

		public static UtcDateTime UtcNow => new UtcDateTime(DateTime.UtcNow);

		/// <summary>
		/// Represents the smallest possible value of UtcDateTime in UTC timezone.
		/// </summary>
		/// <value>January 1, 0001 12:00:00 midnight UTC.</value>
		public static readonly UtcDateTime MinValue = (UtcDateTime)DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

		/// <summary>
		/// Represents the largest possible value of UtcDateTime in UTC timezone.
		/// </summary>
		/// <value>December 31, 9999 11:59:59.9999999 PM UTC.</value>
		public static readonly UtcDateTime MaxValue = (UtcDateTime)DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);

		/// <summary>
		/// Implicitly converts a UtcDateTime to a DateTime in UTC timezone.
		/// </summary>
		/// <param name="normalizedUtcDateTime">The UtcDateTime to convert.</param>
		/// <returns>A DateTime with DateTimeKind.Utc.</returns>
		public static implicit operator DateTime(UtcDateTime normalizedUtcDateTime) => normalizedUtcDateTime.DateTime;

		/// <summary>
		/// Explicitly converts a DateTime to a UtcDateTime, normalizing it to UTC.
		/// </summary>
		/// <param name="dateTime">The DateTime to convert.</param>
		/// <returns>A UtcDateTime normalized to UTC.</returns>
		/// <remarks>
		/// Explicit conversion reminds developers that timezone conversion may occur.
		/// </remarks>
		public static explicit operator UtcDateTime(DateTime dateTime) => new UtcDateTime(dateTime);

		/// <summary>
		/// Implicitly converts a DateTimeOffset to a UtcDateTime, extracting the UTC portion.
		/// </summary>
		/// <param name="dateTimeOffset">The DateTimeOffset to convert.</param>
		/// <returns>A UtcDateTime representing the UTC equivalent.</returns>
		public static implicit operator UtcDateTime(DateTimeOffset dateTimeOffset) => new UtcDateTime(dateTimeOffset);

		/// <summary>
		/// Implicitly converts a UtcDateTime to a DateTimeOffset with zero UTC offset.
		/// </summary>
		/// <param name="normalizedUtcDateTime">The UtcDateTime to convert.</param>
		/// <returns>A DateTimeOffset with +00:00 offset.</returns>
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
