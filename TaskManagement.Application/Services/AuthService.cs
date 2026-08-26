using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using FirebaseAdmin.Auth;
namespace TaskManagement.Application.Services;

public class AuthService : IAuthService  // 👈 add this
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;

    public AuthService(IUnitOfWork uow, IJwtService jwt)
    {
        _uow = uow;
        _jwt = jwt;
    }

    public async Task<AuthResponseWithRefreshDto> RegisterAsync(RegisterDto dto)
    {
        if (await _uow.Users.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "User"
        };

        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponseWithRefreshDto> LoginAsync(LoginDto dto)
    {
        var user = await _uow.Users.GetByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponseWithRefreshDto> RefreshTokenAsync(RefreshTokenRequestDto dto)
    {
        var storedToken = await _uow.Users.GetRefreshTokenAsync(dto.RefreshToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (!storedToken.IsActive)
            throw new UnauthorizedAccessException("Refresh token expired or revoked.");

        var user = storedToken.User;

        storedToken.IsRevoked = true;

        return await GenerateTokensAsync(user);
    }

    private async Task<AuthResponseWithRefreshDto> GenerateTokensAsync(User user)
    {
        var accessToken = _jwt.GenerateToken(user);
        var refreshToken = _jwt.GenerateRefreshToken();

        var tokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Users.AddRefreshTokenAsync(tokenEntity);
        await _uow.SaveChangesAsync();

        return new AuthResponseWithRefreshDto(
            accessToken,
            refreshToken,
            user.Name,
            user.Email,
            user.Role
        );
    }
    public async Task<AuthResponseWithRefreshDto> FacebookLoginAsync(string accessToken)
    {
        // Facebook Graph API se user info lo
        using var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync(
            $"https://graph.facebook.com/me?fields=id,name,email&access_token={accessToken}");

        var fbUser = System.Text.Json.JsonSerializer.Deserialize<FacebookUserInfo>(
            response,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (fbUser == null || string.IsNullOrEmpty(fbUser.Email))
            throw new UnauthorizedAccessException("Facebook token invalid hai.");

        // User exist karta hai check karo
        var user = await _uow.Users.GetByEmailAsync(fbUser.Email);

        if (user == null)
        {
            // Naya user banao
            user = new User
            {
                Name = fbUser.Name,
                Email = fbUser.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                Role = "User"
            };

            await _uow.Users.AddAsync(user);
            await _uow.SaveChangesAsync();
        }

        return await GenerateTokensAsync(user);
    }
    public async Task<AuthResponseWithRefreshDto> GoogleLoginAsync(string idToken)
    {
        var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

        var email = decodedToken.Claims["email"].ToString()!;
        var name = decodedToken.Claims["name"].ToString()!;

        var user = await _uow.Users.GetByEmailAsync(email);

        if (user == null)
        {
            user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                Role = "User"
            };
            await _uow.Users.AddAsync(user);
            await _uow.SaveChangesAsync();
        }

        return await GenerateTokensAsync(user);
    }
}