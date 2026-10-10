using System.Text.RegularExpressions;
using Daymark.Api.Data;
using Daymark.Api.Domain;
using Daymark.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Daymark.Api.Services;

public enum CheckInWriteResult
{
    NotFound,
    Archived,
    Done
}

public class HabitService
{
    private const string DefaultColor = "#1f6f5c";

    private readonly DaymarkDbContext _db;

    public HabitService(DaymarkDbContext db)
    {
        _db = db;
    }

    public async Task<List<HabitDto>> GetHabitsAsync(int userId, bool includeArchived, DateOnly today, CancellationToken ct)
    {
        var habits = await _db.Habits
            .AsNoTracking()
            .Include(h => h.CheckIns)
            .Where(h => h.UserId == userId)
            .Where(h => includeArchived || !h.Archived)
            .OrderBy(h => h.Archived)
            .ThenBy(h => h.Name)
            .ToListAsync(ct);

        return habits.Select(h => HabitDto.From(h, today)).ToList();
    }

    public async Task<HabitDetailDto?> GetHabitAsync(
        int userId, int habitId, DateOnly from, DateOnly to, DateOnly today, CancellationToken ct)
    {
        var habit = await _db.Habits
            .AsNoTracking()
            .Include(h => h.CheckIns)
            .FirstOrDefaultAsync(h => h.Id == habitId && h.UserId == userId, ct);

        return habit is null ? null : HabitDetailDto.From(habit, today, from, to);
    }

    public async Task<HabitDto> CreateAsync(int userId, CreateHabitRequest request, DateOnly today, CancellationToken ct)
    {
        var habit = new Habit
        {
            UserId = userId,
            Name = request.Name!.Trim(),
            Note = NormalizeNote(request.Note),
            Color = ValidColor(request.Color) ? request.Color! : DefaultColor,
            CreatedOn = today
        };

        _db.Habits.Add(habit);
        await _db.SaveChangesAsync(ct);

        return HabitDto.From(habit, today);
    }

    public async Task<HabitDto?> UpdateAsync(int userId, int habitId, UpdateHabitRequest request, DateOnly today, CancellationToken ct)
    {
        var habit = await _db.Habits
            .Include(h => h.CheckIns)
            .FirstOrDefaultAsync(h => h.Id == habitId && h.UserId == userId, ct);

        if (habit is null)
        {
            return null;
        }

        habit.Name = request.Name!.Trim();
        habit.Note = NormalizeNote(request.Note);
        habit.Color = ValidColor(request.Color) ? request.Color! : habit.Color;
        habit.Archived = request.Archived ?? habit.Archived;

        await _db.SaveChangesAsync(ct);

        return HabitDto.From(habit, today);
    }

    public async Task<bool> DeleteAsync(int userId, int habitId, CancellationToken ct)
    {
        var habit = await _db.Habits
            .Include(h => h.CheckIns)
            .FirstOrDefaultAsync(h => h.Id == habitId && h.UserId == userId, ct);

        if (habit is null)
        {
            return false;
        }

        _db.CheckIns.RemoveRange(habit.CheckIns);
        _db.Habits.Remove(habit);
        await _db.SaveChangesAsync(ct);

        return true;
    }

    public async Task<CheckInWriteResult> AddCheckInAsync(int userId, int habitId, DateOnly date, string? note, CancellationToken ct)
    {
        var habit = await _db.Habits
            .Include(h => h.CheckIns)
            .FirstOrDefaultAsync(h => h.Id == habitId && h.UserId == userId, ct);

        if (habit is null)
        {
            return CheckInWriteResult.NotFound;
        }

        if (habit.Archived)
        {
            return CheckInWriteResult.Archived;
        }

        if (habit.CheckIns.Any(c => c.Date == date))
        {
            return CheckInWriteResult.Done;
        }

        habit.CheckIns.Add(new CheckIn
        {
            Date = date,
            Note = NormalizeNote(note),
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return CheckInWriteResult.Done;
    }

    public async Task<CheckInWriteResult> RemoveCheckInAsync(int userId, int habitId, DateOnly date, CancellationToken ct)
    {
        var habit = await _db.Habits
            .Include(h => h.CheckIns)
            .FirstOrDefaultAsync(h => h.Id == habitId && h.UserId == userId, ct);

        if (habit is null)
        {
            return CheckInWriteResult.NotFound;
        }

        if (habit.Archived)
        {
            return CheckInWriteResult.Archived;
        }

        var checkIn = habit.CheckIns.FirstOrDefault(c => c.Date == date);
        if (checkIn is not null)
        {
            _db.CheckIns.Remove(checkIn);
            await _db.SaveChangesAsync(ct);
        }

        return CheckInWriteResult.Done;
    }

    private static string? NormalizeNote(string? note) =>
        string.IsNullOrWhiteSpace(note) ? null : note.Trim();

    private static bool ValidColor(string? color) =>
        color is not null && Regex.IsMatch(color, "^#[0-9a-fA-F]{6}$");
}
