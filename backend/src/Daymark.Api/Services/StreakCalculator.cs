namespace Daymark.Api.Services;

public static class StreakCalculator
{
    // Days checked in a row ending today. If today isn't checked yet, the
    // streak counts back from yesterday, so it doesn't reset at midnight.
    public static int Current(IEnumerable<DateOnly> dates, DateOnly today)
    {
        var set = new HashSet<DateOnly>(dates);
        var start = set.Contains(today) ? today : today.AddDays(-1);

        if (!set.Contains(start))
        {
            return 0;
        }

        var streak = 0;
        var cursor = start;
        while (set.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }

    public static int Longest(IEnumerable<DateOnly> dates)
    {
        var sorted = dates.Distinct().OrderBy(d => d).ToList();
        if (sorted.Count == 0)
        {
            return 0;
        }

        var best = 1;
        var run = 1;
        for (var i = 1; i < sorted.Count; i++)
        {
            run = sorted[i].DayNumber == sorted[i - 1].DayNumber + 1 ? run + 1 : 1;
            best = Math.Max(best, run);
        }

        return best;
    }
}
