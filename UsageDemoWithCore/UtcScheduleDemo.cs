using Knara.UtcStrict;

namespace UsageDemoWithCore;

internal class UtcScheduleDemo
{
	public void RunDemo()
	{
		Console.WriteLine("=== UtcSchedule Demo ===\n");

		// Use Case: Enable/disable features on a schedule
		Console.WriteLine("BUSINESS SCENARIO: Feature rollout scheduling\n");

		// 1. No schedule = feature disabled
		Console.WriteLine("1. Unscheduled Feature:");
		var unscheduled = UtcSchedule.Unscheduled;
		var (active1, reason1) = unscheduled.IsActiveAt(UtcDateTime.UtcNow);
		Console.WriteLine($"   Has Schedule: {unscheduled.HasSchedule()}");
		Console.WriteLine($"   Active Now: {active1} - {reason1}\n");

		// 2. Create new schedule (must be future)
		Console.WriteLine("2. Create New Feature Schedule:");
		try
		{
			var enableAt = new UtcDateTime(DateTime.UtcNow.AddHours(1));
			var disableAt = new UtcDateTime(DateTime.UtcNow.AddHours(24));

			var newSchedule = UtcSchedule.CreateSchedule(enableAt, disableAt);
			var (active2, reason2) = newSchedule.IsActiveAt(UtcDateTime.UtcNow);
			Console.WriteLine($"   Enable:  {enableAt.DateTime}");
			Console.WriteLine($"   Disable: {disableAt.DateTime}");
			Console.WriteLine($"   Active Now: {active2} - {reason2}\n");
		}
		catch (ArgumentException ex)
		{
			Console.WriteLine($"   Error: {ex.Message}\n");
		}

		// 3. Load existing schedule from database (can be any dates)
		Console.WriteLine("3. Active Schedule (loaded from DB):");
		var enabledYesterday = new UtcDateTime(DateTime.UtcNow.AddHours(-12));
		var disablesTomorrow = new UtcDateTime(DateTime.UtcNow.AddHours(12));

		var activeSchedule = new UtcSchedule(enabledYesterday, disablesTomorrow);
		var (active3, reason3) = activeSchedule.IsActiveAt(UtcDateTime.UtcNow);
		Console.WriteLine($"   Enable:  {enabledYesterday.DateTime}");
		Console.WriteLine($"   Disable: {disablesTomorrow.DateTime}");
		Console.WriteLine($"   Active Now: {active3} - {reason3}\n");

		// 4. Expired schedule
		Console.WriteLine("4. Expired Schedule:");
		var expiredSchedule = new UtcSchedule(
			new UtcDateTime(DateTime.UtcNow.AddHours(-24)),
			new UtcDateTime(DateTime.UtcNow.AddHours(-1))
		);
		var (active4, reason4) = expiredSchedule.IsActiveAt(UtcDateTime.UtcNow);
		Console.WriteLine($"   Active Now: {active4} - {reason4}\n");

		// 5. Different timezone inputs - all normalized to UTC
		Console.WriteLine("5. Timezone Normalization:");
		var easternStart = new DateTimeOffset(2024, 6, 15, 9, 0, 0, TimeSpan.FromHours(-4)); // 9 AM EDT
		var pacificEnd = new DateTimeOffset(2024, 6, 15, 18, 0, 0, TimeSpan.FromHours(-7));   // 6 PM PDT

		var timezoneSchedule = new UtcSchedule(easternStart, pacificEnd);
		Console.WriteLine($"   EDT 9 AM → UTC: {timezoneSchedule.EnableOn.DateTime}");
		Console.WriteLine($"   PDT 6 PM → UTC: {timezoneSchedule.DisableOn.DateTime}\n");

		Console.WriteLine("=== Key Point: All schedules in UTC, no timezone confusion! ===");
	}
}