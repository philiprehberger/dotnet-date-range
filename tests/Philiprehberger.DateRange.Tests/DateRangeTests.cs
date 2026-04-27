using Xunit;

namespace Philiprehberger.DateRange.Tests;

public class DateRangeTests
{
    private static readonly DateTimeOffset T1 = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset T2 = new(2024, 1, 2, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset T3 = new(2024, 1, 3, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset T4 = new(2024, 1, 4, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_ValidRange_ReturnsDateRange()
    {
        var range = DateRange.Create(T1, T3);

        Assert.Equal(T1, range.Start);
        Assert.Equal(T3, range.End);
    }

    [Fact]
    public void Create_StartAfterEnd_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => DateRange.Create(T3, T1));
    }

    [Fact]
    public void Duration_ReturnsDifferenceBetweenStartAndEnd()
    {
        var range = DateRange.Create(T1, T3);

        Assert.Equal(TimeSpan.FromDays(2), range.Duration);
    }

    [Fact]
    public void Overlaps_OverlappingRanges_ReturnsTrue()
    {
        var a = DateRange.Create(T1, T3);
        var b = DateRange.Create(T2, T4);

        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_NonOverlappingRanges_ReturnsFalse()
    {
        var a = DateRange.Create(T1, T2);
        var b = DateRange.Create(T3, T4);

        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void Contains_PointInRange_ReturnsTrue()
    {
        var range = DateRange.Create(T1, T3);

        Assert.True(range.Contains(T2));
    }

    [Fact]
    public void Contains_PointAtEnd_ReturnsFalse()
    {
        var range = DateRange.Create(T1, T3);

        Assert.False(range.Contains(T3));
    }

    [Fact]
    public void Intersection_OverlappingRanges_ReturnsOverlap()
    {
        var a = DateRange.Create(T1, T3);
        var b = DateRange.Create(T2, T4);

        var intersection = a.Intersection(b);

        Assert.NotNull(intersection);
        Assert.Equal(T2, intersection.Value.Start);
        Assert.Equal(T3, intersection.Value.End);
    }

    [Fact]
    public void Intersection_NonOverlappingRanges_ReturnsNull()
    {
        var a = DateRange.Create(T1, T2);
        var b = DateRange.Create(T3, T4);

        Assert.Null(a.Intersection(b));
    }

    [Fact]
    public void Union_OverlappingRanges_ReturnsMergedRange()
    {
        var a = DateRange.Create(T1, T3);
        var b = DateRange.Create(T2, T4);

        var union = a.Union(b);

        Assert.Equal(T1, union.Start);
        Assert.Equal(T4, union.End);
    }

    [Fact]
    public void Gap_NonOverlappingRanges_ReturnsGap()
    {
        var a = DateRange.Create(T1, T2);
        var b = DateRange.Create(T3, T4);

        var gap = a.Gap(b);

        Assert.NotNull(gap);
        Assert.Equal(T2, gap.Value.Start);
        Assert.Equal(T3, gap.Value.End);
    }

    [Fact]
    public void Split_DurationSmallerThanRange_ReturnsMultipleSegments()
    {
        var range = DateRange.Create(T1, T3);

        var segments = range.Split(TimeSpan.FromDays(1)).ToList();

        Assert.Equal(2, segments.Count);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsAdjacent_AdjacentAndNonAdjacent_ReturnsCorrectResult(bool adjacent)
    {
        var a = DateRange.Create(T1, T2);
        var b = adjacent ? DateRange.Create(T2, T3) : DateRange.Create(T3, T4);

        Assert.Equal(adjacent, a.IsAdjacent(b));
    }

    // MergeAll tests

    [Fact]
    public void MergeAll_OverlappingRanges_ReturnsMergedSet()
    {
        var ranges = new[]
        {
            DateRange.Create(T1, T3),
            DateRange.Create(T2, T4)
        };

        var merged = DateRange.MergeAll(ranges);

        Assert.Single(merged);
        Assert.Equal(T1, merged[0].Start);
        Assert.Equal(T4, merged[0].End);
    }

    [Fact]
    public void MergeAll_NonOverlappingRanges_ReturnsAll()
    {
        var ranges = new[]
        {
            DateRange.Create(T3, T4),
            DateRange.Create(T1, T2)
        };

        var merged = DateRange.MergeAll(ranges);

        Assert.Equal(2, merged.Count);
        Assert.Equal(T1, merged[0].Start);
        Assert.Equal(T3, merged[1].Start);
    }

    [Fact]
    public void MergeAll_EmptyCollection_ReturnsEmpty()
    {
        var merged = DateRange.MergeAll(Array.Empty<DateRange>());

        Assert.Empty(merged);
    }

    [Fact]
    public void MergeAll_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => DateRange.MergeAll(null!));
    }

    // BusinessDays tests

    [Fact]
    public void BusinessDays_FullWeek_ReturnsFive()
    {
        // Monday 2024-01-01 to Monday 2024-01-08 (full week)
        var monday = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var nextMonday = new DateTimeOffset(2024, 1, 8, 0, 0, 0, TimeSpan.Zero);
        var range = DateRange.Create(monday, nextMonday);

        Assert.Equal(5, range.BusinessDays());
    }

    [Fact]
    public void BusinessDays_WeekendOnly_ReturnsZero()
    {
        // Saturday 2024-01-06 to Monday 2024-01-08
        var saturday = new DateTimeOffset(2024, 1, 6, 0, 0, 0, TimeSpan.Zero);
        var monday = new DateTimeOffset(2024, 1, 8, 0, 0, 0, TimeSpan.Zero);
        var range = DateRange.Create(saturday, monday);

        Assert.Equal(0, range.BusinessDays());
    }

    [Fact]
    public void BusinessDays_SingleWeekday_ReturnsOne()
    {
        // Monday 2024-01-01 to Tuesday 2024-01-02
        var monday = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var tuesday = new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero);
        var range = DateRange.Create(monday, tuesday);

        Assert.Equal(1, range.BusinessDays());
    }

    [Fact]
    public void BusinessDays_TwoWeeks_ReturnsTen()
    {
        var monday = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var twoWeeksLater = new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero);
        var range = DateRange.Create(monday, twoWeeksLater);

        Assert.Equal(10, range.BusinessDays());
    }

