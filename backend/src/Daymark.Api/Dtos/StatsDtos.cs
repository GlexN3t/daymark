namespace Daymark.Api.Dtos;

public record SummaryDto(
    int ActiveHabits,
    int CheckedInToday,
    int BestCurrentStreak,
    int BestLongestStreak,
    int CompletionRate,
    List<HabitSummaryDto> Habits);

public record HabitSummaryDto(
    int Id,
    string Name,
    string Color,
    int CurrentStreak,
    int LongestStreak,
    int TotalCheckIns,
    bool CheckedInToday);

public record DayCountDto(DateOnly Date, int Count);
