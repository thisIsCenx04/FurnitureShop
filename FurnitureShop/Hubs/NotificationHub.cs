using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace FurnitureShop.Hubs;

public sealed class NotificationHub : Hub
{
    public static string AdminGroup => "Admins";
    public static string UserGroup(string userId) => $"user:{userId}";

    public override async Task OnConnectedAsync()
    {
        if (Context.User?.Identity?.IsAuthenticated == true)
        {
            var uid = Context.User.FindFirst("uid")?.Value;
            var role = Context.User.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrWhiteSpace(uid))
                await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(uid));

            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
        }

        await base.OnConnectedAsync();
    }
}
