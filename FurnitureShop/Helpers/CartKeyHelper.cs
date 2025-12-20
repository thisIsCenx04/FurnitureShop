using Microsoft.AspNetCore.Http;

namespace FurnitureShop.Helpers;

public static class CartKeyHelper
{
    public const string CookieName = "CartKey";

    public static string GetOrCreateCartKey(HttpContext http)
    {
        if (http.Request.Cookies.TryGetValue(CookieName, out var key) && !string.IsNullOrWhiteSpace(key))
            return key;

        key = Guid.NewGuid().ToString("N");
        http.Response.Cookies.Append(CookieName, key, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = http.Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });

        return key;
    }

    public static string GetOrSetCartKey(HttpContext http) => GetOrCreateCartKey(http);

    public static string? TryGetCartKey(HttpContext http)
        => http.Request.Cookies.TryGetValue(CookieName, out var key) ? key : null;
}
