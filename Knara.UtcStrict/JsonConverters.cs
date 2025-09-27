using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Knara.UtcStrict
{
	/// <summary>
	/// Provides JSON serialization and deserialization support for UtcDateTime using System.Text.Json.
	/// </summary>
	/// <remarks>
	/// This converter ensures that UtcDateTime values are properly serialized to and deserialized from JSON
	/// while maintaining UTC timezone consistency. It handles both DateTime and DateTimeOffset string formats
	/// during deserialization and always outputs ISO 8601 UTC format during serialization.
	/// 
	/// Supported JSON formats:
	/// - ISO 8601 with timezone: "2024-06-15T14:30:00-04:00"
	/// - ISO 8601 UTC: "2024-06-15T18:30:00Z" or "2024-06-15T18:30:00.0000000Z"
	/// - Local format: "2024-06-15T14:30:00" (interpreted based on UtcDateTime logic)
	/// 
	/// - Various standard DateTime string formats 
	/// The converter is automatically applied to UtcDateTime through the JsonConverter attribute,
	/// so no manual registration is required in most scenarios.
	/// </remarks>
	/// <example>
	/// <code>
	/// // Automatic usage with JsonSerializer
	/// var schedule = new UtcSchedule(
	///     new UtcDateTime(DateTime.UtcNow.AddHours(1)),
	///     new UtcDateTime(DateTime.UtcNow.AddDays(1))
	/// );
	/// 
	/// var json = JsonSerializer.Serialize(schedule);
	/// var deserialized = JsonSerializer.Deserialize&lt;UtcSchedule&gt;(json);
	/// 
	/// // Manual registration (if needed)
	/// var options = new JsonSerializerOptions();
	/// options.Converters.Add(new UtcDateTimeJsonConverter());
	/// var customJson = JsonSerializer.Serialize(schedule, options);
	/// </code>
	/// </example>
	public class UtcDateTimeJsonConverter : JsonConverter<UtcDateTime>
	{
		/// <summary>
		/// Reads and converts the JSON representation to a UtcDateTime.
		/// </summary>
		/// <param name="reader">The Utf8JsonReader to read from.</param>
		/// <param name="typeToConvert">The type being converted (UtcDateTime).</param>
		/// <param name="options">The serializer options being used.</param>
		/// <returns>A UtcDateTime normalized to UTC from the JSON string value.</returns>
		/// <exception cref="JsonException">
		/// Thrown when the JSON value cannot be parsed as a valid DateTime or DateTimeOffset.
		/// </exception>
		/// <example>
		/// Input JSON: "2024-06-15T10:30:00-04:00" (Eastern Daylight Time)
		/// Output: UtcDateTime representing 2024-06-15T14:30:00Z (UTC)
		/// </example>
		public override UtcDateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
			{
				var dateTimeString = reader.GetString();
				if (DateTimeOffset.TryParse(dateTimeString, out var dateTimeOffset))
				{
					return new UtcDateTime(dateTimeOffset);
				}

				if (DateTime.TryParse(dateTimeString, out var dateTime))
				{
					return new UtcDateTime(dateTime);
				}
			}

			throw new JsonException($"Unable to convert JSON value to UtcDateTime");
		}

		/// <summary>
		/// Writes the UtcDateTime as a JSON string value in ISO 8601 UTC format.
		/// </summary>
		/// <param name="writer">The Utf8JsonWriter to write to.</param>
		/// <param name="value">The UtcDateTime value to serialize.</param>
		/// <param name="options">The serializer options being used.</param>
		/// <example>
		/// <code>
		/// var utcDateTime = new UtcDateTime(new DateTime(2024, 6, 15, 18, 30, 0, DateTimeKind.Utc));
		/// // Serializes to: "2024-06-15T18:30:00.0000000Z"
		/// 
		/// var easternTime = new UtcDateTime(
		///     new DateTimeOffset(2024, 6, 15, 14, 30, 0, TimeSpan.FromHours(-4))
		/// );
		/// // Also serializes to: "2024-06-15T18:30:00.0000000Z" (converted to UTC)
		/// </code>
		/// </example>
		public override void Write(Utf8JsonWriter writer, UtcDateTime value, JsonSerializerOptions options)
		{
			// Write as ISO 8601 UTC format
			writer.WriteStringValue(value.DateTime.ToString("O"));
		}
	}
}
