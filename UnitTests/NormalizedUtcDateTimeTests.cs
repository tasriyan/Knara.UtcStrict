using Knara.UtcStrict;

namespace UnitTests
{
    public class NormalizedUtcDateTimeTests
    {
        [Fact]
        public void Constructor_WithDateTime_NormalizesToUtc()
        {
            // Arrange
            var localDateTime = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Local);

            // Act
            var normalized = new UtcDateTime(localDateTime);

            // Assert
            Assert.Equal(DateTimeKind.Utc, normalized.DateTime.Kind);
        }

        [Fact]
        public void Constructor_WithDateTimeOffset_NormalizesToUtc()
        {
            // Arrange
            var dateTimeOffset = new DateTimeOffset(2023, 6, 15, 14, 30, 0, TimeSpan.FromHours(5));

            // Act
            var normalized = new UtcDateTime(dateTimeOffset);

            // Assert
            Assert.Equal(DateTimeKind.Utc, normalized.DateTime.Kind);
        }

        [Fact]
        public void Constructor_WithUtcDateTime_PreservesValue()
        {
            // Arrange
            var utcDateTime = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);

            // Act
            var normalized = new UtcDateTime(utcDateTime);

            // Assert
            Assert.Equal(utcDateTime, normalized.DateTime);
            Assert.Equal(DateTimeKind.Utc, normalized.DateTime.Kind);
        }

        [Fact]
        public void UtcNow_ReturnsCurrentUtcTime()
        {
            // Act
            var normalized = UtcDateTime.UtcNow;

            // Assert
            Assert.Equal(DateTimeKind.Utc, normalized.DateTime.Kind);
            // Should be close to DateTime.UtcNow (within a few seconds)
            var diff = Math.Abs((DateTime.UtcNow - normalized.DateTime).TotalSeconds);
            Assert.True(diff < 5, "UtcNow should be close to DateTime.UtcNow");
        }

        [Fact]
        public void MinValue_ReturnsMinValueAsUtc()
        {
            // Act
            var normalized = UtcDateTime.MinValue;

			// Assert
			Assert.Equal(DateTimeOffset.MinValue.ToUniversalTime(), normalized.DateTime);
            Assert.Equal(DateTimeKind.Utc, normalized.DateTime.Kind);
        }

        [Fact]
        public void MaxValue_ReturnsMaxValueAsUtc()
        {
            // Act
            var normalized = UtcDateTime.MaxValue;

            // Assert
            Assert.Equal(DateTime.MaxValue.ToUniversalTime(), normalized.DateTime);
            Assert.Equal(DateTimeKind.Utc, normalized.DateTime.Kind);
        }

        [Fact]
        public void ImplicitOperator_ToDateTime_ReturnsInternalDateTime()
        {
            // Arrange
            var utcDateTime = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);
            var normalized = new UtcDateTime(utcDateTime);

            // Act
            DateTime result = normalized;

            // Assert
            Assert.Equal(normalized.DateTime, result);
        }

        [Fact]
        public void ImplicitOperator_FromDateTimeOffset_CreatesNormalizedInstance()
        {
            // Arrange
            var dateTimeOffset = new DateTimeOffset(2023, 6, 15, 14, 30, 0, TimeSpan.FromHours(-3));

			// Act
			UtcDateTime normalized = dateTimeOffset;

            // Assert
            Assert.Equal(DateTimeKind.Utc, normalized.DateTime.Kind);
        }

        [Fact]
        public void ImplicitOperator_ToDateTimeOffset_CreatesCorrectOffset()
        {
            // Arrange
            var utcDateTime = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);
            var normalized = new UtcDateTime(utcDateTime);

            // Act
            DateTimeOffset result = normalized;

            // Assert
            Assert.Equal(TimeSpan.Zero, result.Offset);
            Assert.Equal(utcDateTime, result.DateTime);
        }

        [Fact]
        public void EqualityOperator_WithSameValues_ReturnsTrue()
        {
            // Arrange
            var utcDateTime = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);
            var normalized1 = new UtcDateTime(utcDateTime);
            var normalized2 = new UtcDateTime(utcDateTime);

            // Act & Assert
            Assert.True(normalized1 == normalized2);
        }

        [Fact]
        public void EqualityOperator_WithDifferentValues_ReturnsFalse()
        {
            // Arrange
            var utcDateTime1 = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);
            var utcDateTime2 = new DateTime(2023, 6, 15, 15, 30, 0, DateTimeKind.Utc);
            var normalized1 = new UtcDateTime(utcDateTime1);
            var normalized2 = new UtcDateTime(utcDateTime2);

            // Act & Assert
            Assert.False(normalized1 == normalized2);
        }

        [Fact]
        public void InequalityOperator_WithDifferentValues_ReturnsTrue()
        {
            // Arrange
            var utcDateTime1 = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);
            var utcDateTime2 = new DateTime(2023, 6, 15, 15, 30, 0, DateTimeKind.Utc);
            var normalized1 = new UtcDateTime(utcDateTime1);
            var normalized2 = new UtcDateTime(utcDateTime2);

            // Act & Assert
            Assert.True(normalized1 != normalized2);
        }

        [Fact]
        public void LessThanOrEqualOperator_WorksCorrectly()
        {
            // Arrange
            var earlier = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);
            var later = new DateTime(2023, 6, 15, 15, 30, 0, DateTimeKind.Utc);
            var normalized1 = new UtcDateTime(earlier);
            var normalized2 = new UtcDateTime(later);

            // Act & Assert
            Assert.True(normalized1 <= normalized2);
            Assert.True(normalized1 <= normalized1);
            Assert.False(normalized2 <= normalized1);
        }

        [Fact]
        public void GreaterThanOrEqualOperator_WorksCorrectly()
        {
            // Arrange
            var earlier = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);
            var later = new DateTime(2023, 6, 15, 15, 30, 0, DateTimeKind.Utc);
            var normalized1 = new UtcDateTime(earlier);
            var normalized2 = new UtcDateTime(later);

            // Act & Assert
            Assert.True(normalized2 >= normalized1);
            Assert.True(normalized1 >= normalized1);
            Assert.False(normalized1 >= normalized2);
        }

        [Fact]
        public void Equals_WithSameValue_ReturnsTrue()
        {
            // Arrange
            var utcDateTime = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);
            var normalized1 = new UtcDateTime(utcDateTime);
            var normalized2 = new UtcDateTime(utcDateTime);

            // Act & Assert
            Assert.True(normalized1.Equals(normalized2));
        }

        [Fact]
        public void Equals_WithDifferentType_ReturnsFalse()
        {
            // Arrange
            var utcDateTime = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);
            var normalized = new UtcDateTime(utcDateTime);
            var other = "not a NormalizedUtcDateTime";

            // Act & Assert
            Assert.False(normalized.Equals(other));
        }

        [Fact]
        public void GetHashCode_WithSameValue_ReturnsSameHashCode()
        {
            // Arrange
            var utcDateTime = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);
            var normalized1 = new UtcDateTime(utcDateTime);
            var normalized2 = new UtcDateTime(utcDateTime);

            // Act & Assert
            Assert.Equal(normalized1.GetHashCode(), normalized2.GetHashCode());
        }

        [Theory]
        [InlineData(DateTimeKind.Local)]
        [InlineData(DateTimeKind.Unspecified)]
        [InlineData(DateTimeKind.Utc)]
        public void Constructor_WithDifferentDateTimeKinds_NormalizesToUtc(DateTimeKind kind)
        {
            // Arrange
            var dateTime = new DateTime(2023, 6, 15, 14, 30, 0, kind);

            // Act
            var normalized = new UtcDateTime(dateTime);

            // Assert
            Assert.Equal(DateTimeKind.Utc, normalized.DateTime.Kind);
        }
    }
}