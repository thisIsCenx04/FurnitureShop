namespace FurnitureShop.Models.Enums;

public enum OrderStatus
{
    Pending = 0,      // Đã tiếp nhận
    Confirmed = 1,    // Đã xác nhận
    Shipping = 2,     // Đang giao
    Completed = 3,    // Hoàn thành
    Cancelled = 4     // Đã hủy (dự phòng)
}
