using Knara.UtcStrict;
using UsageDemoWithCore;

Console.WriteLine("=== Knara.UtcStrict Library Demos ===\n");

// Run UtcDateTime Demo
var utcDateTimeDemo = new UtcDateTimeDemo();
utcDateTimeDemo.RunDemo();

Console.WriteLine("\n" + new string('=', 50) + "\n");

// Run UtcSchedule Demo
var scheduleDemo = new UtcScheduleDemo();
scheduleDemo.RunDemo();

Console.WriteLine("\n" + new string('=', 50) + "\n");

// Run TimeWindow Demo
var timeWindowDemo = new TimeWindowDemo();
timeWindowDemo.RunDemo();

Console.WriteLine("\n=== All Demos Complete ===");

