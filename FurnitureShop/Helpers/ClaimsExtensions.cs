using System.Security.Claims;

namespace FurnitureShop.Helpers;

public static class ClaimsExtensions
{
    public static string? GetUserId(this ClaimsPrincipal user)
        => user.FindFirstValue("uid");

    public static string? GetUserIdOrNull(this ClaimsPrincipal user)
    {
        return user?.Identity?.IsAuthenticated == true
            ? user.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;
    }

    public static string? GetUserName(this ClaimsPrincipal user)
    {
        return user?.Identity?.IsAuthenticated == true
            ? user.Identity!.Name
            : null;
    }

    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole("Admin");
    }
}
