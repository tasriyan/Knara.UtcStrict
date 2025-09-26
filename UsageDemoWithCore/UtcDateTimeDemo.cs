using Knara.UtcStrict;

namespace UsageDemoWithCore;

internal class UtcDateTimeDemo
{
	public void RunDemo()
	{
		Console.WriteLine("=== UtcDateTime Demo ===\n");

		// The Problem: DateTime chaos in mixed environments
		Console.WriteLine("THE PROBLEM - DateTime Chaos:");
		var dbValue = new DateTime(2024, 6, 15, 14, 30, 0);		// From database - what timezone?
		var localInput = DateTime.Now;							// From user input - local time
		var apiValue = DateTime.UtcNow;							// From API - already UTC

		Console.WriteLine($"Database:   {dbValue} (Kind: {dbValue.Kind}) - What timezone is this?");
		Console.WriteLine($"User Input: {localInput} (Kind: {localInput.Kind}) - Local to server");
		Console.WriteLine($"API:        {apiValue} (Kind: {apiValue.Kind}) - Already UTC\n");

		// The Solution: UtcDateTime normalizes everything
		Console.WriteLine("THE SOLUTION - UtcDateTime Normalization:");
		UtcDateTime utcFromDb = (UtcDateTime)dbValue;		// Assumes server timezone for Unspecified
		UtcDateTime utcFromLocal = (UtcDateTime)localInput; // Converts from local
		UtcDateTime utcFromApi = (UtcDateTime)apiValue;		// Preserves UTC

		Console.WriteLine($"Database -> UTC:   {utcFromDb.DateTime}");
		Console.WriteLine($"Local -> UTC:      {utcFromLocal.DateTime}");
		Console.WriteLine($"API -> UTC:        {utcFromApi.DateTime}\n");

		// Safe Comparisons: Now all times are in UTC
		Console.WriteLine("SAFE COMPARISONS:");
		Console.WriteLine($"All normalized to UTC - safe to compare/store/process");
		Console.WriteLine($"DB < Local: {utcFromDb < utcFromLocal}");
		Console.WriteLine($"Local == API: {utcFromLocal == utcFromApi}\n");

		// DateTimeOffset Support
		Console.WriteLine("DATEOFFSET SUPPORT:");
		var easternTime = new DateTimeOffset(2024, 6, 15, 10, 0, 0, TimeSpan.FromHours(-4)); // 10 AM EDT
		UtcDateTime utcFromOffset = easternTime;
		Console.WriteLine($"EDT 10:00 AM -> UTC: {utcFromOffset.DateTime}\n");

		Console.WriteLine("=== Key Point: No more timezone bugs! ===");
	}
}