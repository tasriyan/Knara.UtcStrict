using System;
using System.Text.Json.Serialization;

namespace Knara.UtcStrict
{
	[JsonConverter(typeof(UtcDateTimeOffsetJsonConverter))]
	public struct UtcDateTimeOffset
	{
		public DateTimeOffset DateTimeOffset { get; }

		public UtcDateTimeOffset(DateTime dateTime, string? sourceTimeZone = null)
		{
			if (sourceTimeZone == null)
			{
				var utcDateTime = DateTimeUtilities.NormalizeToUtc(dateTime, DateTime.UtcNow);
				DateTimeOffset = new DateTimeOffset(utcDateTime, TimeSpan.Zero);
			}
			else
			{
				var utcDateTime = DateTimeUtilities.NormalizeToUtc(dateTime, sourceTimeZone, DateTime.UtcNow);
				DateTimeOffset = new DateTimeOffset(utcDateTime, TimeSpan.Zero);
			}
		}

		public UtcDateTimeOffset(DateTimeOffset dateTimeOffset)
		{
			// Convert to UTC if not already
			DateTimeOffset = dateTimeOffset.ToUniversalTime();
		}

		public static UtcDateTime UtcNow => new UtcDateTime(DateTime.UtcNow);

		// MinValue and MaxValue are DateTime values with Kind=Utc
		public static readonly UtcDateTime MinValue = new UtcDateTime(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc));
		public static readonly UtcDateTime MaxValue = new UtcDateTime(DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc));

		// For backward compatibility - return the DateTime portion
		public DateTime DateTime => DateTimeOffset.DateTime;

		// Implicit convirsion to DateTime (UTC)
		public static implicit operator DateTime(UtcDateTimeOffset normalizedUtcDateTime) => normalizedUtcDateTime.DateTime;
		// Explicit conversion from DateTime (will be converted to UTC)
		public static explicit operator UtcDateTimeOffset(DateTime dateTime) => new UtcDateTimeOffset(dateTime);
		// Implicit conversion from DateTimeOffset (will be converted to UTC)
		public static implicit operator UtcDateTimeOffset(DateTimeOffset dateTimeOffset) => new UtcDateTimeOffset(dateTimeOffset);
		// Implicit conversion to DateTimeOffset (UTC)
		public static implicit operator DateTimeOffset(UtcDateTimeOffset normalizedUtcDateTime) => normalizedUtcDateTime.DateTimeOffset;

		public static bool operator ==(UtcDateTimeOffset left, UtcDateTimeOffset right) => left.DateTimeOffset == right.DateTimeOffset;
		public static bool operator !=(UtcDateTimeOffset left, UtcDateTimeOffset right) => left.DateTimeOffset != right.DateTimeOffset;
		public static bool operator <=(UtcDateTimeOffset left, UtcDateTimeOffset right) => left.DateTimeOffset <= right.DateTimeOffset;
		public static bool operator >=(UtcDateTimeOffset left, UtcDateTimeOffset right) => left.DateTimeOffset >= right.DateTimeOffset;
		public static bool operator <(UtcDateTimeOffset left, UtcDateTimeOffset right) => left.DateTimeOffset < right.DateTimeOffset;
		public static bool operator >(UtcDateTimeOffset left, UtcDateTimeOffset right) => left.DateTimeOffset > right.DateTimeOffset;

		public override bool Equals(object obj)
		{
			if (obj is UtcDateTimeOffset normalizedUtcDateTime)
			{
				return DateTimeOffset.Equals(normalizedUtcDateTime.DateTimeOffset);
			}
			return false;
		}

		public override int GetHashCode() => DateTimeOffset.GetHashCode();
	}

	public class UtcDateTimeOffsetJsonConverter : JsonConverter<UtcDateTimeOffset>
	{
		public override UtcDateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
			{
				var dateTimeString = reader.GetString();
				if (DateTimeOffset.TryParse(dateTimeString, out var dateTimeOffset))
				{
					return new UtcDateTimeOffset(dateTimeOffset);
				}

				if (DateTime.TryParse(dateTimeString, out var dateTime))
				{
					return new UtcDateTimeOffset(dateTime);
				}
			}

			throw new JsonException($"Unable to convert JSON value to UtcDateTime");
		}

		public override void Write(Utf8JsonWriter writer, UtcDateTimeOffset value, JsonSerializerOptions options)
		{
			// Write as ISO 8601 UTC format
			writer.WriteStringValue(value.DateTimeOffset.ToString("O"));
		}
	}
}