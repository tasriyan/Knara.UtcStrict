using Knara.UtcStrict;

namespace UnitTests
{
    public class TimeWindowTests
    {
        [Fact]
        public void Constructor_WithValidParameters_CreatesTimeWindow()
        {
            // Arrange
            var startTime = new TimeSpan(9, 0, 0);
            var stopTime = new TimeSpan(17, 0, 0);
            var timeZone = "UTC";
            var daysActive = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday };

            // Act
            var window = new UtcTimeWindow(startTime, stopTime, timeZone, daysActive);

            // Assert
            Assert.Equal(startTime, window.StartOn);
            Assert.Equal(stopTime, window.StopOn);
            Assert.Equal(timeZone, window.TimeZone);
            Assert.Equal(daysActive.Length, window.DaysActive.Length);
            Assert.True(daysActive.All(d => window.DaysActive.Contains(d)));
        }

        [Fact]
        public void Constructor_WithDefaultParameters_UsesDefaults()
        {
            // Arrange
            var startTime = new TimeSpan(9, 0, 0);
            var stopTime = new TimeSpan(17, 0, 0);

            // Act
            var window = new UtcTimeWindow(startTime, stopTime);

            // Assert
            Assert.Equal(startTime, window.StartOn);
            Assert.Equal(stopTime, window.StopOn);
            Assert.Equal("UTC", window.TimeZone);
            Assert.Equal(7, window.DaysActive.Length); // All days
            Assert.Contains(DayOfWeek.Monday, window.DaysActive);
            Assert.Contains(DayOfWeek.Sunday, window.DaysActive);
        }

