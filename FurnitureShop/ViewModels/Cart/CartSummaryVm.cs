namespace FurnitureShop.ViewModels.Cart;

public sealed class CartSummaryVm
{
    public long CartId { get; set; }
    public string CartKey { get; set; } = "";
    public List<CartItemVm> Items { get; set; } = new();
    public decimal Subtotal => Items.Sum(x => x.LineTotal);
    public int TotalQty => Items.Sum(x => x.Quantity);
}
