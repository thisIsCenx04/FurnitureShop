namespace FurnitureShop.ViewModels.Orders;

public sealed class MyOrdersItemVm
{
    public long Id { get; set; }
    public string OrderCode { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public int Status { get; set; }
    public decimal Total { get; set; }
}
