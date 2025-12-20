namespace FurnitureShop.ViewModels.Admin.Dashboard;

public sealed class DashboardVm
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public List<RevenuePointVm> RevenueByDay { get; set; } = new();
    public List<OrderStatusCountVm> OrdersByStatus { get; set; } = new();
    public List<TopProductVm> TopProducts { get; set; } = new();

    public decimal TotalRevenueInRange { get; set; }
    public int TotalOrdersInRange { get; set; }

    // ====== for Chart.js ======
    public List<string> RevenueLabels { get; set; } = new();
    public List<decimal> RevenueValues { get; set; } = new();

    public List<string> StatusLabels { get; set; } = new();
    public List<int> StatusCounts { get; set; } = new();

    public List<string> TopProductLabels { get; set; } = new();
    public List<int> TopProductQty { get; set; } = new();
}

public sealed class RevenuePointVm
{
    public string Label { get; set; } = ""; // "yyyy-MM-dd"
    public decimal Value { get; set; }
}

public sealed class OrderStatusCountVm
{
    public int Status { get; set; }
    public string StatusText { get; set; } = "";
    public int Count { get; set; }
}

public sealed class TopProductVm
{
    public int ProductId { get; set; }
    public string Name { get; set; } = "";
    public string? Slug { get; set; }
    public string? MainImageUrl { get; set; }

    public int TotalQty { get; set; }
    public decimal TotalAmount { get; set; }
}
