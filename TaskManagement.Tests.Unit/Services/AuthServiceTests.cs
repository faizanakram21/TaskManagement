using FluentAssertions;
using Moq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using Xunit;

namespace TaskManagement.Tests.Unit.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<IJwtService> _jwtMock = new();
    private readonly AuthService _sut; // "system under test"

    public AuthServiceTests()
    {
        _uowMock.Setup(u => u.Users).Returns(_usersMock.Object);
        _sut = new AuthService(_uowMock.Object, _jwtMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsInvalidOperationException()
    {
        _usersMock.Setup(r => r.EmailExistsAsync("taken@example.com")).ReturnsAsync(true);

        var act = () => _sut.RegisterAsync(new RegisterDto("Ali", "taken@example.com", "Pass@123"));

        await act.Should().ThrowAsync<InvalidOperationException>();
        _usersMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_CreatesUserAndReturnsTokens()
    {
        _usersMock.Setup(r => r.EmailExistsAsync("new@example.com")).ReturnsAsync(false);
        _jwtMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("access-token-123");
        _jwtMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token-456");

        var result = await _sut.RegisterAsync(new RegisterDto("Ali", "new@example.com", "Pass@123"));

        result.AccessToken.Should().Be("access-token-123");
        result.RefreshToken.Should().Be("refresh-token-456");
        result.Email.Should().Be("new@example.com");
        result.Role.Should().Be("User");
        _usersMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == "new@example.com")), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ThrowsUnauthorizedAccessException()
    {
        _usersMock.Setup(r => r.GetByEmailAsync("ghost@example.com")).ReturnsAsync((User?)null);

        var act = () => _sut.LoginAsync(new LoginDto("ghost@example.com", "Whatever@123"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorizedAccessException()
    {
        var user = new User
        {
            Id = 1,
            Name = "Ali",
            Email = "ali@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass@123"),
            Role = "User"
        };
        _usersMock.Setup(r => r.GetByEmailAsync("ali@example.com")).ReturnsAsync(user);

        var act = () => _sut.LoginAsync(new LoginDto("ali@example.com", "WrongPass@999"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokens()
    {
        var user = new User
        {
            Id = 1,
            Name = "Ali",
            Email = "ali@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass@123"),
            Role = "User"
        };
        _usersMock.Setup(r => r.GetByEmailAsync("ali@example.com")).ReturnsAsync(user);
        _jwtMock.Setup(j => j.GenerateToken(user)).Returns("access-token");
        _jwtMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");

        var result = await _sut.LoginAsync(new LoginDto("ali@example.com", "CorrectPass@123"));

        result.AccessToken.Should().Be("access-token");
        result.Name.Should().Be("Ali");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithUnknownToken_ThrowsUnauthorizedAccessException()
    {
        _usersMock.Setup(r => r.GetRefreshTokenAsync("bad-token")).ReturnsAsync((RefreshToken?)null);

        var act = () => _sut.RefreshTokenAsync(new RefreshTokenRequestDto("bad-token"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredOrRevokedToken_ThrowsUnauthorizedAccessException()
    {
        var user = new User { Id = 1, Name = "Ali", Email = "ali@example.com", Role = "User", PasswordHash = "x" };
        var expiredToken = new RefreshToken
        {
            Token = "expired-token",
            UserId = 1,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // already expired -> IsActive should be false
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        };
        _usersMock.Setup(r => r.GetRefreshTokenAsync("expired-token")).ReturnsAsync(expiredToken);

        var act = () => _sut.RefreshTokenAsync(new RefreshTokenRequestDto("expired-token"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokensAndRevokesOldOne()
    {
        var user = new User { Id = 1, Name = "Ali", Email = "ali@example.com", Role = "User", PasswordHash = "x" };
        var validToken = new RefreshToken
        {
            Token = "good-token",
            UserId = 1,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
        _usersMock.Setup(r => r.GetRefreshTokenAsync("good-token")).ReturnsAsync(validToken);
        _jwtMock.Setup(j => j.GenerateToken(user)).Returns("new-access");
        _jwtMock.Setup(j => j.GenerateRefreshToken()).Returns("new-refresh");

        var result = await _sut.RefreshTokenAsync(new RefreshTokenRequestDto("good-token"));

        result.AccessToken.Should().Be("new-access");
        validToken.IsRevoked.Should().BeTrue("the old refresh token must be revoked once it's used");
    }
}