using Knara.UtcStrict;

namespace UsageDemoWithCore;

internal class TimeWindowDemo
{
	public void RunDemo()
	{
		Console.WriteLine("=== UtcTimeWindow Demo ===\n");

		// Use Case: Control when services/features are available
		Console.WriteLine("BUSINESS SCENARIO: Service availability windows\n");

		var now = UtcDateTime.UtcNow;

		// 1. Always available service
		Console.WriteLine("1. Always Available Service:");
		var alwaysOpen = UtcTimeWindow.AlwaysOpen;
		var (active1, reason1) = alwaysOpen.IsActiveAt(now);
		Console.WriteLine($"   Has Window Restrictions: {alwaysOpen.HasWindow()}");
		Console.WriteLine($"   Available Now: {active1} - {reason1}\n");

		// 2. Business hours (9 AM - 5 PM, weekdays only)
		Console.WriteLine("2. Business Hours Service:");
		var businessHours = new UtcTimeWindow(
			startOn: new TimeSpan(9, 0, 0),    // 9 AM
			stopOn: new TimeSpan(17, 0, 0),    // 5 PM
			timeZone: "UTC",
			daysActive: [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
						DayOfWeek.Thursday, DayOfWeek.Friday]
		);

		var (active2, reason2) = businessHours.IsActiveAt(now);
		Console.WriteLine($"   Hours: {businessHours.StartOn} - {businessHours.StopOn}");
		Console.WriteLine($"   Days: Weekdays only");
		Console.WriteLine($"   Has Window Restrictions: {businessHours.HasWindow()}");
		Console.WriteLine($"   Available Now: {active2} - {reason2}\n");

		// 3. Overnight maintenance window (10 PM - 6 AM)
		Console.WriteLine("3. Overnight Maintenance Window:");
		var maintenance = new UtcTimeWindow(
			startOn: new TimeSpan(22, 0, 0),   // 10 PM
			stopOn: new TimeSpan(6, 0, 0)      // 6 AM (next day)
		);

		var (active3, reason3) = maintenance.IsActiveAt(now);
		Console.WriteLine($"   Hours: {maintenance.StartOn} - {maintenance.StopOn} (overnight)");
		Console.WriteLine($"   Available Now: {active3} - {reason3}\n");

		// 4. The Key Feature: Timezone handling
		Console.WriteLine("4. Timezone Handling (Key Feature):");
		try
		{
			// Same business hours, but in Eastern timezone
			var easternHours = new UtcTimeWindow(
				startOn: new TimeSpan(9, 0, 0),    // 9 AM Eastern
				stopOn: new TimeSpan(17, 0, 0),    // 5 PM Eastern
				timeZone: "Eastern Standard Time",
				daysActive: [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
							DayOfWeek.Thursday, DayOfWeek.Friday]
			);

			var (activeEastern, reasonEastern) = easternHours.IsActiveAt(now);
			Console.WriteLine($"   Same hours in Eastern timezone:");
			Console.WriteLine($"   Available Now: {activeEastern} - {reasonEastern}");
			Console.WriteLine($"   vs UTC business hours: {active2} - {reason2}\n");
		}
		catch (ArgumentException ex)
		{
			Console.WriteLine($"   Eastern timezone not available: {ex.Message}\n");
		}

		// 5. Weekend only service
		Console.WriteLine("5. Weekend Only Service:");
		var weekendOnly = new UtcTimeWindow(
			startOn: TimeSpan.Zero,             // All day
			stopOn: new TimeSpan(23, 59, 59),
			daysActive: [DayOfWeek.Saturday, DayOfWeek.Sunday]
		);

		var (active4, reason4) = weekendOnly.IsActiveAt(now);
		Console.WriteLine($"   Days: Weekends only");
		Console.WriteLine($"   Has Window Restrictions: {weekendOnly.HasWindow()} (day restrictions)");
		Console.WriteLine($"   Available Now: {active4} - {reason4}\n");

		Console.WriteLine("=== Key Point: Timezone-aware availability windows! ===");
	}
}