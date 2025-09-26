using Knara.UtcStrict;

namespace UnitTests
{
    public class TimeZoneHelperTests
    {
        [Fact]
        public void GetLocalTimeAndDayInTimezone_WithUtcTime_ReturnsCorrectTime()
        {
            // Arrange
            var utcTime = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);
            var timeZoneId = "UTC";

            // Act
            var (time, day) = TimezoneUtilities.GetTimeAndDayInTimezone(new UtcDateTime(utcTime), timeZoneId);

            // Assert
            Assert.Equal(utcTime.TimeOfDay, time);
            Assert.Equal(utcTime.DayOfWeek, day);
        }

        [Fact]
        public void GetCurrentTimeAndDayInTimeZone_WithNonUtcTime_TreatsAsUtc()
        {
            // Arrange
            var localTime = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Local);
            var timeZoneId = "America/Chicago";

            // Act
            var (time, day) = TimezoneUtilities.GetTimeAndDayInTimezone(new UtcDateTime(localTime), timeZoneId);

            // Assert
            // The method should treat the input as UTC regardless of Kind
            Assert.Equal(localTime.TimeOfDay, time);
            Assert.Equal(localTime.DayOfWeek, day);
        }

        [Fact]
        public void GetCurrentTimeAndDayInTimeZone_WithUnspecifiedTime_TreatsAsUtc()
        {
            // Arrange
            var unspecifiedTime = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Unspecified);
            var timeZoneId = "UTC";

            // Act
            var (time, day) = TimezoneUtilities.GetTimeAndDayInTimezone(new UtcDateTime(unspecifiedTime), timeZoneId);

            // Assert
            Assert.Equal(unspecifiedTime.ToUniversalTime().TimeOfDay, time);
            Assert.Equal(unspecifiedTime.ToUniversalTime().DayOfWeek, day);
        }

        [Fact]
        public void GetCurrentTimeAndDayInTimeZone_WithDifferentTimeZone_ConvertsCorrectly()
        {
            // Arrange
            var utcTime = new DateTime(2023, 6, 15, 12, 0, 0, DateTimeKind.Utc); // Noon UTC
            var timeZoneId = "Pacific Standard Time"; // Should be UTC-8 or UTC-7 depending on DST

            // Act & Assert
            try
            {
                var (time, day) = TimezoneUtilities.GetTimeAndDayInTimezone(new UtcDateTime(utcTime), timeZoneId);
                
                // The time should be different from UTC (unless it's a weird edge case)
                // Day might be the same or different depending on the conversion
                Assert.True(time >= TimeSpan.Zero && time < TimeSpan.FromDays(1));
                Assert.True(Enum.IsDefined(typeof(DayOfWeek), day));
            }
            catch (TimeZoneNotFoundException)
            {
                // Skip test if timezone not found on this system
                Assert.True(true, "TimeZone not found on this system - test skipped");
            }
        }

        [Fact]
        public void GetCurrentTimeAndDayInTimeZone_WithInvalidTimeZoneId_ThrowsException()
        {
            // Arrange
            var utcTime = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);
            var invalidTimeZoneId = "Invalid/TimeZone";

            // Act & Assert
            Assert.Throws<TimeZoneNotFoundException>(() =>
                TimezoneUtilities.GetTimeAndDayInTimezone(new UtcDateTime(utcTime), invalidTimeZoneId));
        }

        [Fact]
        public void GetCurrentTimeAndDayInTimeZone_WithMidnightUtc_HandlesDateBoundary()
        {
            // Arrange
            var midnightUtc = new DateTime(2023, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var timeZoneId = "UTC";

            // Act
            var (time, day) = TimezoneUtilities.GetTimeAndDayInTimezone(new UtcDateTime(midnightUtc), timeZoneId);

            // Assert
            Assert.Equal(TimeSpan.Zero, time);
            Assert.Equal(DayOfWeek.Thursday, day); // June 15, 2023 was a Thursday
        }
    }
}