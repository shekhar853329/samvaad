using System.Security.Claims;

namespace samvaad_backend.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? user.FindFirst("sub")?.Value;

        if (value is null || !Guid.TryParse(value, out var id))
            throw new InvalidOperationException("User ID claim is missing or invalid.");

        return id;
    }
}
