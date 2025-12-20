namespace FurnitureShop.ViewModels.Orders;

public sealed class OrderDetailVm
{
    public long Id { get; set; }
    public string OrderCode { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public string CustomerName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public string Address { get; set; } = "";

    public int Status { get; set; }
    public int PaymentMethod { get; set; }

    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }

    public List<OrderItemVm> Items { get; set; } = new();
    public List<OrderStatusHistoryVm> Timeline { get; set; } = new();
}
