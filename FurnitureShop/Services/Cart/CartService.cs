using FurnitureShop.Models;
using FurnitureShop.ViewModels.Cart;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Services.Cart;

public sealed class CartService : ICartService
{
    private readonly FurnitureShopContext _db;
    public CartService(FurnitureShopContext db) => _db = db;

    public async Task<CartSummaryVm> GetOrCreateCartAsync(string? userId, string cartKey)
    {
        var cart = await _db.Carts
            .FirstOrDefaultAsync(c =>
                (!string.IsNullOrEmpty(userId) && c.UserId == userId) ||
                (string.IsNullOrEmpty(userId) && c.CartKey == cartKey));

        if (cart == null)
        {
            cart = new Models.Entities.Cart
            {
                UserId = userId,
                CartKey = cartKey,
                CreatedAt = DateTime.UtcNow
            };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
        }

        var items = await (
            from ci in _db.CartItems.AsNoTracking()
            join p in _db.Products.AsNoTracking() on ci.ProductId equals p.Id
            where ci.CartId == cart.Id
            select new CartItemVm
            {
                CartItemId = ci.Id,
                ProductId = p.Id,
                ProductName = p.Name,
                ProductSlug = p.Slug,
                ImageUrl = p.MainImageUrl,
                UnitPrice = (p.SalePrice ?? p.Price),
                Quantity = ci.Quantity,
                Stock = p.Stock
            }
        ).ToListAsync();


        return new CartSummaryVm
        {
            CartId = cart.Id,                    
            CartKey = cart.CartKey,
            Items = items
        };
    }

    public async Task AddToCartAsync(string? userId, string cartKey, int productId, int qty)
    {
        if (qty <= 0) qty = 1;

        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId && x.IsActive);
        if (p == null) throw new InvalidOperationException("Product not found.");

        if (p.Stock <= 0) throw new InvalidOperationException("Sản phẩm đã hết hàng.");
        if (qty > p.Stock) qty = p.Stock;

        var cartVm = await GetOrCreateCartAsync(userId, cartKey);
        var cartId = cartVm.CartId;

        var item = await _db.CartItems.FirstOrDefaultAsync(x => x.CartId == cartId && x.ProductId == productId);

        if (item == null)
        {
            _db.CartItems.Add(new Models.Entities.CartItem
            {
                CartId = cartId,
                ProductId = productId,
                Quantity = qty,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            var newQty = item.Quantity + qty;
            item.Quantity = newQty > p.Stock ? p.Stock : newQty;
        }

        await _db.SaveChangesAsync();
    }


    public async Task UpdateQtyAsync(string? userId, string cartKey, long cartItemId, int qty)
    {
        if (qty <= 0) qty = 1;

        var cartVm = await GetOrCreateCartAsync(userId, cartKey);

        var item = await _db.CartItems.FirstOrDefaultAsync(x => x.Id == cartItemId && x.CartId == cartVm.CartId);
        if (item == null) return;

        var p = await _db.Products.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == item.ProductId && x.IsActive);

        if (p == null) { _db.CartItems.Remove(item); await _db.SaveChangesAsync(); return; }

        if (p.Stock <= 0)
        {
            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
            return;
        }

        if (qty > p.Stock) qty = p.Stock;

        item.Quantity = qty;
        await _db.SaveChangesAsync();
    }


    public async Task RemoveItemAsync(string? userId, string cartKey, long cartItemId)
    {
        var cartVm = await GetOrCreateCartAsync(userId, cartKey);

        var item = await _db.CartItems.FirstOrDefaultAsync(x => x.Id == cartItemId && x.CartId == cartVm.CartId);
        if (item == null) return;

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task ClearCartAsync(long cartId)
    {
        var items = await _db.CartItems.Where(x => x.CartId == cartId).ToListAsync();
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }
}
