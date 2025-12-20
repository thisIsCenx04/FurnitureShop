using FurnitureShop.Hubs;
using FurnitureShop.Models;
using FurnitureShop.Models.Entities;
using Microsoft.AspNetCore.SignalR;
using System;

namespace FurnitureShop.Services.Notifications;

public sealed class NotificationService
{
    private readonly FurnitureShopContext _db;
    private readonly IHubContext<NotificationHub> _hub;

    public NotificationService(FurnitureShopContext db, IHubContext<NotificationHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    public async Task NotifyAdminsAsync(string type, string title, string message, string? url = null)
    {
        // Notifications.UserId null = broadcast for admins :contentReference[oaicite:5]{index=5}
        _db.Notifications.Add(new Notification
        {
            UserId = null,
            Type = type,
            Title = title,
            Message = message,
            Url = url,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        await _hub.Clients.Group(NotificationHub.AdminGroup)
            .SendAsync("notify", new { type, title, message, url, createdAt = DateTime.UtcNow });
    }

    public async Task NotifyUserAsync(string userId, string type, string title, string message, string? url = null)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Url = url,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        await _hub.Clients.Group(NotificationHub.UserGroup(userId))
            .SendAsync("notify", new { type, title, message, url, createdAt = DateTime.UtcNow });
    }
}
