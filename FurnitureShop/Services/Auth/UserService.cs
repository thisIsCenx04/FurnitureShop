using BCrypt.Net;
using FurnitureShop.Models;
using FurnitureShop.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Services.Auth;

public sealed class UserService : IUserService
{
    private readonly FurnitureShopContext _db;

    public UserService(FurnitureShopContext db) => _db = db;

    public Task<User?> FindByLoginAsync(string userNameOrEmail)
    {
        var key = (userNameOrEmail ?? "").Trim();
        return _db.Users.FirstOrDefaultAsync(u => u.UserName == key || u.Email == key);
    }

    public Task<User?> FindByIdAsync(string id)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<(bool Ok, string Error)> RegisterAsync(User user, string rawPassword)
    {
        user.UserName = user.UserName.Trim();
        user.Email = user.Email.Trim();

        // Check duplicate (DB có unique thì càng tốt, nhưng vẫn check trước để báo lỗi đẹp)
        var existsUserName = await _db.Users.AnyAsync(u => u.UserName == user.UserName);
        if (existsUserName) return (false, "UserName đã tồn tại.");

        var existsEmail = await _db.Users.AnyAsync(u => u.Email == user.Email);
        if (existsEmail) return (false, "Email đã tồn tại.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);
        user.CreatedAt = DateTime.UtcNow;

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return (true, "");
    }

    public Task<bool> VerifyPasswordAsync(User user, string rawPassword)
        => Task.FromResult(BCrypt.Net.BCrypt.Verify(rawPassword, user.PasswordHash));

    public async Task<(bool Ok, string Error)> UpdateProfileAsync(string id, string? fullName, string? phone, string? address)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (u == null) return (false, "Không tìm thấy tài khoản.");

        u.FullName = fullName?.Trim();
        u.Phone = phone?.Trim();
        u.Address = address?.Trim();
        u.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (true, "");
    }
}
