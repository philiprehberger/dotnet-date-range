# Philiprehberger.DateRange

[![CI](https://github.com/philiprehberger/dotnet-date-range/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-date-range/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.DateRange.svg)](https://www.nuget.org/packages/Philiprehberger.DateRange)
[![Last updated](https://img.shields.io/github/last-commit/philiprehberger/dotnet-date-range)](https://github.com/philiprehberger/dotnet-date-range/commits/main)

Immutable date/time range type with overlap detection, intersection, union, gap finding, and splitting.

## Installation

```bash
dotnet add package Philiprehberger.DateRange
```

## Usage

```csharp
using Philiprehberger.DateRange;

var meeting = new DateRange(
    new DateTimeOffset(2026, 3, 22, 9, 0, 0, TimeSpan.Zero),
    new DateTimeOffset(2026, 3, 22, 10, 0, 0, TimeSpan.Zero));

var lunch = new DateRange(
    new DateTimeOffset(2026, 3, 22, 11, 30, 0, TimeSpan.Zero),
    new DateTimeOffset(2026, 3, 22, 12, 30, 0, TimeSpan.Zero));

Console.WriteLine(meeting.Overlaps(lunch)); // False
Console.WriteLine(meeting.Duration);        // 01:00:00
```

### Overlap and Intersection

```csharp
using Philiprehberger.DateRange;

var a = new DateRange(start1, end1);
var b = new DateRange(start2, end2);

if (a.Overlaps(b))
{
    var overlap = a.Intersection(b);
    Console.WriteLine($"Overlap: {overlap!.Value.Duration}");
}
```

### Merging Ranges

```csharp
using Philiprehberger.DateRange;

var ranges = new[] { range1, range2, range3 };
var merged = DateRange.MergeAll(ranges); // Minimal non-overlapping set
```

### Finding Gaps

```csharp
using Philiprehberger.DateRange;

var gaps = ranges.FindGaps(); // Returns unoccupied intervals between ranges
```

### Business Days

```csharp
using Philiprehberger.DateRange;

var sprint = DateRange.Create(
    new DateTimeOffset(2026, 3, 23, 0, 0, 0, TimeSpan.Zero),
    new DateTimeOffset(2026, 4, 6, 0, 0, 0, TimeSpan.Zero));

int workdays = sprint.BusinessDays(); // Excludes weekends
var weekdayRanges = sprint.ExcludeWeekends(); // Sub-ranges for weekdays only
```

### Duration Comparisons

```csharp
using Philiprehberger.DateRange;

var range = DateRange.Create(start, end);

bool isQuick = range.IsShorterThan(TimeSpan.FromHours(1));
bool isLong = range.IsLongerThan(TimeSpan.FromDays(7));
bool exactDay = range.DurationEquals(TimeSpan.FromDays(1));
```

## API

| Method | Description |
|--------|-------------|
| `DateRange(start, end)` | Create a new date range |
| `DateRange.Create(start, end)` | Create a validated date range (throws if start >= end) |
| `DateRange.MergeAll(ranges)` | Merge overlapping ranges into minimal non-overlapping set |
| `Duration` | Get the duration of the range |
| `Overlaps(DateRange)` | Check if two ranges overlap |
| `Contains(DateTimeOffset)` | Check if a point is within the range |
| `Contains(DateRange)` | Check if a range is fully contained |
| `Intersection(DateRange)` | Get the overlapping portion of two ranges |
| `Union(DateRange)` | Merge two overlapping ranges into one |
| `Gap(DateRange)` | Get the gap between two non-overlapping ranges |
| `IsAdjacent(DateRange)` | Check if two ranges are adjacent |
| `Split(TimeSpan)` | Split a range into segments of a given duration |
| `BusinessDays()` | Count weekdays within the range excluding weekends |
| `ExcludeWeekends()` | Return sub-ranges for weekday-only portions |
| `IsShorterThan(TimeSpan)` | Check if the range duration is less than a time span |
| `IsLongerThan(TimeSpan)` | Check if the range duration is greater than a time span |
| `DurationEquals(TimeSpan)` | Check if the range duration equals a time span |
| `IEnumerable<DateRange>.Merge()` | Merge all overlapping/adjacent ranges |
| `IEnumerable<DateRange>.FindGaps()` | Find all gaps between ranges |
| `IEnumerable<DateRange>.AnyOverlap()` | Check if any ranges overlap |
| `IEnumerable<DateRange>.TotalDuration()` | Get total covered duration accounting for overlaps |

## Development

```bash
dotnet build src/Philiprehberger.DateRange.csproj --configuration Release
```

## Support

If you find this project useful:

⭐ [Star the repo](https://github.com/philiprehberger/dotnet-date-range)

🐛 [Report issues](https://github.com/philiprehberger/dotnet-date-range/issues?q=is%3Aissue+is%3Aopen+label%3Abug)

💡 [Suggest features](https://github.com/philiprehberger/dotnet-date-range/issues?q=is%3Aissue+is%3Aopen+label%3Aenhancement)

❤️ [Sponsor development](https://github.com/sponsors/philiprehberger)

🌐 [All Open Source Projects](https://philiprehberger.com/open-source-packages)

💻 [GitHub Profile](https://github.com/philiprehberger)

🔗 [LinkedIn Profile](https://www.linkedin.com/in/philiprehberger)

## License

[MIT](LICENSE)
