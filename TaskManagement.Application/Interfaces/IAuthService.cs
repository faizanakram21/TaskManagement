using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseWithRefreshDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseWithRefreshDto> LoginAsync(LoginDto dto);
    Task<AuthResponseWithRefreshDto> RefreshTokenAsync(RefreshTokenRequestDto dto);
    Task<AuthResponseWithRefreshDto> FacebookLoginAsync(string accessToken);
    Task<AuthResponseWithRefreshDto> GoogleLoginAsync(string idToken);
}