using System.Globalization;
using System.Security.Claims;
using Daymark.Api.Dtos;
using Daymark.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Daymark.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/habits")]
public class HabitsController : ControllerBase
{
    private readonly HabitService _habits;

    public HabitsController(HabitService habits)
    {
        _habits = habits;
    }

    [HttpGet]
    public async Task<ActionResult<List<HabitDto>>> GetHabits(
        [FromQuery] bool includeArchived = false, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return Ok(await _habits.GetHabitsAsync(CurrentUserId(), includeArchived, today, ct));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<HabitDetailDto>> GetHabit(
        int id, [FromQuery] string? from = null, [FromQuery] string? to = null, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (!TryParseRange(from, to, today, out var rangeStart, out var rangeEnd))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Dates must use yyyy-MM-dd and 'from' can't be after 'to'.");
        }

        var habit = await _habits.GetHabitAsync(CurrentUserId(), id, rangeStart, rangeEnd, today, ct);
        return habit is null ? NotFound() : Ok(habit);
    }

    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitRequest request, CancellationToken ct)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (name.Length is 0 or > 80)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Name must be between 1 and 80 characters.");
        }

        if (request.Note is { Length: > 500 })
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Note can't be longer than 500 characters.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var habit = await _habits.CreateAsync(CurrentUserId(), request, today, ct);
        return StatusCode(StatusCodes.Status201Created, habit);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<HabitDto>> UpdateHabit(int id, UpdateHabitRequest request, CancellationToken ct)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (name.Length is 0 or > 80)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Name must be between 1 and 80 characters.");
        }

        if (request.Note is { Length: > 500 })
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Note can't be longer than 500 characters.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var habit = await _habits.UpdateAsync(CurrentUserId(), id, request, today, ct);
        return habit is null ? NotFound() : Ok(habit);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteHabit(int id, CancellationToken ct)
    {
        var deleted = await _habits.DeleteAsync(CurrentUserId(), id, ct);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/check-ins")]
    public async Task<IActionResult> AddCheckIn(int id, CheckInRequest request, CancellationToken ct)
    {
        if (!TryParseDate(request.Date, out var date))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Date must use yyyy-MM-dd.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (date > today)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "You can't check in for a future date.");
        }

        if (request.Note is { Length: > 300 })
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Note can't be longer than 300 characters.");
        }

        var result = await _habits.AddCheckInAsync(CurrentUserId(), id, date, request.Note, ct);
        if (result == CheckInWriteResult.NotFound)
        {
            return NotFound();
        }

        if (result == CheckInWriteResult.Archived)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                detail: "This habit is archived. Unarchive it to keep checking in.");
        }

        return Ok(new { checkedIn = true });
    }

    [HttpDelete("{id:int}/check-ins/{date}")]
    public async Task<IActionResult> RemoveCheckIn(int id, string date, CancellationToken ct)
    {
        if (!TryParseDate(date, out var parsed))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Date must use yyyy-MM-dd.");
        }

        var result = await _habits.RemoveCheckInAsync(CurrentUserId(), id, parsed, ct);
        if (result == CheckInWriteResult.NotFound)
        {
            return NotFound();
        }

        if (result == CheckInWriteResult.Archived)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                detail: "This habit is archived. Unarchive it to change its history.");
        }

        return Ok(new { checkedIn = false });
    }

    private int CurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null && int.TryParse(claim.Value, out var id) ? id : 0;
    }

    private static bool TryParseDate(string? value, out DateOnly date) =>
        DateOnly.TryParseExact(
            value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    private static bool TryParseRange(string? from, string? to, DateOnly today, out DateOnly rangeStart, out DateOnly rangeEnd)
    {
        rangeStart = today.AddDays(-83);
        rangeEnd = today;

        if (from is not null && !TryParseDate(from, out rangeStart))
        {
            return false;
        }

        if (to is not null && !TryParseDate(to, out rangeEnd))
        {
            return false;
        }

        return rangeStart <= rangeEnd;
    }
}
