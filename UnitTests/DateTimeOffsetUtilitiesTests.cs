using Knara.UtcStrict;

namespace UnitTests
{
    public class DateTimeOffsetUtilitiesTests
    {
        [Fact]
        public void NormalizeToUtc_WithNullDateTimeOffset_ReturnsUtcReplacementDt()
        {
            // Arrange
            DateTimeOffset? dateTimeOffset = null;
            var utcReplacementDt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);

            // Act
            var result = DateTimeOffsetUtilities.NormalizeToUtc(dateTimeOffset, utcReplacementDt);

            // Assert
            Assert.Equal(utcReplacementDt, result);
        }

        [Fact]
        public void NormalizeToUtc_WithMinValue_ReturnsMinValueToUniversalTime()
        {
            // Arrange
            var dateTimeOffset = DateTimeOffset.MinValue;
            var utcReplacementDt = DateTime.UtcNow;

            // Act
            var result = DateTimeOffsetUtilities.NormalizeToUtc(dateTimeOffset, utcReplacementDt);

			// Assert
			Assert.Equal(DateTimeOffset.MinValue.ToUniversalTime(), result);
		}

        [Fact]
        public void NormalizeToUtc_WithMaxValue_ReturnsMaxValueToUniversalTime()
        {
            // Arrange
            var dateTimeOffset = DateTimeOffset.MaxValue;
            var utcReplacementDt = DateTime.UtcNow;

            // Act
            var result = DateTimeOffsetUtilities.NormalizeToUtc(dateTimeOffset, utcReplacementDt);

            // Assert
            Assert.Equal(DateTime.MaxValue.ToUniversalTime(), result);
        }

        [Fact]
        public void NormalizeToUtc_WithZeroOffset_ReturnsDateTimeWithUtcKind()
        {
            // Arrange
            var dateTime = new DateTime(2023, 6, 15, 14, 30, 0);
            var dateTimeOffset = new DateTimeOffset(dateTime, TimeSpan.Zero);
            var utcReplacementDt = DateTime.UtcNow;

            // Act
            var result = DateTimeOffsetUtilities.NormalizeToUtc(dateTimeOffset, utcReplacementDt);

            // Assert
            Assert.Equal(dateTime, result);
            Assert.Equal(DateTimeKind.Utc, result.Kind);
        }

        [Fact]
        public void NormalizeToUtc_WithPositiveOffset_ConvertsToUtc()
        {
            // Arrange
            var localDateTime = new DateTime(2023, 6, 15, 14, 30, 0);
            var offset = TimeSpan.FromHours(5); // +5 hours
            var dateTimeOffset = new DateTimeOffset(localDateTime, offset);
            var utcReplacementDt = DateTime.UtcNow;

            // Act
            var result = DateTimeOffsetUtilities.NormalizeToUtc(dateTimeOffset, utcReplacementDt);

            // Assert
            var expectedUtc = localDateTime.ToUniversalTime();
            Assert.Equal(DateTimeKind.Utc, result.Kind);
            // The result should be the UTC equivalent
            Assert.Equal(dateTimeOffset.UtcDateTime, result);
        }

        [Fact]
        public void NormalizeToUtc_WithNegativeOffset_ConvertsToUtc()
        {
            // Arrange
            var localDateTime = new DateTime(2023, 6, 15, 14, 30, 0);
            var offset = TimeSpan.FromHours(-3); // -3 hours
            var dateTimeOffset = new DateTimeOffset(localDateTime, offset);
            var utcReplacementDt = DateTime.UtcNow;

            // Act
            var result = DateTimeOffsetUtilities.NormalizeToUtc(dateTimeOffset, utcReplacementDt);

            // Assert
            Assert.Equal(DateTimeKind.Utc, result.Kind);
            Assert.Equal(dateTimeOffset.UtcDateTime, result);
        }

        [Theory]
        [InlineData(1)] // +1 hour
        [InlineData(-2)] // -2 hours
        [InlineData(8)] // +8 hours
        [InlineData(-5)] // -5 hours
        public void NormalizeToUtc_WithVariousOffsets_ConvertsCorrectly(int offsetHours)
        {
            // Arrange
            var localDateTime = new DateTime(2023, 6, 15, 14, 30, 0);
            var offset = TimeSpan.FromHours(offsetHours);
            var dateTimeOffset = new DateTimeOffset(localDateTime, offset);
            var utcReplacementDt = DateTime.UtcNow;

            // Act
            var result = DateTimeOffsetUtilities.NormalizeToUtc(dateTimeOffset, utcReplacementDt);

            // Assert
            Assert.Equal(DateTimeKind.Utc, result.Kind);
            Assert.Equal(dateTimeOffset.UtcDateTime, result);
        }

        [Fact]
        public void NormalizeToUtc_WithUtcOffset_HandlesCorrectly()
        {
            // Arrange
            var utcDateTime = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);
            var dateTimeOffset = new DateTimeOffset(utcDateTime);
            var utcReplacementDt = DateTime.UtcNow;

            // Act
            var result = DateTimeOffsetUtilities.NormalizeToUtc(dateTimeOffset, utcReplacementDt);

            // Assert
            Assert.Equal(DateTimeKind.Utc, result.Kind);
            Assert.Equal(utcDateTime, result);
        }
    }
}