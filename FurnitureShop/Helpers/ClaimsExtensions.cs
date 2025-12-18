using System.Security.Claims;

namespace FurnitureShop.Helpers;

public static class ClaimsExtensions
{
    public static string? GetUserId(this ClaimsPrincipal user)
        => user.FindFirstValue("uid");

    public static bool IsAdmin(this ClaimsPrincipal user)
        => user.FindFirstValue("role") == "1";
}
