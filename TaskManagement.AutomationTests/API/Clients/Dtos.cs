using System.Text.Json.Serialization;

namespace TaskManagement.AutomationTests.API.Clients;

public record RegisterDto(string Name, string Email, string Password);
public record LoginDto(string Email, string Password);

public record AuthResponseDto(
    [property: JsonPropertyName("accessToken")] string Token,
    string Name,
    string Email,
    string Role
);

public record RefreshTokenRequestDto(string RefreshToken);

public record TaskDto(int? Id, string Title, string? Description, bool IsCompleted, DateTime? DueDate);