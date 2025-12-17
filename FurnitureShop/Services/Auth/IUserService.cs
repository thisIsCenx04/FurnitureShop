using FurnitureShop.Models.Entities;

namespace FurnitureShop.Services.Auth;

public interface IUserService
{
    Task<User?> FindByLoginAsync(string userNameOrEmail);
    Task<User?> FindByIdAsync(string id);

    Task<(bool Ok, string Error)> RegisterAsync(User user, string rawPassword);
    Task<bool> VerifyPasswordAsync(User user, string rawPassword);

    Task<(bool Ok, string Error)> UpdateProfileAsync(string id, string? fullName, string? phone, string? address);
}
