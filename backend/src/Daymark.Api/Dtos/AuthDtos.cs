using Daymark.Api.Domain;

namespace Daymark.Api.Dtos;

public record UserDto(int Id, string Email, string DisplayName)
{
    public static UserDto From(User user) => new(user.Id, user.Email, user.DisplayName);
}

public record RegisterRequest(string? Email, string? DisplayName, string? Password);

public record LoginRequest(string? Email, string? Password);

public record AuthResponse(string Token, UserDto User);