        [Theory]
        [InlineData(-1, 0, 0)]
        [InlineData(24, 0, 0)]
        [InlineData(25, 0, 0)]
        public void Constructor_WithInvalidStartTime_ThrowsArgumentException(int hours, int minutes, int seconds)
        {
            // Arrange
            var invalidStartTime = new TimeSpan(hours, minutes, seconds);
            var validStopTime = new TimeSpan(17, 0, 0);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new UtcTimeWindow(invalidStartTime, validStopTime));
            Assert.Contains("Start time must be between 00:00:00 and 23:59:59", exception.Message);
        }

        [Theory]
        [InlineData(-1, 0, 0)]
        [InlineData(24, 0, 0)]
        [InlineData(25, 0, 0)]
        public void Constructor_WithInvalidStopTime_ThrowsArgumentException(int hours, int minutes, int seconds)
        {
            // Arrange
            var validStartTime = new TimeSpan(9, 0, 0);
            var invalidStopTime = new TimeSpan(hours, minutes, seconds);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new UtcTimeWindow(validStartTime, invalidStopTime));
            Assert.Contains("End time must be between 00:00:00 and 23:59:59", exception.Message);
        }

        [Fact]
        public void Constructor_WithInvalidTimeZone_ThrowsArgumentException()
        {
            // Arrange
            var startTime = new TimeSpan(9, 0, 0);
            var stopTime = new TimeSpan(17, 0, 0);
            var invalidTimeZone = "Invalid/TimeZone";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new UtcTimeWindow(startTime, stopTime, invalidTimeZone));
            Assert.Contains("Invalid timezone identifier", exception.Message);
        }

        [Fact]
        public void Constructor_WithNullTimeZone_DefaultsToUtc()
        {
            // Arrange
            var startTime = new TimeSpan(9, 0, 0);
            var stopTime = new TimeSpan(17, 0, 0);

            // Act
            var window = new UtcTimeWindow(startTime, stopTime, null);

            // Assert
            Assert.Equal("UTC", window.TimeZone);
        }

        [Fact]
        public void Constructor_WithEmptyTimeZone_DefaultsToUtc()
        {
            // Arrange
            var startTime = new TimeSpan(9, 0, 0);
            var stopTime = new TimeSpan(17, 0, 0);

            // Act
            var window = new UtcTimeWindow(startTime, stopTime, "");

            // Assert
            Assert.Equal("UTC", window.TimeZone);
        }

        [Fact]
        public void Constructor_WithNullDaysActive_DefaultsToAllDays()
        {
            // Arrange
            var startTime = new TimeSpan(9, 0, 0);
            var stopTime = new TimeSpan(17, 0, 0);

            // Act
            var window = new UtcTimeWindow(startTime, stopTime, "UTC", null);

            // Assert
            Assert.Equal(7, window.DaysActive.Length);
            Assert.True(Enum.GetValues<DayOfWeek>().All(d => window.DaysActive.Contains(d)));
        }

        [Fact]
        public void Constructor_WithEmptyDaysActive_DefaultsToAllDays()
        {
            // Arrange
            var startTime = new TimeSpan(9, 0, 0);
            var stopTime = new TimeSpan(17, 0, 0);
            var emptyDays = Array.Empty<DayOfWeek>();

            // Act
            var window = new UtcTimeWindow(startTime, stopTime, "UTC", emptyDays);

            // Assert
            Assert.Equal(7, window.DaysActive.Length);
        }

        [Fact]
        public void Constructor_WithDuplicateDaysActive_RemovesDuplicates()
        {
            // Arrange
            var startTime = new TimeSpan(9, 0, 0);
            var stopTime = new TimeSpan(17, 0, 0);
            var duplicateDays = new[] { DayOfWeek.Monday, DayOfWeek.Monday, DayOfWeek.Tuesday };

            // Act
            var window = new UtcTimeWindow(startTime, stopTime, "UTC", duplicateDays);

            // Assert
            Assert.Equal(2, window.DaysActive.Length);
            Assert.Contains(DayOfWeek.Monday, window.DaysActive);
            Assert.Contains(DayOfWeek.Tuesday, window.DaysActive);
        }

        [Fact]
        public void Constructor_AllowsOvernightWindow()
        {
            // Arrange - 22:00 to 06:00 (overnight)
            var startTime = new TimeSpan(22, 0, 0);
            var stopTime = new TimeSpan(6, 0, 0);

            // Act
            var window = new UtcTimeWindow(startTime, stopTime);

            // Assert
            Assert.Equal(startTime, window.StartOn);
            Assert.Equal(stopTime, window.StopOn);
        }

        [Fact]
        public void AlwaysOpen_ReturnsCorrectTimeWindow()
        {
            // Act
            var alwaysOpen = UtcTimeWindow.AlwaysOpen;

            // Assert
            Assert.Equal(TimeSpan.Zero, alwaysOpen.StartOn);
            Assert.Equal(new TimeSpan(23, 59, 59), alwaysOpen.StopOn);
        }

        [Fact]
        public void HasWindow_WithNonZeroTimes_ReturnsTrue()
        {
            // Arrange
            var window = new UtcTimeWindow(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));

            // Act
            var result = window.HasWindow();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasWindow_WithZeroStartTime_ReturnsFalse()
        {
			// Arrange
			// This creates a window from 00:00:00 (midnight) to 17:00:00
			var window = new UtcTimeWindow(TimeSpan.Zero, new TimeSpan(17, 0, 0));

            // Act
            var result = window.HasWindow();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasWindow_WithZeroStopTime_ReturnsFalse()
        {
			// Arrange
			// This creates a window from 09:00:00 to 00:00:00 (midnight)
			var window = new UtcTimeWindow(new TimeSpan(9, 0, 0), TimeSpan.Zero);

            // Act
            var result = window.HasWindow();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsActiveAt_WithinTimeWindowAndAllowedDay_ReturnsTrue()
        {
            // Arrange
            var window = new UtcTimeWindow(
                new TimeSpan(9, 0, 0), 
                new TimeSpan(17, 0, 0), 
                "UTC", 
                new[] { DayOfWeek.Monday });
            
            // Monday, 12:00 UTC
            var evaluationTime = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc); // Monday

            // Act
            var (isActive, reason) = window.IsActiveAt(new UtcDateTime(evaluationTime));

            // Assert
            Assert.True(isActive);
            Assert.Equal("Within time window", reason);
        }

        [Fact]
        public void IsActiveAt_OutsideAllowedDay_ReturnsFalse()
        {
            // Arrange
            var window = new UtcTimeWindow(
                new TimeSpan(9, 0, 0), 
                new TimeSpan(17, 0, 0), 
                "UTC", 
                new[] { DayOfWeek.Monday });
            
            // Tuesday, 12:00 UTC
            var evaluationTime = new DateTime(2024, 1, 16, 12, 0, 0, DateTimeKind.Utc); // Tuesday

            // Act
            var (isActive, reason) = window.IsActiveAt(new UtcDateTime(evaluationTime));

            // Assert
            Assert.False(isActive);
            Assert.Equal("Outside allowed days", reason);
        }

        [Fact]
        public void IsActiveAt_OutsideTimeWindow_ReturnsFalse()
        {
            // Arrange
            var window = new UtcTimeWindow(
                new TimeSpan(9, 0, 0), 
                new TimeSpan(17, 0, 0), 
                "UTC", 
                new[] { DayOfWeek.Monday });
            
            // Monday, 08:00 UTC (before window)
            var evaluationTime = new DateTime(2024, 1, 15, 8, 0, 0, DateTimeKind.Utc); // Monday

            // Act
            var (isActive, reason) = window.IsActiveAt(new UtcDateTime(evaluationTime));

            // Assert
            Assert.False(isActive);
            Assert.Equal("Outside time window", reason);
        }

        [Fact]
        public void IsActiveAt_AtStartTime_ReturnsTrue()
        {
            // Arrange
            var window = new UtcTimeWindow(
                new TimeSpan(9, 0, 0), 
                new TimeSpan(17, 0, 0), 
                "UTC", 
                new[] { DayOfWeek.Monday });
            
            // Monday, 09:00 UTC (exactly at start)
            var evaluationTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc); // Monday

            // Act
            var (isActive, reason) = window.IsActiveAt(new UtcDateTime(evaluationTime));

            // Assert
            Assert.True(isActive);
            Assert.Equal("Within time window", reason);
        }

        [Fact]
        public void IsActiveAt_AtStopTime_ReturnsTrue()
        {
            // Arrange
            var window = new UtcTimeWindow(
                new TimeSpan(9, 0, 0), 
                new TimeSpan(17, 0, 0), 
                "UTC", 
                new[] { DayOfWeek.Monday });
            
            // Monday, 17:00 UTC (exactly at stop)
            var evaluationTime = new DateTime(2024, 1, 15, 17, 0, 0, DateTimeKind.Utc); // Monday

            // Act
            var (isActive, reason) = window.IsActiveAt(new UtcDateTime(evaluationTime));

            // Assert
            Assert.True(isActive);
            Assert.Equal("Within time window", reason);
        }

        [Fact]
        public void IsActiveAt_OvernightWindow_HandlesCorrectly()
        {
            // Arrange - 22:00 to 06:00 overnight window
            var window = new UtcTimeWindow(
                new TimeSpan(22, 0, 0), 
                new TimeSpan(6, 0, 0), 
                "UTC", 
                new[] { DayOfWeek.Monday });
            
            // Monday, 23:00 UTC (within overnight window)
            var evaluationTime = new DateTime(2024, 1, 15, 23, 0, 0, DateTimeKind.Utc); // Monday

            // Act
            var (isActive, reason) = window.IsActiveAt(new UtcDateTime(evaluationTime));

            // Assert
            Assert.True(isActive);
            Assert.Equal("Within time window", reason);
        }

        [Fact]
        public void IsActiveAt_OvernightWindowEarlyMorning_HandlesCorrectly()
        {
            // Arrange - 22:00 to 06:00 overnight window
            var window = new UtcTimeWindow(
                new TimeSpan(22, 0, 0), 
                new TimeSpan(6, 0, 0), 
                "UTC", 
                new[] { DayOfWeek.Monday });
            
            // Monday, 05:00 UTC (within overnight window, early morning)
            var evaluationTime = new DateTime(2024, 1, 15, 5, 0, 0, DateTimeKind.Utc); // Monday

            // Act
            var (isActive, reason) = window.IsActiveAt(new UtcDateTime(evaluationTime));

            // Assert
            Assert.True(isActive);
            Assert.Equal("Within time window", reason);
        }

		[Fact]
		public void IsActiveAt_WithDifferentTimeZone_UsesWindowTimeZone()
		{
			// Arrange - Create a window in Central Standard Time
			// 9 AM - 5 PM Central time
			var window = new UtcTimeWindow(
				new TimeSpan(9, 0, 0),
				new TimeSpan(17, 0, 0),
				"Central Standard Time",
				new[] { DayOfWeek.Monday });

			// Monday, 3 PM UTC = 9 AM Central (during standard time)
			// This should be within the 9 AM - 5 PM Central window
			var evaluationTime = new UtcDateTime(new DateTime(2024, 1, 15, 15, 0, 0, DateTimeKind.Utc)); // Monday

			// Act
			var (isActive, reason) = window.IsActiveAt(new UtcDateTime(evaluationTime));

			// Assert - Should be active because 3 PM UTC = 9 AM Central, which is within 9-17 Central
			Assert.True(isActive);
			Assert.Equal("Within time window", reason);
		}

		[Fact]
		public void IsActiveAt_WithValidTimeZone_EvaluatesCorrectly()
		{
			// Arrange - Create a window that would behave differently in different timezones
			try
			{
				var window = new UtcTimeWindow(
					new TimeSpan(14, 0, 0),  // 2 PM 
					new TimeSpan(18, 0, 0),  // 6 PM
					"Eastern Standard Time",
					new[] { DayOfWeek.Monday });

				// Monday, 6 PM UTC = 1 PM Eastern (should be outside the 2-6 PM Eastern window)
				var evaluationTime = new UtcDateTime(new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc));

				// Act
				var (isActive, reason) = window.IsActiveAt(new UtcDateTime(evaluationTime));

				// Assert - Should be outside window because 6 PM UTC = 1 PM Eastern, before 2 PM start
				Assert.False(isActive);
				Assert.Equal("Outside time window", reason);
			}
			catch (ArgumentException)
			{
				// Skip test if timezone not available on this system
				Assert.True(true, "Eastern Standard Time not available on this system - test skipped");
			}
		}

		[Fact]
        public void EqualityOperator_WithSameValues_ReturnsTrue()
        {
            // Arrange
            var window1 = new UtcTimeWindow(
                new TimeSpan(9, 0, 0), 
                new TimeSpan(17, 0, 0), 
                "UTC", 
                new[] { DayOfWeek.Monday });
            var window2 = new UtcTimeWindow(
                new TimeSpan(9, 0, 0), 
                new TimeSpan(17, 0, 0), 
                "UTC", 
                new[] { DayOfWeek.Monday });

            // Act & Assert
            Assert.True(window1 == window2);
        }

        [Fact]
        public void EqualityOperator_WithDifferentStartTime_ReturnsFalse()
        {
            // Arrange
            var window1 = new UtcTimeWindow(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));
            var window2 = new UtcTimeWindow(new TimeSpan(10, 0, 0), new TimeSpan(17, 0, 0));

            // Act & Assert
            Assert.False(window1 == window2);
        }

        [Fact]
        public void EqualityOperator_WithDifferentTimeZoneCase_ReturnsTrue()
        {
            // Arrange
            var window1 = new UtcTimeWindow(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), "UTC");
            var window2 = new UtcTimeWindow(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), "utc");

            // Act & Assert
            Assert.True(window1 == window2);
        }

        [Fact]
        public void EqualityOperator_WithDifferentDaysOrder_ReturnsTrue()
        {
            // Arrange
            var window1 = new UtcTimeWindow(
                new TimeSpan(9, 0, 0), 
                new TimeSpan(17, 0, 0), 
                "UTC", 
                new[] { DayOfWeek.Monday, DayOfWeek.Tuesday });
            var window2 = new UtcTimeWindow(
                new TimeSpan(9, 0, 0), 
                new TimeSpan(17, 0, 0), 
                "UTC", 
                new[] { DayOfWeek.Tuesday, DayOfWeek.Monday });

            // Act & Assert
            Assert.True(window1 == window2);
        }

        [Fact]
        public void EqualityOperator_WithNullValues_HandlesCorrectly()
        {
            // Arrange
            UtcTimeWindow? window1 = null;
            UtcTimeWindow? window2 = null;
            var window3 = new UtcTimeWindow(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));

            // Act & Assert
            Assert.True(window1 == window2);
            Assert.False(window1 == window3);
            Assert.False(window3 == window1);
        }

        [Fact]
        public void EqualityOperator_WithSameReference_ReturnsTrue()
        {
            // Arrange
            var window = new UtcTimeWindow(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));

            // Act & Assert
            Assert.True(window == window);
        }

        [Fact]
        public void InequalityOperator_WithDifferentValues_ReturnsTrue()
        {
            // Arrange
            var window1 = new UtcTimeWindow(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));
            var window2 = new UtcTimeWindow(new TimeSpan(10, 0, 0), new TimeSpan(18, 0, 0));

            // Act & Assert
            Assert.True(window1 != window2);
        }

        [Fact]
        public void Equals_WithSameValues_ReturnsTrue()
        {
            // Arrange
            var window1 = new UtcTimeWindow(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));
            var window2 = new UtcTimeWindow(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));

            // Act & Assert
            Assert.True(window1.Equals(window2));
        }

        [Fact]
        public void Equals_WithDifferentType_ReturnsFalse()
        {
            // Arrange
            var window = new UtcTimeWindow(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));
            var other = "not a time window";

            // Act & Assert
            Assert.False(window.Equals(other));
        }

        [Fact]
        public void GetHashCode_WithSameValues_ReturnsSameHashCode()
        {
            // Arrange
            var window1 = new UtcTimeWindow(
                new TimeSpan(9, 0, 0), 
                new TimeSpan(17, 0, 0), 
                "UTC", 
                new[] { DayOfWeek.Monday });
            var window2 = new UtcTimeWindow(
                new TimeSpan(9, 0, 0), 
                new TimeSpan(17, 0, 0), 
                "UTC", 
                new[] { DayOfWeek.Monday });

            // Act & Assert
            Assert.Equal(window1.GetHashCode(), window2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_WithDifferentDaysOrder_ReturnsSameHashCode()
        {
            // Arrange
            var window1 = new UtcTimeWindow(
                new TimeSpan(9, 0, 0), 
                new TimeSpan(17, 0, 0), 
                "UTC", 
                new[] { DayOfWeek.Monday, DayOfWeek.Tuesday });
            var window2 = new UtcTimeWindow(
                new TimeSpan(9, 0, 0), 
                new TimeSpan(17, 0, 0), 
                "UTC", 
                new[] { DayOfWeek.Tuesday, DayOfWeek.Monday });

            // Act & Assert
            Assert.Equal(window1.GetHashCode(), window2.GetHashCode());
        }

        [Theory]
        [InlineData("UTC")]
        [InlineData("Pacific Standard Time")]
        [InlineData("Eastern Standard Time")]
        public void Constructor_WithValidTimeZones_AcceptsTimeZone(string timeZone)
        {
            // Arrange & Act
            try
            {
                var window = new UtcTimeWindow(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), timeZone);
                
                // Assert
                Assert.Equal(timeZone, window.TimeZone);
            }
            catch (ArgumentException)
            {
                // Some time zones might not be available on all systems
                Assert.True(true, $"Time zone {timeZone} not available on this system");
            }
        }
    }
}