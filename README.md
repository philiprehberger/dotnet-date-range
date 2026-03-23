# Philiprehberger.DateRange

[![CI](https://github.com/philiprehberger/dotnet-date-range/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-date-range/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.DateRange)](https://www.nuget.org/packages/Philiprehberger.DateRange)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-date-range)](LICENSE)

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
var ranges = new[] { range1, range2, range3 };
var merged = ranges.Merge(); // Combines overlapping/adjacent ranges
```

### Finding Gaps

```csharp
var gaps = ranges.FindGaps(); // Returns unoccupied intervals between ranges
```

## API

| Method | Description |
|--------|-------------|
| `DateRange(start, end)` | Create a new date range |
| `Duration` | Get the duration of the range |
| `Overlaps(DateRange)` | Check if two ranges overlap |
| `Contains(DateTimeOffset)` | Check if a point is within the range |
| `Contains(DateRange)` | Check if a range is fully contained |
| `Intersection(DateRange)` | Get the overlapping portion of two ranges |
| `Union(DateRange)` | Merge two overlapping ranges into one |
| `Gap(DateRange)` | Get the gap between two non-overlapping ranges |
| `IEnumerable<DateRange>.Merge()` | Merge all overlapping/adjacent ranges |
| `IEnumerable<DateRange>.FindGaps()` | Find all gaps between ranges |
| `IEnumerable<DateRange>.AnyOverlap()` | Check if any ranges overlap |

## Development

```bash
dotnet build src/Philiprehberger.DateRange.csproj --configuration Release
```

## License

MIT
