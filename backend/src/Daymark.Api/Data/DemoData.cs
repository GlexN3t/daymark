using Daymark.Api.Domain;
using Daymark.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Daymark.Api.Data;

// Seeds a throwaway demo account so a fresh clone has something to look at.
// Enabled only in Development via the SeedDemoData setting.
public static class DemoData
{
    public const string DemoEmail = "demo@daymark.dev";
    public const string DemoPassword = "daymark-demo";

    public static async Task SeedAsync(DaymarkDbContext db, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(ct))
        {
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var user = new User
        {
            Email = DemoEmail,
            DisplayName = "Demo",
            PasswordHash = PasswordHasher.Hash(DemoPassword),
            CreatedAt = DateTime.UtcNow
        };

        var reading = new Habit
        {
            Name = "Read 20 pages",
            Note = "Paper book, before bed.",
            Color = "#3b6ef5",
            CreatedOn = today.AddDays(-45)
        };

        var walk = new Habit
        {
            Name = "Walk 30 minutes",
            Note = "Around the block, no phone.",
            Color = "#1f6f5c",
            CreatedOn = today.AddDays(-30)
        };

        var water = new Habit
        {
            Name = "Drink water first thing",
            Color = "#a4552b",
            CreatedOn = today.AddDays(-14)
        };

        for (var i = 0; i <= 21; i++)
        {
            reading.CheckIns.Add(new CheckIn { Date = today.AddDays(-i), CreatedAt = DateTime.UtcNow });
        }

        for (var i = 0; i <= 9; i++)
        {
            walk.CheckIns.Add(new CheckIn { Date = today.AddDays(-i), CreatedAt = DateTime.UtcNow });
        }

        walk.CheckIns.Add(new CheckIn { Date = today.AddDays(-13), CreatedAt = DateTime.UtcNow });
        water.CheckIns.Add(new CheckIn { Date = today, CreatedAt = DateTime.UtcNow });

        user.Habits.Add(reading);
        user.Habits.Add(walk);
        user.Habits.Add(water);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
    }
}
