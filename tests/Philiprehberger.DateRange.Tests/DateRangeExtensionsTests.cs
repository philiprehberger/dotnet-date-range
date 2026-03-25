using Xunit;

namespace Philiprehberger.DateRange.Tests;

public class DateRangeExtensionsTests
{
    private static readonly DateTimeOffset T1 = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset T2 = new(2024, 1, 2, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset T3 = new(2024, 1, 3, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset T4 = new(2024, 1, 4, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset T5 = new(2024, 1, 5, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Merge_OverlappingRanges_ReturnsMergedSet()
    {
        var ranges = new[]
        {
            DateRange.Create(T1, T3),
            DateRange.Create(T2, T4)
        };

        var merged = ranges.Merge();

        Assert.Single(merged);
        Assert.Equal(T1, merged[0].Start);
        Assert.Equal(T4, merged[0].End);
    }

    [Fact]
    public void FindGaps_RangesWithGap_ReturnsGaps()
    {
        var ranges = new[]
        {
            DateRange.Create(T1, T2),
            DateRange.Create(T3, T4)
        };

        var gaps = ranges.FindGaps();

        Assert.Single(gaps);
        Assert.Equal(T2, gaps[0].Start);
        Assert.Equal(T3, gaps[0].End);
    }

    [Fact]
    public void AnyOverlap_OverlappingRanges_ReturnsTrue()
    {
        var ranges = new[]
        {
            DateRange.Create(T1, T3),
            DateRange.Create(T2, T4)
        };

        Assert.True(ranges.AnyOverlap());
    }

    [Fact]
    public void AnyOverlap_NonOverlappingRanges_ReturnsFalse()
    {
        var ranges = new[]
        {
            DateRange.Create(T1, T2),
            DateRange.Create(T3, T4)
        };

        Assert.False(ranges.AnyOverlap());
    }

    [Fact]
    public void TotalDuration_OverlappingRanges_CountsOverlapOnce()
    {
        var ranges = new[]
        {
            DateRange.Create(T1, T3),
            DateRange.Create(T2, T4)
        };

        var duration = ranges.TotalDuration();

        Assert.Equal(TimeSpan.FromDays(3), duration);
    }

    [Fact]
    public void Merge_EmptyCollection_ReturnsEmptyList()
    {
        var ranges = Array.Empty<DateRange>();

        var merged = ranges.Merge();

        Assert.Empty(merged);
    }
}
