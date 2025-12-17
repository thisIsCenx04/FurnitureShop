using System.Text;
using System.Text.RegularExpressions;

namespace FurnitureShop.Helpers;

public static class SlugHelper
{
    public static string GenerateSlug(string input, int maxLen = 200)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";

        var str = input.Trim().ToLowerInvariant();
        str = RemoveDiacritics(str);

        // bỏ ký tự không hợp lệ
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        // gom khoảng trắng
        str = Regex.Replace(str, @"\s+", " ").Trim();
        // cắt độ dài
        if (str.Length > maxLen) str = str[..maxLen].Trim();

        // thay space thành -
        str = str.Replace(" ", "-");
        // gom nhiều dấu - liên tiếp
        str = Regex.Replace(str, "-{2,}", "-").Trim('-');

        return str;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        // riêng chữ đ/Đ
        return sb.ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace("đ", "d")
            .Replace("Đ", "D");
    }
}
