using FurnitureShop.Models.Enums;

namespace FurnitureShop.Helpers;

public static class OrderStatusHelper
{
    public static string ToText(int status) => ((OrderStatus)status) switch
    {
        OrderStatus.Pending => "Đã tiếp nhận",
        OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.Shipping => "Đang giao",
        OrderStatus.Completed => "Hoàn thành",
        OrderStatus.Cancelled => "Đã hủy",
        _ => "Không xác định"
    };

    public static string ToBadgeClass(int status) => ((OrderStatus)status) switch
    {
        OrderStatus.Pending => "bg-secondary",
        OrderStatus.Confirmed => "bg-info",
        OrderStatus.Shipping => "bg-warning",
        OrderStatus.Completed => "bg-success",
        OrderStatus.Cancelled => "bg-danger",
        _ => "bg-dark"
    };
}
