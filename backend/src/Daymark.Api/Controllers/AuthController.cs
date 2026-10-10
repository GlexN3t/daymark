using System.Net.Mail;
using System.Security.Claims;
using Daymark.Api.Data;
using Daymark.Api.Domain;
using Daymark.Api.Dtos;
using Daymark.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Daymark.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly DaymarkDbContext _db;
    private readonly TokenService _tokens;

    public AuthController(DaymarkDbContext db, TokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var displayName = request.DisplayName?.Trim() ?? string.Empty;

        if (!IsValidInput(email, displayName, request.Password, out var errors))
        {
            return ValidationProblem(errors);
        }

        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                detail: "That email is already registered.");
        }

        var user = new User
        {
            Email = email,
            DisplayName = displayName,
            PasswordHash = PasswordHasher.Hash(request.Password!),
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        var response = new AuthResponse(_tokens.CreateToken(user), UserDto.From(user));
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is null || !PasswordHasher.Verify(request.Password ?? string.Empty, user.PasswordHash))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                detail: "Email or password is incorrect.");
        }

        return Ok(new AuthResponse(_tokens.CreateToken(user), UserDto.From(user)));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == CurrentUserId(), ct);

        return user is null ? Unauthorized() : Ok(UserDto.From(user));
    }

    private int CurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null && int.TryParse(claim.Value, out var id) ? id : 0;
    }

    private static bool IsValidInput(string email, string displayName, string? password, out ModelStateDictionary errors)
    {
        errors = new ModelStateDictionary();

        if (!IsValidEmail(email))
        {
            errors.AddModelError("email", "Enter a valid email address.");
        }

        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > 80)
        {
            errors.AddModelError("displayName", "Display name must be between 1 and 80 characters.");
        }

        if (string.IsNullOrEmpty(password) || password.Length < 8)
        {
            errors.AddModelError("password", "Password must be at least 8 characters.");
        }

        return errors.Count == 0;
    }

    private static bool IsValidEmail(string email)
    {
        if (email.Length is 0 or > 256 || !email.Contains('@'))
        {
            return false;
        }

        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
