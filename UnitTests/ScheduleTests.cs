using Knara.UtcStrict;

namespace UnitTests
{
    public class ScheduleTests
    {
        [Fact]
        public void Constructor_WithValidDates_CreatesSchedule()
        {
            // Arrange
            var enableOn = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var disableOn = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);

            // Act
            var schedule = new UtcSchedule(new UtcDateTime(enableOn), new UtcDateTime(disableOn));

            // Assert
            Assert.Equal(new UtcDateTime(enableOn), schedule.EnableOn.DateTime);
            Assert.Equal(new UtcDateTime(disableOn), schedule.DisableOn.DateTime);
        }

        [Fact]
        public void Constructor_WithDisableBeforeEnable_ThrowsArgumentException()
        {
            // Arrange
            var enableOn = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
            var disableOn = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new UtcSchedule(new UtcDateTime(enableOn), new UtcDateTime(disableOn)));
            Assert.Contains("Scheduled disable date must be after the scheduled enable date", exception.Message);
        }

        [Fact]
        public void Constructor_WithEqualDates_ThrowsArgumentException()
        {
            // Arrange
            var dateTime = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new UtcSchedule(new UtcDateTime(dateTime), new UtcDateTime(dateTime)));
            Assert.Contains("Scheduled disable date must be after the scheduled enable date", exception.Message);
        }

        [Fact]
        public void Constructor_WithDateTimeOffset_NormalizesToUtc()
        {
            // Arrange
            var enableOn = new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.FromHours(5));
            var disableOn = new DateTimeOffset(2024, 1, 15, 18, 0, 0, TimeSpan.FromHours(5));

            // Act
            var schedule = new UtcSchedule(new UtcDateTime(enableOn), new UtcDateTime(disableOn));

            // Assert
            Assert.Equal(DateTimeKind.Utc, schedule.EnableOn.DateTime.Kind);
            Assert.Equal(DateTimeKind.Utc, schedule.DisableOn.DateTime.Kind);
        }

        [Fact]
        public void Unscheduled_ReturnsScheduleWithMinMaxValues()
        {
            // Act
            var unscheduled = UtcSchedule.Unscheduled;

			// Assert
			Assert.Equal(DateTimeOffset.MinValue.ToUniversalTime(), unscheduled.EnableOn.DateTime);
            Assert.Equal(DateTimeOffset.MaxValue.ToUniversalTime(), unscheduled.DisableOn.DateTime);
        }

        [Fact]
        public void CreateSchedule_WithFutureDates_CreatesValidSchedule()
        {
            // Arrange
            var futureEnable = DateTime.UtcNow.AddHours(1);
            var futureDisable = DateTime.UtcNow.AddHours(2);

            // Act
            var schedule = UtcSchedule.CreateSchedule(new UtcDateTime(futureEnable), new UtcDateTime(futureDisable));

            // Assert
            Assert.Equal(new UtcDateTime(futureEnable), schedule.EnableOn.DateTime);
            Assert.Equal(new UtcDateTime(futureDisable), schedule.DisableOn.DateTime);
        }

        [Fact]
        public void CreateSchedule_WithPastEnableDate_ThrowsArgumentException()
        {
            // Arrange
            var pastEnable = UtcDateTime.UtcNow.DateTime.AddHours(-1);
            var futureDisable = UtcDateTime.UtcNow.DateTime.AddHours(1);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => UtcSchedule.CreateSchedule(new UtcDateTime(pastEnable), new UtcDateTime(futureDisable)));
            Assert.Contains("Scheduled enable date must be in the future", exception.Message);
        }

        [Fact]
        public void CreateSchedule_WithCurrentTime_ThrowsArgumentException()
        {
            // Arrange
            var currentTime = UtcDateTime.UtcNow;
            var futureDisable = UtcDateTime.UtcNow.DateTime.AddHours(1);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => UtcSchedule.CreateSchedule(new UtcDateTime(currentTime), new UtcDateTime(futureDisable)));
            Assert.Contains("Scheduled enable date must be in the future", exception.Message);
        }

        [Fact]
        public void HasSchedule_WithUnscheduled_ReturnsFalse()
        {
            // Arrange
            var unscheduled = UtcSchedule.Unscheduled;

            // Act
            var result = unscheduled.HasSchedule();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasSchedule_WithValidSchedule_ReturnsTrue()
        {
            // Arrange
            var enableOn = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var disableOn = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
            var schedule = new UtcSchedule(new UtcDateTime(enableOn), new UtcDateTime(disableOn));

            // Act
            var result = schedule.HasSchedule();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasSchedule_WithOnlyEnableSet_ReturnsTrue()
        {
            // Arrange
            var enableOn = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var disableOn = DateTime.MaxValue;
            var schedule = new UtcSchedule(new UtcDateTime(enableOn), new UtcDateTime(disableOn));

            // Act
            var result = schedule.HasSchedule();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsActiveSchedule_WithUnscheduled_ReturnsFalseWithReason()
        {
            // Arrange
            var unscheduled = UtcSchedule.Unscheduled;
            var evaluationTime = UtcDateTime.UtcNow;

            // Act
            var (isActive, reason) = unscheduled.IsActiveAt(new UtcDateTime(evaluationTime));

            // Assert
            Assert.False(isActive);
            Assert.Equal("No active schedule set", reason);
        }

        [Fact]
        public void IsActiveSchedule_BeforeEnableDate_ReturnsFalseWithReason()
        {
            // Arrange
            var enableOn = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var disableOn = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
            var schedule = new UtcSchedule(new UtcDateTime(enableOn), new UtcDateTime(disableOn));
            var evaluationTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);

            // Act
            var (isActive, reason) = schedule.IsActiveAt(new UtcDateTime(evaluationTime));

            // Assert
            Assert.False(isActive);
            Assert.Equal("Scheduled enable date not reached", reason);
        }

        [Fact]
        public void IsActiveSchedule_AfterDisableDate_ReturnsFalseWithReason()
        {
            // Arrange
            var enableOn = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var disableOn = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
            var schedule = new UtcSchedule(new UtcDateTime(enableOn), new UtcDateTime(disableOn));
            var evaluationTime = new DateTime(2024, 1, 15, 19, 0, 0, DateTimeKind.Utc);

            // Act
            var (isActive, reason) = schedule.IsActiveAt(new UtcDateTime(evaluationTime));

            // Assert
            Assert.False(isActive);
            Assert.Equal("Scheduled disable date passed", reason);
        }

        [Fact]
        public void IsActiveSchedule_AtDisableDate_ReturnsFalseWithReason()
        {
            // Arrange
            var enableOn = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var disableOn = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
            var schedule = new UtcSchedule(new UtcDateTime(enableOn), new UtcDateTime(disableOn));

            // Act
            var (isActive, reason) = schedule.IsActiveAt(new UtcDateTime(disableOn));

            // Assert
            Assert.False(isActive);
            Assert.Equal("Scheduled disable date passed", reason);
        }

        [Fact]
        public void IsActiveSchedule_BetweenEnableAndDisable_ReturnsTrueWithReason()
        {
            // Arrange
            var enableOn = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var disableOn = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
            var schedule = new UtcSchedule(new UtcDateTime(enableOn), new UtcDateTime(disableOn));
            var evaluationTime = new DateTime(2024, 1, 15, 14, 0, 0, DateTimeKind.Utc);

            // Act
            var (isActive, reason) = schedule.IsActiveAt(new UtcDateTime(evaluationTime));

            // Assert
            Assert.True(isActive);
            Assert.Equal("Schedule is active", reason);
        }

        [Fact]
        public void IsActiveSchedule_AtEnableDate_ReturnsTrueWithReason()
        {
            // Arrange
            var enableOn = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var disableOn = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
            var schedule = new UtcSchedule(new UtcDateTime(enableOn), new UtcDateTime(disableOn));

            // Act
            var (isActive, reason) = schedule.IsActiveAt(new UtcDateTime(enableOn));

            // Assert
            Assert.True(isActive);
            Assert.Equal("Schedule is active", reason);
        }

        [Fact]
        public void EqualityOperator_WithSameValues_ReturnsTrue()
        {
            // Arrange
            var enableOn = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var disableOn = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
            var schedule1 = new UtcSchedule(new UtcDateTime(enableOn), new UtcDateTime(disableOn));
            var schedule2 = new UtcSchedule(new UtcDateTime(enableOn), new UtcDateTime(disableOn));

            // Act & Assert
            Assert.True(schedule1 == schedule2);
        }

        [Fact]
        public void EqualityOperator_WithDifferentValues_ReturnsFalse()
        {
            // Arrange
            var enableOn1 = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var disableOn1 = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
            var enableOn2 = new DateTime(2024, 1, 15, 11, 0, 0, DateTimeKind.Utc);
            var disableOn2 = new DateTime(2024, 1, 15, 19, 0, 0, DateTimeKind.Utc);
            var schedule1 = new UtcSchedule(new UtcDateTime(enableOn1), new UtcDateTime(disableOn1));
            var schedule2 = new UtcSchedule(new UtcDateTime(enableOn2), new UtcDateTime(disableOn2));

            // Act & Assert
            Assert.False(schedule1 == schedule2);
        }

        [Fact]
        public void EqualityOperator_WithNullValues_HandlesCorrectly()
        {
            // Arrange
            UtcSchedule? schedule1 = null;
            UtcSchedule? schedule2 = null;
            var schedule3 = new UtcSchedule(
                new UtcDateTime(new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc)),
                new UtcDateTime(new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc)));

            // Act & Assert
            Assert.True(schedule1 == schedule2);
            Assert.False(schedule1 == schedule3);
            Assert.False(schedule3 == schedule1);
        }

        [Fact]
        public void InequalityOperator_WithDifferentValues_ReturnsTrue()
        {
            // Arrange
            var enableOn1 = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var disableOn1 = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
            var enableOn2 = new DateTime(2024, 1, 15, 11, 0, 0, DateTimeKind.Utc);
            var disableOn2 = new DateTime(2024, 1, 15, 19, 0, 0, DateTimeKind.Utc);
            var schedule1 = new UtcSchedule(new UtcDateTime(enableOn1), new UtcDateTime(disableOn1));
            var schedule2 = new UtcSchedule(new UtcDateTime(enableOn2), new UtcDateTime(disableOn2));

            // Act & Assert
            Assert.True(schedule1 != schedule2);
        }

        [Fact]
        public void Equals_WithSameValues_ReturnsTrue()
        {
            // Arrange
            var enableOn = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var disableOn = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
            var schedule1 = new UtcSchedule(new UtcDateTime(enableOn), new UtcDateTime(disableOn));
            var schedule2 = new UtcSchedule(new UtcDateTime(enableOn), new UtcDateTime(disableOn));

            // Act & Assert
            Assert.True(schedule1.Equals(schedule2));
        }

        [Fact]
        public void Equals_WithDifferentType_ReturnsFalse()
        {
            // Arrange
            var schedule = new UtcSchedule(
				new UtcDateTime(new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc)),
                new UtcDateTime(new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc)));
            var other = "not a schedule";

            // Act & Assert
            Assert.False(schedule.Equals(other));
        }

        [Fact]
        public void GetHashCode_WithSameValues_ReturnsSameHashCode()
        {
            // Arrange
            var enableOn = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var disableOn = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
            var schedule1 = new UtcSchedule(new UtcDateTime(enableOn), new UtcDateTime(disableOn));
            var schedule2 = new UtcSchedule(new UtcDateTime(enableOn), new UtcDateTime(disableOn));

            // Act & Assert
            Assert.Equal(schedule1.GetHashCode(), schedule2.GetHashCode());
        }

        [Theory]
        [InlineData(DateTimeKind.Local)]
        [InlineData(DateTimeKind.Unspecified)]
        [InlineData(DateTimeKind.Utc)]
        public void Constructor_WithDifferentDateTimeKinds_NormalizesToUtc(DateTimeKind kind)
        {
            // Arrange
            var enableOn = new DateTime(2024, 1, 15, 10, 0, 0, kind);
            var disableOn = new DateTime(2024, 1, 15, 18, 0, 0, kind);

            // Act
            var schedule = new UtcSchedule(new UtcDateTime(enableOn), new UtcDateTime(disableOn));

            // Assert
            Assert.Equal(DateTimeKind.Utc, schedule.EnableOn.DateTime.Kind);
            Assert.Equal(DateTimeKind.Utc, schedule.DisableOn.DateTime.Kind);
        }

        [Fact]
        public void JsonSerialization_AndDeserialization_WorksCorrectly()
        {
            // Arrange
            var enableOn = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var disableOn = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
            var schedule = new UtcSchedule(new UtcDateTime(enableOn), new UtcDateTime(disableOn));
            // Act
            var json = System.Text.Json.JsonSerializer.Serialize(schedule);
            var deserializedSchedule = System.Text.Json.JsonSerializer.Deserialize<UtcSchedule>(json);
            // Assert
            Assert.NotNull(deserializedSchedule);
            Assert.Equal(schedule.EnableOn.DateTime, deserializedSchedule!.EnableOn.DateTime);
            Assert.Equal(schedule.DisableOn.DateTime, deserializedSchedule.DisableOn.DateTime);

            // Additional check for DateTimeKind
            schedule = UtcSchedule.CreateSchedule(DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddDays(7));
			// Act
			json = System.Text.Json.JsonSerializer.Serialize(schedule);
			deserializedSchedule = System.Text.Json.JsonSerializer.Deserialize<UtcSchedule>(json);
			// Assert
			Assert.NotNull(deserializedSchedule);
			Assert.Equal(schedule.EnableOn.DateTime, deserializedSchedule!.EnableOn.DateTime);
			Assert.Equal(schedule.DisableOn.DateTime, deserializedSchedule.DisableOn.DateTime);
		}
	}
}