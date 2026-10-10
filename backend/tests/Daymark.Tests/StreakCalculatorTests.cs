using System.Globalization;
using Daymark.Api.Services;
using Xunit;

namespace Daymark.Tests;

public class StreakCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 9, 30);

    private static DateOnly D(string iso) =>
        DateOnly.ParseExact(iso, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    [Fact]
    public void Current_returns_zero_when_nothing_checked()
    {
        Assert.Equal(0, StreakCalculator.Current([], Today));
    }

    [Fact]
    public void Current_counts_back_from_today()
    {
        var dates = new[] { D("2026-09-28"), D("2026-09-29"), D("2026-09-30") };

        Assert.Equal(3, StreakCalculator.Current(dates, Today));
    }

    [Fact]
    public void Current_counts_from_yesterday_when_today_is_not_checked_yet()
    {
        var dates = new[] { D("2026-09-28"), D("2026-09-29") };

        Assert.Equal(2, StreakCalculator.Current(dates, Today));
    }

    [Fact]
    public void Current_resets_after_a_gap()
    {
        var dates = new[] { D("2026-09-20"), D("2026-09-29"), D("2026-09-30") };

        Assert.Equal(2, StreakCalculator.Current(dates, Today));
    }

    [Fact]
    public void Longest_picks_the_longest_run()
    {
        var dates = new[]
        {
            D("2026-09-01"), D("2026-09-02"), D("2026-09-03"),
            D("2026-09-10"), D("2026-09-11")
        };

        Assert.Equal(3, StreakCalculator.Longest(dates));
    }

    [Fact]
    public void Longest_ignores_duplicate_dates()
    {
        var dates = new[] { D("2026-09-05"), D("2026-09-05"), D("2026-09-06") };

        Assert.Equal(2, StreakCalculator.Longest(dates));
    }

    [Fact]
    public void Longest_returns_zero_when_empty()
    {
        Assert.Equal(0, StreakCalculator.Longest([]));
    }
}
