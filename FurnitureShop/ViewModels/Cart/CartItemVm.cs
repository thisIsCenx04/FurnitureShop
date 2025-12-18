namespace FurnitureShop.ViewModels.Cart;

public sealed class CartItemVm
{
    public long CartItemId { get; set; }
    public long CartId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string ProductSlug { get; set; } = "";
    public string? ImageUrl { get; set; }

    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
    public int Stock { get; set; }
    public bool OutOfStock => Stock <= 0;

}
