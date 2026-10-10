using System.Security.Claims;
using Daymark.Api.Dtos;
using Daymark.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Daymark.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/stats")]
public class StatsController : ControllerBase
{
    private readonly StatsService _stats;

    public StatsController(StatsService stats)
    {
        _stats = stats;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<SummaryDto>> GetSummary(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return Ok(await _stats.GetSummaryAsync(CurrentUserId(), today, ct));
    }

    [HttpGet("calendar")]
    public async Task<ActionResult<List<DayCountDto>>> GetCalendar([FromQuery] int? year, CancellationToken ct)
    {
        var calendarYear = year ?? DateTime.UtcNow.Year;
        if (calendarYear is < 2000 or > 2100)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Year must be between 2000 and 2100.");
        }

        return Ok(await _stats.GetCalendarAsync(CurrentUserId(), calendarYear, ct));
    }

    private int CurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null && int.TryParse(claim.Value, out var id) ? id : 0;
    }
}
