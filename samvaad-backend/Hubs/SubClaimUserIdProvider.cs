using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;

namespace samvaad_backend.Hubs;

/// <summary>
/// Program.cs sets MapInboundClaims = false, so the JWT "sub" claim is NOT
/// auto-mapped to ClaimTypes.NameIdentifier. SignalR's default IUserIdProvider
/// looks for ClaimTypes.NameIdentifier and finds nothing, making Clients.User()
/// route to zero connections. This provider reads "sub" directly so that
/// Clients.User(userId) correctly routes to the right user's connections.
/// </summary>
public sealed class SubClaimUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
        => connection.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? connection.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
}
