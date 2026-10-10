using Daymark.Api.Domain;
using Daymark.Api.Dtos;
using Xunit;

namespace Daymark.Tests;

public class HabitDtoTests
{
    private static readonly DateOnly Today = new(2026, 9, 30);

    [Fact]
    public void From_maps_streaks_and_today_flag()
    {
        var habit = new Habit
        {
            Id = 1,
            Name = "Read",
            Color = "#1f6f5c",
            CreatedOn = new DateOnly(2026, 9, 1),
            CheckIns =
            [
                new CheckIn { Date = new DateOnly(2026, 9, 29) },
                new CheckIn { Date = new DateOnly(2026, 9, 30) }
            ]
        };

        var dto = HabitDto.From(habit, Today);

        Assert.Equal(2, dto.CurrentStreak);
        Assert.Equal(2, dto.LongestStreak);
        Assert.Equal(2, dto.TotalCheckIns);
        Assert.Equal(new DateOnly(2026, 9, 30), dto.LastCheckIn);
        Assert.True(dto.CheckedInToday);
    }

    [Fact]
    public void From_handles_a_habit_with_no_checkins()
    {
        var habit = new Habit
        {
            Id = 2,
            Name = "Meditate",
            Color = "#1f6f5c",
            CreatedOn = Today
        };

        var dto = HabitDto.From(habit, Today);

        Assert.Equal(0, dto.CurrentStreak);
        Assert.Equal(0, dto.LongestStreak);
        Assert.Equal(0, dto.TotalCheckIns);
        Assert.Null(dto.LastCheckIn);
        Assert.False(dto.CheckedInToday);
    }
}
