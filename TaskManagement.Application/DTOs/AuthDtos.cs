using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs;

public record RegisterDto(
    [Required(ErrorMessage = "Name is required")]
    string Name,

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email,

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    string Password
);

public record LoginDto(string Email, string Password);
public record AuthResponseDto(string Token, string Name, string Email, string Role);

public record RefreshTokenRequestDto(string RefreshToken);

public record AuthResponseWithRefreshDto(
    string AccessToken,
    string RefreshToken,
    string Name,
    string Email,
    string Role
);
public record FacebookLoginDto(string AccessToken);