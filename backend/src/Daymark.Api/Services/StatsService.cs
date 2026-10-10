using Daymark.Api.Data;
using Daymark.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Daymark.Api.Services;

public class StatsService
{
    private readonly DaymarkDbContext _db;

    public StatsService(DaymarkDbContext db)
    {
        _db = db;
    }

    public async Task<SummaryDto> GetSummaryAsync(int userId, DateOnly today, CancellationToken ct)
    {
        var habits = await _db.Habits
            .AsNoTracking()
            .Include(h => h.CheckIns)
            .Where(h => h.UserId == userId && !h.Archived)
            .OrderBy(h => h.Name)
            .ToListAsync(ct);

        var windowStart = today.AddDays(-29);
        var possibleDays = 0;
        var checkedDays = 0;
        var summaries = new List<HabitSummaryDto>();

        foreach (var habit in habits)
        {
            var dates = habit.CheckIns.Select(c => c.Date).ToList();

            summaries.Add(new HabitSummaryDto(
                habit.Id,
                habit.Name,
                habit.Color,
                StreakCalculator.Current(dates, today),
                StreakCalculator.Longest(dates),
                dates.Count,
                dates.Contains(today)));

            var countedFrom = habit.CreatedOn > windowStart ? habit.CreatedOn : windowStart;
            var available = today.DayNumber - countedFrom.DayNumber + 1;
            if (available > 0)
            {
                possibleDays += available;
                checkedDays += dates.Count(d => d >= countedFrom && d <= today);
            }
        }

        var rate = possibleDays == 0 ? 0 : (int)Math.Round(checkedDays * 100.0 / possibleDays);

        return new SummaryDto(
            habits.Count,
            summaries.Count(s => s.CheckedInToday),
            summaries.Count > 0 ? summaries.Max(s => s.CurrentStreak) : 0,
            summaries.Count > 0 ? summaries.Max(s => s.LongestStreak) : 0,
            rate,
            summaries);
    }

    public async Task<List<DayCountDto>> GetCalendarAsync(int userId, int year, CancellationToken ct)
    {
        var start = new DateOnly(year, 1, 1);
        var end = new DateOnly(year, 12, 31);

        var rows = await _db.CheckIns
            .AsNoTracking()
            .Where(c => c.Habit!.UserId == userId && !c.Habit.Archived)
            .Where(c => c.Date >= start && c.Date <= end)
            .GroupBy(c => c.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return rows
            .Select(r => new DayCountDto(r.Date, r.Count))
            .OrderBy(r => r.Date)
            .ToList();
    }
}
