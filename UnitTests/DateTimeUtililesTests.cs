using Knara.UtcStrict;

namespace UnitTests
{
    public class DateTimeUtililesTests
    {
        [Fact]
        public void NormalizeToUtc_WithNullDateTime_ReturnsUtcReplacementDt()
        {
            // Arrange
            DateTime? dateTime = null;
            var utcReplacementDt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);

            // Act
            var result = DateTimeUtilities.NormalizeToUtc(dateTime, utcReplacementDt);

            // Assert
            Assert.Equal(utcReplacementDt, result);
        }

        [Fact]
        public void NormalizeToUtc_WithMinValue_ReturnsMinValueToUniversalTime()
        {
            // Arrange
            var dateTime = DateTime.MinValue;
            var utcReplacementDt = DateTime.UtcNow;

            // Act
            var result = DateTimeUtilities.NormalizeToUtc(dateTime, utcReplacementDt);

            // Assert
            Assert.Equal(DateTimeOffset.MinValue.ToUniversalTime(), result);
        }

        [Fact]
        public void NormalizeToUtc_WithMinValueAlreadyUtc_ReturnsSameValue()
        {
            // Arrange
            var dateTime = DateTime.MinValue.ToUniversalTime();
            var utcReplacementDt = DateTime.UtcNow;

            // Act
            var result = DateTimeUtilities.NormalizeToUtc(dateTime, utcReplacementDt);

			// Assert
			Assert.Equal(DateTimeOffset.MinValue.ToUniversalTime(), result);
		}

        [Fact]
        public void NormalizeToUtc_WithMaxValue_ReturnsMaxValueToUniversalTime()
        {
            // Arrange
            var dateTime = DateTime.MaxValue;
            var utcReplacementDt = DateTime.UtcNow;

            // Act
            var result = DateTimeUtilities.NormalizeToUtc(dateTime, utcReplacementDt);

            // Assert
            Assert.Equal(DateTime.MaxValue.ToUniversalTime(), result);
        }

        [Fact]
        public void NormalizeToUtc_WithMaxValueAlreadyUtc_ReturnsSameValue()
        {
            // Arrange
            var dateTime = DateTime.MaxValue.ToUniversalTime();
            var utcReplacementDt = DateTime.UtcNow;

            // Act
            var result = DateTimeUtilities.NormalizeToUtc(dateTime, utcReplacementDt);

            // Assert
            Assert.Equal(dateTime, result);
        }

        [Fact]
        public void NormalizeToUtc_WithUtcDateTime_ReturnsSameValue()
        {
            // Arrange
            var dateTime = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);
            var utcReplacementDt = DateTime.UtcNow;

            // Act
            var result = DateTimeUtilities.NormalizeToUtc(dateTime, utcReplacementDt);

            // Assert
            Assert.Equal(dateTime, result);
            Assert.Equal(DateTimeKind.Utc, result.Kind);
        }

        [Fact]
        public void NormalizeToUtc_WithLocalDateTime_ReturnsToUniversalTime()
        {
            // Arrange
            var localDateTime = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Local);
            var utcReplacementDt = DateTime.UtcNow;

            // Act
            var result = DateTimeUtilities.NormalizeToUtc(localDateTime, utcReplacementDt);

            // Assert
            var expected = localDateTime.ToUniversalTime();
            Assert.Equal(expected, result);
            Assert.Equal(DateTimeKind.Utc, result.Kind);
        }

        [Fact]
        public void NormalizeToUtc_WithUnspecifiedDateTime_ReturnsToUniversalTime()
        {
            // Arrange
            var unspecifiedDateTime = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Unspecified);
            var utcReplacementDt = DateTime.UtcNow;

            // Act
            var result = DateTimeUtilities.NormalizeToUtc(unspecifiedDateTime, utcReplacementDt);

            // Assert
            var expected = unspecifiedDateTime.ToUniversalTime();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(DateTimeKind.Local)]
        [InlineData(DateTimeKind.Unspecified)]
        public void NormalizeToUtc_WithNonUtcKind_CallsToUniversalTime(DateTimeKind kind)
        {
            // Arrange
            var dateTime = new DateTime(2023, 6, 15, 14, 30, 0, kind);
            var utcReplacementDt = DateTime.UtcNow;

            // Act
            var result = DateTimeUtilities.NormalizeToUtc(dateTime, utcReplacementDt);

            // Assert
            var expected = dateTime.ToUniversalTime();
            Assert.Equal(expected, result);
        }
    }
}