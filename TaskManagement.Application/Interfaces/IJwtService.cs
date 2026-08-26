using System.Security.Claims;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);

    // ✅ Yeh 2 naye methods add karo
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}