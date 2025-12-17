using System.Security.Claims;

namespace FurnitureShop.Helpers;

public static class ClaimsExtensions
{
    public static string? GetUserId(this ClaimsPrincipal user)
        => user.FindFirst("uid")?.Value;

    public static bool IsAdmin(this ClaimsPrincipal user)
        => string.Equals(user.FindFirst(ClaimTypes.Role)?.Value, "Admin", StringComparison.OrdinalIgnoreCase);
}
