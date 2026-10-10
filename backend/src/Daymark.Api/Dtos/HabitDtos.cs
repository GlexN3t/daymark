using Daymark.Api.Domain;
using Daymark.Api.Services;

namespace Daymark.Api.Dtos;

public record HabitDto(
    int Id,
    string Name,
    string? Note,
    string Color,
    bool Archived,
    DateOnly CreatedOn,
    int CurrentStreak,
    int LongestStreak,
    int TotalCheckIns,
    DateOnly? LastCheckIn,
    bool CheckedInToday)
{
    public static HabitDto From(Habit habit, DateOnly today)
    {
        var dates = habit.CheckIns.Select(c => c.Date).ToList();
        DateOnly? last = dates.Count > 0 ? (DateOnly?)dates.Max() : null;

        return new HabitDto(
            habit.Id,
            habit.Name,
            habit.Note,
            habit.Color,
            habit.Archived,
            habit.CreatedOn,
            StreakCalculator.Current(dates, today),
            StreakCalculator.Longest(dates),
            dates.Count,
            last,
            dates.Contains(today));
    }
}

public record HabitDetailDto(
    int Id,
    string Name,
    string? Note,
    string Color,
    bool Archived,
    DateOnly CreatedOn,
    int CurrentStreak,
    int LongestStreak,
    int TotalCheckIns,
    DateOnly? LastCheckIn,
    bool CheckedInToday,
    List<CheckInDto> CheckIns)
{
    public static HabitDetailDto From(Habit habit, DateOnly today, DateOnly from, DateOnly to)
    {
        var habitDto = HabitDto.From(habit, today);
        var checkIns = habit.CheckIns
            .Where(c => c.Date >= from && c.Date <= to)
            .OrderBy(c => c.Date)
            .Select(c => new CheckInDto(c.Date, c.Note))
            .ToList();

        return new HabitDetailDto(
            habitDto.Id,
            habitDto.Name,
            habitDto.Note,
            habitDto.Color,
            habitDto.Archived,
            habitDto.CreatedOn,
            habitDto.CurrentStreak,
            habitDto.LongestStreak,
            habitDto.TotalCheckIns,
            habitDto.LastCheckIn,
            habitDto.CheckedInToday,
            checkIns);
    }
}

public record CheckInDto(DateOnly Date, string? Note);

public record CreateHabitRequest(string? Name, string? Note, string? Color);

public record UpdateHabitRequest(string? Name, string? Note, string? Color, bool? Archived);

public record CheckInRequest(string? Date, string? Note);
