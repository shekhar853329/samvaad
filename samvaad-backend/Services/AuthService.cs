using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using samvaad_backend.Common;
using samvaad_backend.Data;
using samvaad_backend.Models.DTOs.Auth;
using samvaad_backend.Models.Entities;
using samvaad_backend.Services.Interfaces;

namespace samvaad_backend.Services;

public class AuthService(AppDbContext db, IConfiguration config) : IAuthService
{
    private readonly int _accessTokenMinutes = int.Parse(config["Jwt:AccessTokenMinutes"] ?? "15");
    private readonly int _refreshTokenDays = int.Parse(config["Jwt:RefreshTokenDays"] ?? "7");

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var usernameTaken = await db.Users.AnyAsync(u => u.Username == request.Username);
        if (usernameTaken)
            throw new AppException("Username is already taken.", 409);

        var emailTaken = await db.Users.AnyAsync(u => u.Email == request.Email);
        if (emailTaken)
            throw new AppException("An account with this email already exists.", 409);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            DisplayName = request.DisplayName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            JoinedAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow
        };

        // Create default preferences alongside the new user
        user.NotificationPreferences = new UserNotificationPreferences { UserId = user.Id };
        user.PrivacySettings = new UserPrivacySettings { UserId = user.Id };
        user.FeedPreferences = new UserFeedPreferences { UserId = user.Id };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var identifier = request.Identifier.Trim().ToLowerInvariant();

        var user = await db.Users.FirstOrDefaultAsync(u =>
            u.Email == identifier || u.Username == identifier);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new AppException("Invalid credentials.", 401);

        user.LastSeenAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);

        var stored = await db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (stored is null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
            throw new AppException("Invalid or expired refresh token.", 401);

        // Rotate — revoke old, issue new
        stored.IsRevoked = true;
        stored.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return await IssueTokensAsync(stored.User);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);

        var stored = await db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (stored is not null && !stored.IsRevoked)
        {
            stored.IsRevoked = true;
            stored.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<AuthResponse> IssueTokensAsync(User user)
    {
        var accessExpiry = DateTime.UtcNow.AddMinutes(_accessTokenMinutes);
        var accessToken = BuildAccessToken(user, accessExpiry);

        var rawRefresh = GenerateSecureToken();
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = HashToken(rawRefresh),
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenDays),
            CreatedAt = DateTime.UtcNow
        };

        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = rawRefresh,
            ExpiresAt = accessExpiry,
            User = new UserSummary
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                IsVerified = user.IsVerified
            }
        };
    }

    private string BuildAccessToken(User user, DateTime expiry)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Secret"]
                ?? throw new InvalidOperationException("Jwt:Secret is not configured.")));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("username", user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiry,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
