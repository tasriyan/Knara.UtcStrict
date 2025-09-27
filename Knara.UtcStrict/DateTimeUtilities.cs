using System;

namespace Knara.UtcStrict
{
	public static class DateTimeUtilities
	{
		/// <summary>
		/// Normalizes a nullable DateTime to UTC using server timezone assumptions for unspecified kinds.
		/// </summary>
		/// <param name="dateTime">The DateTime to normalize. Can be null.</param>
		/// <param name="utcReplacementDt">The UTC DateTime to return if dateTime is null.</param>
		/// <returns>A DateTime in UTC timezone with DateTimeKind.Utc.</returns>
		/// <remarks>
		/// Conversion logic:
		/// - null: Returns utcReplacementDt
		/// - DateTimeKind.Utc: Returned unchanged
		/// - DateTimeKind.Local: Converted from system local time to UTC
		/// - DateTimeKind.Unspecified: Assumed to be local time and converted to UTC
		/// - MinValue/MaxValue: Handled safely without timezone conversion exceptions
		/// 
		/// This overload is suitable for legacy scenarios where the source timezone is unknown
		/// and server timezone assumptions are acceptable.
		/// </remarks>
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

		/// <summary>
		/// Normalizes a nullable DateTime to UTC using an explicit source timezone for unspecified kinds.
		/// </summary>
		/// <param name="dateTime">The DateTime to normalize. Can be null.</param>
		/// <param name="sourceTimeZone">
		/// The timezone identifier to use for DateTimeKind.Unspecified values.
		/// Accepts both Windows timezone IDs (e.g., "Eastern Standard Time") and 
		/// IANA timezone IDs (e.g., "America/New_York").
		/// </param>
		/// <param name="utcReplacementDt">The UTC DateTime to return if dateTime is null.</param>
		/// <returns>A DateTime in UTC timezone with DateTimeKind.Utc.</returns>
		/// <remarks>
		/// Conversion logic:
		/// - null: Returns utcReplacementDt
		/// - DateTimeKind.Utc: Returned unchanged
		/// - DateTimeKind.Local: Converted from system local time to UTC
		/// - DateTimeKind.Unspecified: Converted from sourceTimeZone to UTC
		/// - MinValue/MaxValue: Handled safely without timezone conversion exceptions
		/// 
		/// This overload provides explicit timezone control for scenarios where the source
		/// timezone is known (e.g., API inputs, user-specified timezones, external data).
		/// </remarks>
		/// <exception cref="TimeZoneNotFoundException">
		/// Thrown when sourceTimeZone is not a valid timezone identifier.
		/// </exception>
		/// <exception cref="InvalidTimeZoneException">
		/// Thrown when sourceTimeZone represents corrupted timezone data.
		/// </exception>
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

	public static class DateTimeOffsetUtilities
	{
		/// <summary>
		/// Normalizes a nullable DateTimeOffset to UTC DateTime by extracting the UTC equivalent.
		/// </summary>
		/// <param name="dateTimeOffset">The DateTimeOffset to normalize. Can be null.</param>
		/// <param name="utcReplacementDt">The UTC DateTime to return if dateTimeOffset is null.</param>
		/// <returns>A DateTime in UTC timezone with DateTimeKind.Utc.</returns>
		/// <remarks>
		/// Conversion logic:
		/// - null: Returns utcReplacementDt
		/// - Zero offset (UTC): DateTime portion is marked as DateTimeKind.Utc
		/// - Non-zero offset: UtcDateTime property is extracted and marked as DateTimeKind.Utc
		/// - MinValue/MaxValue: Handled safely without conversion exceptions
		/// 
		/// This method safely extracts the UTC equivalent from any DateTimeOffset, ensuring
		/// the resulting DateTime has the correct DateTimeKind.Utc marking.
		/// </remarks>
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
