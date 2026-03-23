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

    /// <inheritdoc />
    public int CompareTo(DateRange other)
    {
        int result = Start.CompareTo(other.Start);
        return result != 0 ? result : End.CompareTo(other.End);
    }

    /// <inheritdoc />
    public override string ToString() => $"[{Start:O}, {End:O})";
}
