namespace FurnitureShop.ViewModels.Orders;

public sealed class OrderStatusHistoryVm
{
    public int FromStatus { get; set; }
    public int ToStatus { get; set; }
    public string? Note { get; set; }
    public DateTime ChangedAt { get; set; }
}