    // ExcludeWeekends tests

    [Fact]
    public void ExcludeWeekends_FullWeek_ReturnsSingleWeekdayRange()
    {
        // Monday to next Monday (half-open, so next Monday is excluded)
        var monday = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var nextMonday = new DateTimeOffset(2024, 1, 8, 0, 0, 0, TimeSpan.Zero);
        var range = DateRange.Create(monday, nextMonday);

        var weekdays = range.ExcludeWeekends();

        Assert.Single(weekdays);
        Assert.Equal(monday, weekdays[0].Start);
        Assert.Equal(new DateTimeOffset(2024, 1, 6, 0, 0, 0, TimeSpan.Zero), weekdays[0].End);
    }

    [Fact]
    public void ExcludeWeekends_WeekendOnly_ReturnsEmpty()
    {
        // Saturday to Monday
        var saturday = new DateTimeOffset(2024, 1, 6, 0, 0, 0, TimeSpan.Zero);
        var monday = new DateTimeOffset(2024, 1, 8, 0, 0, 0, TimeSpan.Zero);
        var range = DateRange.Create(saturday, monday);

        var weekdays = range.ExcludeWeekends();

        Assert.Empty(weekdays);
    }

    [Fact]
    public void ExcludeWeekends_WeekdaysOnly_ReturnsSingleRange()
    {
        // Monday to Friday
        var monday = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var friday = new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero);
        var range = DateRange.Create(monday, friday);

        var weekdays = range.ExcludeWeekends();

