namespace Philiprehberger.DateRange;

/// <summary>
/// Extension methods for working with collections of <see cref="DateRange"/> values.
/// </summary>
public static class DateRangeExtensions
{
    /// <summary>
    /// Merges all overlapping and adjacent ranges into a minimal set of non-overlapping ranges.
    /// </summary>
    /// <param name="ranges">The ranges to merge.</param>
    /// <returns>A list of merged, non-overlapping ranges sorted by start time.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ranges"/> is null.</exception>
    public static List<DateRange> Merge(this IEnumerable<DateRange> ranges)
    {
        ArgumentNullException.ThrowIfNull(ranges);

        var sorted = ranges.OrderBy(r => r.Start).ThenBy(r => r.End).ToList();

        if (sorted.Count == 0)
            return [];

        var result = new List<DateRange> { sorted[0] };

        for (int i = 1; i < sorted.Count; i++)
        {
            var current = sorted[i];
            var last = result[^1];

            if (last.Overlaps(current) || last.IsAdjacent(current))
            {
                // Merge: extend the last range
                var mergedEnd = last.End > current.End ? last.End : current.End;
                result[^1] = new DateRange(last.Start, mergedEnd);
            }
            else
            {
                result.Add(current);
            }
        }

        return result;
    }

    /// <summary>
    /// Finds all gaps (unoccupied intervals) between the given ranges.
    /// Ranges are first merged to eliminate overlaps before gap detection.
    /// </summary>
    /// <param name="ranges">The ranges to find gaps between.</param>
    /// <returns>A list of gap ranges sorted by start time.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ranges"/> is null.</exception>
    public static List<DateRange> FindGaps(this IEnumerable<DateRange> ranges)
    {
        ArgumentNullException.ThrowIfNull(ranges);

        var merged = ranges.Merge();
        var gaps = new List<DateRange>();

        for (int i = 1; i < merged.Count; i++)
        {
            var gap = merged[i - 1].Gap(merged[i]);
            if (gap.HasValue)
            {
                gaps.Add(gap.Value);
            }
        }

        return gaps;
    }

    /// <summary>
    /// Checks whether any of the given ranges overlap with each other.
    /// </summary>
    /// <param name="ranges">The ranges to check.</param>
    /// <returns>True if at least two ranges overlap.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ranges"/> is null.</exception>
    public static bool AnyOverlap(this IEnumerable<DateRange> ranges)
    {
        ArgumentNullException.ThrowIfNull(ranges);

        var sorted = ranges.OrderBy(r => r.Start).ThenBy(r => r.End).ToList();

        for (int i = 1; i < sorted.Count; i++)
        {
            if (sorted[i - 1].Overlaps(sorted[i]))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Computes the total covered duration of the given ranges, accounting for overlaps.
    /// Overlapping portions are counted only once.
    /// </summary>
    /// <param name="ranges">The ranges to compute total duration for.</param>
    /// <returns>The total covered duration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ranges"/> is null.</exception>
    public static TimeSpan TotalDuration(this IEnumerable<DateRange> ranges)
    {
        ArgumentNullException.ThrowIfNull(ranges);

        var merged = ranges.Merge();
        var total = TimeSpan.Zero;

        foreach (var range in merged)
        {
            total += range.Duration;
        }

        return total;
    }
}
