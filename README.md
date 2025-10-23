# Knara.UtcStrict
[![Build and Test](https://github.com/tasriyan/Knara.UtcStrict/actions/workflows/build.yml/badge.svg)](https://github.com/tasriyan/Knara.UtcStrict/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Knara.UtcStrict.svg)](https://www.nuget.org/packages/Knara.UtcStrict/)
[![NuGet](https://img.shields.io/nuget/dt/Knara.UtcStrict.svg)](https://www.nuget.org/packages/Knara.UtcStrict/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Opinionated library that enforces UTC time handling in .NET applications.

## The Problem

Mixed environments with legacy systems create DateTime chaos:

- Database stores some timestamps as UTC, others as local time
- APIs return DateTime vs DateTimeOffset inconsistently  
- Frontend sends local or UTC time depending on implementation
- Converting already-UTC DateTime to UTC again produces wrong results
- `DateTime.ToUniversalTime()` makes assumptions about timezone that are often wrong

## The Solution

**UtcStrict** is an opinionated library that forces developers to work exclusively in UTC, eliminating bugs caused by mixing `DateTime`, `DateTimeOffset`, `DateTime.Now`, and `DateTime.UtcNow` throughout the codebase.

The library normalizes all datetime values to UTC and provides convenient business logic types (`UtcSchedule` and `UtcTimeWindow`) that handle common scheduling scenarios without boilerplate code or timezone-related bugs.

## Installation

```bash
dotnet add package Knara.UtcStrict
```

## Requirements

- .NET Standard 2.0 or later
- No external dependencies

## Core Types

### UtcDateTime

Enforces UTC timezone on all DateTime values:

```csharp
// All these become UTC automatically
UtcDateTime utcFromLocal = DateTime.Now;           // Converts local → UTC  
UtcDateTime utcFromOffset = DateTimeOffset.Now;    // Extracts UTC portion
UtcDateTime utcFromDb = dbDateTime;                // Assumes server timezone

// Safe comparisons - everything is UTC
if (utcFromDb < utcFromLocal) { /* ... */ }

// Explicit timezone conversion when you know the source
var easternTime = new DateTime(2024, 6, 15, 14, 30, 0);
var utc = new UtcDateTime(easternTime, "Eastern Standard Time");
```

### UtcSchedule

Enable/disable features on a schedule:

```csharp
// Create new schedule (must be future)
var schedule = UtcSchedule.CreateSchedule(
    enableOn: new UtcDateTime(DateTime.UtcNow.AddHours(1)),
    disableOn: new UtcDateTime(DateTime.UtcNow.AddDays(7))
);

// Load existing schedule from database (any dates allowed)
var existing = new UtcSchedule(enabledDate, disabledDate);

// Check if feature should be active
var (isActive, reason) = schedule.IsActiveAt(UtcDateTime.UtcNow);
if (isActive) 
{
    // Feature is enabled
}

// Unscheduled = always inactive
var unscheduled = UtcSchedule.Unscheduled;
Console.WriteLine(unscheduled.HasSchedule()); // False
```

### UtcTimeWindow

Control service availability by time of day and timezone:

```csharp
// Business hours in Eastern timezone
var businessHours = new UtcTimeWindow(
    startOn: new TimeSpan(9, 0, 0),    // 9 AM
    stopOn: new TimeSpan(17, 0, 0),    // 5 PM
    timeZone: "Eastern Standard Time",
    daysActive: new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, 
                       DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }
);

// Check availability (UTC time gets converted to Eastern automatically)
var (isOpen, reason) = businessHours.IsActiveAt(UtcDateTime.UtcNow);

// Overnight window (10 PM to 6 AM)
var maintenance = new UtcTimeWindow(
    startOn: new TimeSpan(22, 0, 0),   // 10 PM
    stopOn: new TimeSpan(6, 0, 0)      // 6 AM next day
);

// Always available
var alwaysOpen = UtcTimeWindow.AlwaysOpen;
Console.WriteLine(alwaysOpen.HasWindow()); // False (no restrictions)
```

## Usage Scenarios

**API Controllers:**
```csharp
public class EventController : ControllerBase
{
    public IActionResult CreateEvent(CreateEventRequest request)
    {
        // Frontend might send local time - normalize to UTC
        var eventTime = new UtcDateTime(request.EventDate);
        
        var evt = new Event 
        { 
            StartTime = eventTime.DateTime, // Always UTC in database
            Schedule = request.EnableSchedule 
                ? UtcSchedule.CreateSchedule(eventTime, eventTime.DateTime.AddHours(4))
                : UtcSchedule.Unscheduled
        };
        
        return Ok(evt);
    }
}
```

**Database Queries:**
```csharp
public class EventRepository
{
    public List<Event> GetActiveEvents()
    {
        var now = UtcDateTime.UtcNow;
        
        return context.Events
            .Where(e => e.StartTime <= now.DateTime && e.EndTime >= now.DateTime)
            .ToList();
    }
    
    public Event LoadEvent(int id)
    {
        var evt = context.Events.Find(id);
        
        // Convert database DateTime to UtcDateTime for business logic
        evt.NormalizedStartTime = new UtcDateTime(evt.StartTime);
        return evt;
    }
}
```

**Business Logic:**
```csharp
public class FeatureService
{
    public bool IsFeatureEnabled(string featureId, UtcDateTime atTime)
    {
        var feature = GetFeature(featureId);
        
        // Check schedule
        var (scheduleActive, _) = feature.Schedule.IsActiveAt(atTime);
        if (!scheduleActive) return false;
        
        // Check time window
        var (windowActive, _) = feature.TimeWindow.IsActiveAt(atTime);
        return windowActive;
    }
}
```

## Key Benefits

- **Prevents double-conversion bugs** - detects already-UTC values
- **Handles mixed input sources** - DateTime, DateTimeOffset, database values
- **Timezone-aware business logic** - schedules and time windows work across timezones
- **Implicit conversions** - works with existing DateTime code
- **Clear business intent** - `HasSchedule()` and `HasWindow()` methods

## License
MIT License. Copyright 2025 Tatyana Asriyan
