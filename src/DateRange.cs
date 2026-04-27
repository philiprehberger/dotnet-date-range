namespace Philiprehberger.DateRange;

/// <summary>
/// An immutable date/time range defined by a start and end point.
/// Supports overlap detection, intersection, union, and gap computation.
/// </summary>
/// <param name="Start">The inclusive start of the range.</param>
/// <param name="End">The exclusive end of the range.</param>
public readonly record struct DateRange(DateTimeOffset Start, DateTimeOffset End) : IComparable<DateRange>
{
    /// <summary>
    /// Initializes a new <see cref="DateRange"/> and validates that Start is before End.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when Start is not before End.</exception>
    /// <summary>
    /// Creates a new DateRange with validation.
    /// </summary>
    public static DateRange Create(DateTimeOffset start, DateTimeOffset end)
    {
        if (start >= end)
            throw new ArgumentException("Start must be before End.", nameof(start));
        return new DateRange(start, end);
    }

    /// <summary>
    /// Gets the duration of this range.
    /// </summary>
    public TimeSpan Duration => End - Start;

    /// <summary>
    /// Checks whether this range overlaps with another range.
    /// Two ranges overlap if one starts before the other ends and vice versa.
    /// </summary>
    /// <param name="other">The other range to check.</param>
    /// <returns>True if the ranges overlap.</returns>
    public bool Overlaps(DateRange other) => Start < other.End && other.Start < End;

    /// <summary>
    /// Checks whether this range fully contains the specified point in time.
    /// The start is inclusive and the end is exclusive.
    /// </summary>
    /// <param name="point">The point in time to check.</param>
    /// <returns>True if the point is within [Start, End).</returns>
    public bool Contains(DateTimeOffset point) => point >= Start && point < End;

    /// <summary>
    /// Checks whether this range fully contains another range.
    /// </summary>
    /// <param name="other">The other range to check.</param>
    /// <returns>True if the other range is fully within this range.</returns>
    public bool Contains(DateRange other) => other.Start >= Start && other.End <= End;

    /// <summary>
    /// Computes the intersection (overlapping portion) of this range with another.
    /// </summary>
    /// <param name="other">The other range.</param>
    /// <returns>The intersection as a <see cref="DateRange"/>, or null if the ranges do not overlap.</returns>
    public DateRange? Intersection(DateRange other)
    {
        if (!Overlaps(other))
            return null;

        var start = Start > other.Start ? Start : other.Start;
        var end = End < other.End ? End : other.End;

        return new DateRange(start, end);
    }

    /// <summary>
    /// Computes the union (merged span) of this range with an overlapping or adjacent range.
    /// </summary>
    /// <param name="other">The other range. Must overlap or be adjacent.</param>
    /// <returns>A single <see cref="DateRange"/> covering both ranges.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the ranges do not overlap and are not adjacent.</exception>
    public DateRange Union(DateRange other)
    {
        if (!Overlaps(other) && !IsAdjacent(other))
            throw new InvalidOperationException("Cannot compute union of non-overlapping, non-adjacent ranges. Use Gap() to find the gap between them.");

        var start = Start < other.Start ? Start : other.Start;
        var end = End > other.End ? End : other.End;

        return new DateRange(start, end);
    }

    /// <summary>
    /// Computes the gap between this range and a non-overlapping range.
    /// </summary>
    /// <param name="other">The other range.</param>
    /// <returns>A <see cref="DateRange"/> representing the gap, or null if the ranges overlap or are adjacent.</returns>
    public DateRange? Gap(DateRange other)
    {
        if (Overlaps(other) || IsAdjacent(other))
            return null;

        var earlier = Start < other.Start ? this : other;
        var later = Start < other.Start ? other : this;

        return new DateRange(earlier.End, later.Start);
    }

    /// <summary>
    /// Checks whether this range is adjacent to another (one ends exactly where the other starts).
    /// </summary>
    /// <param name="other">The other range.</param>
    /// <returns>True if the ranges are adjacent.</returns>
    public bool IsAdjacent(DateRange other) => End == other.Start || other.End == Start;

    /// <summary>
    /// Splits this range into segments of the specified duration.
    /// The last segment may be shorter than the specified duration.
    /// </summary>
    /// <param name="segmentDuration">The duration of each segment.</param>
    /// <returns>An enumerable of date range segments.</returns>
    /// <exception cref="ArgumentException">Thrown when segmentDuration is zero or negative.</exception>
    public IEnumerable<DateRange> Split(TimeSpan segmentDuration)
    {
        if (segmentDuration <= TimeSpan.Zero)
            throw new ArgumentException("Segment duration must be positive.", nameof(segmentDuration));

        var current = Start;
        while (current < End)
        {
            var segmentEnd = current + segmentDuration;
            if (segmentEnd > End)
                segmentEnd = End;

            yield return new DateRange(current, segmentEnd);
            current = segmentEnd;
        }
    }

    /// <summary>
    /// Merges a collection of potentially overlapping ranges into a minimal set of non-overlapping ranges
    /// using a sort-and-sweep algorithm.
    /// </summary>
    /// <param name="ranges">The ranges to merge.</param>
    /// <returns>A read-only list of merged, non-overlapping ranges sorted by start time.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ranges"/> is null.</exception>
    public static IReadOnlyList<DateRange> MergeAll(IEnumerable<DateRange> ranges)
    {
        ArgumentNullException.ThrowIfNull(ranges);

        var sorted = ranges.OrderBy(r => r.Start).ThenBy(r => r.End).ToList();

        if (sorted.Count == 0)
            return Array.Empty<DateRange>();

        var result = new List<DateRange> { sorted[0] };

        for (int i = 1; i < sorted.Count; i++)
        {
            var current = sorted[i];
            var last = result[^1];

            if (last.Overlaps(current) || last.IsAdjacent(current))
            {
                var mergedEnd = last.End > current.End ? last.End : current.End;
                result[^1] = new DateRange(last.Start, mergedEnd);
            }
            else
            {
                result.Add(current);
            }
        }

        return result.AsReadOnly();
    }

    /// <summary>
    /// Counts the number of business days (weekdays, Monday through Friday) within this range.
    /// Partial days are not counted; only dates where the entire day falls within [Start, End) are considered.
    /// Each calendar date that is a weekday and whose start-of-day (UTC) falls within the range is counted.
    /// </summary>
    /// <returns>The number of weekdays in the range.</returns>
    public int BusinessDays()
    {
        int count = 0;
        var current = Start.Date;
        var endDate = End.Date;

        // If End is exactly midnight, that day is excluded (half-open interval)
        if (End == new DateTimeOffset(endDate, End.Offset))
            endDate = endDate.AddDays(-1);

        while (current <= endDate)
        {
            var dayOfWeek = current.DayOfWeek;
            if (dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday)
                count++;

            current = current.AddDays(1);
        }

        return count;
    }

    /// <summary>
    /// Returns a collection of sub-ranges representing only the weekday (Monday through Friday) portions
    /// of this range. Weekend days are excluded, effectively splitting the range at weekend boundaries.
    /// </summary>
    /// <returns>A read-only list of <see cref="DateRange"/> values covering weekday-only portions.</returns>
    public IReadOnlyList<DateRange> ExcludeWeekends()
    {
        var result = new List<DateRange>();
        var current = Start;

        while (current < End)
        {
            // Skip weekend days
            if (current.DayOfWeek == DayOfWeek.Saturday)
            {
                current = new DateTimeOffset(current.Date.AddDays(2), current.Offset);
                continue;
            }

            if (current.DayOfWeek == DayOfWeek.Sunday)
            {
                current = new DateTimeOffset(current.Date.AddDays(1), current.Offset);
                continue;
            }

            // Find the end of the current weekday run
            var weekdayEnd = current;
            while (weekdayEnd < End && weekdayEnd.DayOfWeek != DayOfWeek.Saturday && weekdayEnd.DayOfWeek != DayOfWeek.Sunday)
            {
                var nextDay = new DateTimeOffset(weekdayEnd.Date.AddDays(1), weekdayEnd.Offset);
                if (nextDay > End)
                {
                    weekdayEnd = End;
                    break;
                }

                weekdayEnd = nextDay;

                if (weekdayEnd.DayOfWeek == DayOfWeek.Saturday || weekdayEnd.DayOfWeek == DayOfWeek.Sunday)
                    break;
            }

            if (weekdayEnd > current)
                result.Add(new DateRange(current, weekdayEnd));

            current = weekdayEnd;
        }

        return result.AsReadOnly();
    }

    /// <summary>
    /// Gets a value indicating whether this range is empty (Start equals End).
    /// </summary>
    public bool IsEmpty => Start == End;

    /// <summary>
    /// Gets an empty range positioned at <see cref="DateTimeOffset.MinValue"/>.
    /// Useful as a sentinel; <see cref="IsEmpty"/> returns <c>true</c>.
    /// </summary>
    public static DateRange Empty => new(DateTimeOffset.MinValue, DateTimeOffset.MinValue);

    /// <summary>
    /// Returns a new <see cref="DateRange"/> shifted by the specified duration.
    /// Positive deltas move the range later; negative deltas move it earlier.
    /// </summary>
    /// <param name="delta">The amount to shift the range by.</param>
    /// <returns>A new range with both endpoints translated by <paramref name="delta"/>.</returns>
    public DateRange Shift(TimeSpan delta) => new(Start + delta, End + delta);

    /// <summary>
    /// Returns the portions of this range that do not overlap with <paramref name="other"/>.
    /// The result contains zero, one, or two ranges depending on how the ranges intersect.
    /// </summary>
    /// <param name="other">The range to subtract from this range.</param>
    /// <returns>A read-only list of non-overlapping sub-ranges sorted by start time.</returns>
    public IReadOnlyList<DateRange> Subtract(DateRange other)
    {
        if (!Overlaps(other))
        {
            return new[] { this };
        }

        if (other.Start <= Start && other.End >= End)
        {
            return Array.Empty<DateRange>();
        }

        var result = new List<DateRange>(2);
        if (Start < other.Start)
        {
            result.Add(new DateRange(Start, other.Start));
        }
        if (other.End < End)
        {
            result.Add(new DateRange(other.End, End));
        }
        return result;
    }

    /// <summary>
    /// Determines whether the duration of this range is shorter than the specified time span.
    /// </summary>
    /// <param name="timeSpan">The time span to compare against.</param>
    /// <returns>True if the range duration is strictly less than <paramref name="timeSpan"/>.</returns>
    public bool IsShorterThan(TimeSpan timeSpan) => Duration < timeSpan;

    /// <summary>
    /// Determines whether the duration of this range is longer than the specified time span.
    /// </summary>
    /// <param name="timeSpan">The time span to compare against.</param>
    /// <returns>True if the range duration is strictly greater than <paramref name="timeSpan"/>.</returns>
    public bool IsLongerThan(TimeSpan timeSpan) => Duration > timeSpan;

    /// <summary>
    /// Determines whether the duration of this range is exactly equal to the specified time span.
    /// </summary>
    /// <param name="timeSpan">The time span to compare against.</param>
    /// <returns>True if the range duration equals <paramref name="timeSpan"/>.</returns>
    public bool DurationEquals(TimeSpan timeSpan) => Duration == timeSpan;

    /// <inheritdoc />
    public int CompareTo(DateRange other)
    {
        int result = Start.CompareTo(other.Start);
        return result != 0 ? result : End.CompareTo(other.End);
    }

    /// <inheritdoc />
    public override string ToString() => $"[{Start:O}, {End:O})";
}
