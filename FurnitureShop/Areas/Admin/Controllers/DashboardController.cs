using FurnitureShop.ViewModels.Admin.Dashboard;
using FurnitureShop.Helpers;
using FurnitureShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Areas.Admin.Controllers;

[Area("Admin")]
public sealed class DashboardController : Controller
{
    private readonly FurnitureShopContext _db;

    public DashboardController(FurnitureShopContext db) => _db = db;

    // GET: /Admin  OR /Admin/Dashboard
    [HttpGet("/Admin")]
    [HttpGet("/Admin/Dashboard")]
    public async Task<IActionResult> Index(DateTime? from, DateTime? to, int top = 10)
    {
        if (top <= 0 || top > 50) top = 10;

        // range mặc định: 30 ngày gần nhất
        var toDate = (to ?? DateTime.Today).Date.AddDays(1).AddTicks(-1); // end of day
        var fromDate = (from ?? DateTime.Today.AddDays(-29)).Date;        // start day

        // Base orders (trong khoảng)
        var ordersInRange = _db.Orders.AsNoTracking()
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate);

        // Tổng doanh thu trong range: Subtotal + ShippingFee
        // (giả sử Subtotal/ShippingFee là decimal không nullable)
        var totalRevenue = await ordersInRange
            .Select(o => o.Subtotal + o.ShippingFee)
            .SumAsync();

        var totalOrders = await ordersInRange.CountAsync();

        // 1) Revenue by day
        var revenueByDay = await ordersInRange
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new
            {
                Day = g.Key,
                Revenue = g.Sum(x => x.Subtotal + x.ShippingFee)
            })
            .OrderBy(x => x.Day)
            .ToListAsync();

        // Fill missing days (để chart/table không bị trống)
        var dayPoints = new List<RevenuePointVm>();
        for (var d = fromDate.Date; d <= toDate.Date; d = d.AddDays(1))
        {
            var hit = revenueByDay.FirstOrDefault(x => x.Day == d);
            dayPoints.Add(new RevenuePointVm
            {
                Label = d.ToString("yyyy-MM-dd"),
                Value = hit?.Revenue ?? 0m
            });
        }

        // 2) Orders count by status (trong range)
        var statusCountsRaw = await ordersInRange
            .GroupBy(o => o.Status)
            .Select(g => new { Status = (int)g.Key, Count = g.Count() })
            .OrderBy(x => x.Status)
            .ToListAsync();

        var statusCounts = statusCountsRaw
            .Select(x => new OrderStatusCountVm
            {
                Status = x.Status,
                StatusText = OrderStatusHelper.ToText(x.Status),
                Count = x.Count
            })
            .ToList();

        // 3) Top products best sellers (trong range)
        var topProducts = await (
            from oi in _db.OrderItems.AsNoTracking()
            join o in _db.Orders.AsNoTracking() on oi.OrderId equals o.Id
            join p in _db.Products.AsNoTracking() on oi.ProductId equals p.Id
            where o.CreatedAt >= fromDate && o.CreatedAt <= toDate
            group new { oi, p } by new { oi.ProductId, p.Name, p.Slug, p.MainImageUrl } into g
            select new TopProductVm
            {
                ProductId = g.Key.ProductId,
                Name = g.Key.Name,
                Slug = g.Key.Slug,
                MainImageUrl = g.Key.MainImageUrl,
                TotalQty = g.Sum(x => x.oi.Quantity),
                TotalAmount = g.Sum(x => (decimal)x.oi.UnitPrice * x.oi.Quantity)
            }
        )
        .OrderByDescending(x => x.TotalQty)
        .ThenByDescending(x => x.TotalAmount)
        .Take(top)
        .ToListAsync();

        // ===== Build arrays for charts (Chart.js) =====
        var revenueLabels = dayPoints.Select(x => x.Label).ToList();
        var revenueValues = dayPoints.Select(x => x.Value).ToList();

        var statusLabels = statusCounts.Select(x => x.StatusText).ToList();
        var statusValues = statusCounts.Select(x => x.Count).ToList();

        var topProductLabels = topProducts.Select(x => x.Name).ToList();
        var topProductQty = topProducts.Select(x => x.TotalQty).ToList();

        var vm = new DashboardVm
        {
            From = fromDate,
            To = toDate,

            RevenueByDay = dayPoints,
            OrdersByStatus = statusCounts,
            TopProducts = topProducts,

            TotalRevenueInRange = totalRevenue,
            TotalOrdersInRange = totalOrders,

            RevenueLabels = revenueLabels,
            RevenueValues = revenueValues,

            StatusLabels = statusLabels,
            StatusCounts = statusValues,

            TopProductLabels = topProductLabels,
            TopProductQty = topProductQty
        };

        return View(vm);
    }
}
