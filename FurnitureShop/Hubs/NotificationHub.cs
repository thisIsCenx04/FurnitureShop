using Microsoft.AspNetCore.SignalR;

namespace FurnitureShop.Hubs;

public sealed class NotificationHub : Hub
{
    // Module 7 sẽ dùng group "Admins" và "user:{id}"
    public static string AdminGroup => "Admins";
    public static string UserGroup(string userId) => $"user:{userId}";
}
