using FurnitureShop.ViewModels.Cart;

namespace FurnitureShop.Services.Cart;

public interface ICartService
{
    Task<CartSummaryVm> GetOrCreateCartAsync(string? userId, string cartKey);
    Task AddToCartAsync(string? userId, string cartKey, int productId, int qty);
    Task UpdateQtyAsync(string? userId, string cartKey, long cartItemId, int qty);
    Task RemoveItemAsync(string? userId, string cartKey, long cartItemId);
    Task ClearCartAsync(long cartId);
}
