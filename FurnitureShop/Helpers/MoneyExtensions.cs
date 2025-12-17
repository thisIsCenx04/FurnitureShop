using System.Globalization;

namespace FurnitureShop.Helpers;

public static class MoneyExtensions
{
    private static readonly CultureInfo ViCulture = new("vi-VN");

    // hiển thị dạng: 1.234.567 ₫
    public static string ToVnd(this decimal value)
        => string.Format(ViCulture, "{0:c0}", value);

    // hiển thị dạng: 1.234.567 (không ký hiệu)
    public static string ToNumber(this decimal value)
        => value.ToString("N0", ViCulture);
}