        Assert.Single(weekdays);
        Assert.Equal(monday, weekdays[0].Start);
        Assert.Equal(friday, weekdays[0].End);
    }

    [Fact]
    public void ExcludeWeekends_TwoWeeks_ReturnsTwoChunks()
    {
        // Monday Jan 1 to Friday Jan 12 (spans two work weeks)
        var monday = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var secondFriday = new DateTimeOffset(2024, 1, 12, 0, 0, 0, TimeSpan.Zero);
        var range = DateRange.Create(monday, secondFriday);

        var weekdays = range.ExcludeWeekends();

        Assert.Equal(2, weekdays.Count);
        // First week: Mon Jan 1 to Sat Jan 6
        Assert.Equal(monday, weekdays[0].Start);
        Assert.Equal(new DateTimeOffset(2024, 1, 6, 0, 0, 0, TimeSpan.Zero), weekdays[0].End);
        // Second week: Mon Jan 8 to Fri Jan 12
        Assert.Equal(new DateTimeOffset(2024, 1, 8, 0, 0, 0, TimeSpan.Zero), weekdays[1].Start);
        Assert.Equal(secondFriday, weekdays[1].End);
    }

    // Duration comparison helpers tests

    [Fact]
    public void IsShorterThan_ShorterDuration_ReturnsTrue()
    {
        var range = DateRange.Create(T1, T2); // 1 day

        Assert.True(range.IsShorterThan(TimeSpan.FromDays(2)));
    }

    [Fact]
    public void IsShorterThan_EqualDuration_ReturnsFalse()
    {
        var range = DateRange.Create(T1, T2); // 1 day

        Assert.False(range.IsShorterThan(TimeSpan.FromDays(1)));
    }

    [Fact]
    public void IsShorterThan_LongerDuration_ReturnsFalse()
    {
        var range = DateRange.Create(T1, T3); // 2 days

        Assert.False(range.IsShorterThan(TimeSpan.FromDays(1)));
    }

    [Fact]
    public void IsLongerThan_LongerDuration_ReturnsTrue()
    {
        var range = DateRange.Create(T1, T3); // 2 days

        Assert.True(range.IsLongerThan(TimeSpan.FromDays(1)));
    }

    [Fact]
    public void IsLongerThan_EqualDuration_ReturnsFalse()
    {
        var range = DateRange.Create(T1, T2); // 1 day

        Assert.False(range.IsLongerThan(TimeSpan.FromDays(1)));
    }

    [Fact]
    public void IsLongerThan_ShorterDuration_ReturnsFalse()
    {
        var range = DateRange.Create(T1, T2); // 1 day

        Assert.False(range.IsLongerThan(TimeSpan.FromDays(2)));
    }

    [Fact]
    public void DurationEquals_EqualDuration_ReturnsTrue()
    {
        var range = DateRange.Create(T1, T2); // 1 day

        Assert.True(range.DurationEquals(TimeSpan.FromDays(1)));
    }

    [Fact]
    public void DurationEquals_DifferentDuration_ReturnsFalse()
    {
        var range = DateRange.Create(T1, T2); // 1 day

        Assert.False(range.DurationEquals(TimeSpan.FromDays(2)));
    }

    [Fact]
    public void DurationEquals_ZeroDuration_ReturnsFalse()
    {
        var range = DateRange.Create(T1, T2); // 1 day

        Assert.False(range.DurationEquals(TimeSpan.Zero));
    }

    [Fact]
    public void Shift_PositiveDelta_MovesRangeForward()
    {
        var range = DateRange.Create(T1, T2);

        var shifted = range.Shift(TimeSpan.FromDays(1));

        Assert.Equal(T2, shifted.Start);
        Assert.Equal(T3, shifted.End);
    }

    [Fact]
    public void Shift_NegativeDelta_MovesRangeBackward()
    {
        var range = DateRange.Create(T2, T3);

        var shifted = range.Shift(TimeSpan.FromDays(-1));

        Assert.Equal(T1, shifted.Start);
        Assert.Equal(T2, shifted.End);
    }

    [Fact]
    public void IsEmpty_WhenStartEqualsEnd_ReturnsTrue()
    {
        var empty = new DateRange(T1, T1);

        Assert.True(empty.IsEmpty);
        Assert.True(DateRange.Empty.IsEmpty);
    }

    [Fact]
    public void IsEmpty_WhenStartBeforeEnd_ReturnsFalse()
    {
        var range = DateRange.Create(T1, T2);

        Assert.False(range.IsEmpty);
    }

    [Fact]
    public void Subtract_NoOverlap_ReturnsOriginalRange()
    {
        var a = DateRange.Create(T1, T2);
        var b = DateRange.Create(T3, T4);

        var result = a.Subtract(b);

        Assert.Single(result);
        Assert.Equal(a, result[0]);
    }

    [Fact]
    public void Subtract_OtherFullyContainsThis_ReturnsEmpty()
    {
        var a = DateRange.Create(T2, T3);
        var b = DateRange.Create(T1, T4);

        var result = a.Subtract(b);

        Assert.Empty(result);
    }

    [Fact]
    public void Subtract_PartialOverlapAtStart_ReturnsTrailingPortion()
    {
        var a = DateRange.Create(T1, T3);
        var b = DateRange.Create(T1, T2);

        var result = a.Subtract(b);

        Assert.Single(result);
        Assert.Equal(T2, result[0].Start);
        Assert.Equal(T3, result[0].End);
    }

    [Fact]
    public void Subtract_OtherFullyInside_ReturnsLeadingAndTrailing()
    {
        var a = DateRange.Create(T1, T4);
        var b = DateRange.Create(T2, T3);

        var result = a.Subtract(b);

        Assert.Equal(2, result.Count);
        Assert.Equal(new DateRange(T1, T2), result[0]);
        Assert.Equal(new DateRange(T3, T4), result[1]);
    }
}
