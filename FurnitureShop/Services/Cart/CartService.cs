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
        global::FurnitureShop.Models.Entities.Cart? userCart = null;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            userCart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
        }

        var keyCart = await _db.Carts.FirstOrDefaultAsync(c => c.CartKey == cartKey);

        global::FurnitureShop.Models.Entities.Cart cart;

        if (userCart == null && keyCart != null)
        {
            if (!string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(keyCart.UserId))
            {
                keyCart.UserId = userId;
                await _db.SaveChangesAsync();
            }

            cart = keyCart;
        }
        else if (userCart != null && keyCart != null && userCart.Id != keyCart.Id)
        {
            var userItems = await _db.CartItems.Where(x => x.CartId == userCart.Id).ToListAsync();
            var guestItems = await _db.CartItems.Where(x => x.CartId == keyCart.Id).ToListAsync();

            foreach (var gi in guestItems)
            {
                var exist = userItems.FirstOrDefault(x => x.ProductId == gi.ProductId);
                if (exist == null)
                {
                    gi.CartId = userCart.Id;
                }
                else
                {
                    exist.Quantity += gi.Quantity;
                    _db.CartItems.Remove(gi);
                }
            }

            _db.Carts.Remove(keyCart);

            await _db.SaveChangesAsync();
            cart = userCart;
        }
        else if (userCart != null)
        {
            cart = userCart;
        }
        else if (keyCart == null)
        {
            cart = new global::FurnitureShop.Models.Entities.Cart
            {
                CartKey = cartKey,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Carts.Add(cart);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                cart = await _db.Carts.FirstAsync(c => c.CartKey == cartKey);
            }
        }
        else
        {
            cart = keyCart;
        }

        var items = await (
            from ci in _db.CartItems.AsNoTracking()
            join p in _db.Products.AsNoTracking() on ci.ProductId equals p.Id
            where ci.CartId == cart.Id
            select new CartItemVm
            {
                CartItemId = ci.Id,
                CartId = ci.CartId,
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
            _db.CartItems.Add(new global::FurnitureShop.Models.Entities.CartItem
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

        if (p == null)
        {
            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
            return;
        }

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
