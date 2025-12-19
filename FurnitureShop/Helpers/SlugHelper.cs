using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
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

    public static async Task<string> GenerateUniqueSlugAsync<T>(
        IQueryable<T> query,
        string name,
        Expression<Func<T, string?>> slugSelector,
        Expression<Func<T, int>> idSelector,
        int? excludeId = null,
        int maxLen = 200,
        CancellationToken ct = default)
        where T : class
    {
        var baseSlug = GenerateSlug(name, maxLen);
        if (string.IsNullOrWhiteSpace(baseSlug)) return "";

        // Build query: slug == candidate (+ exclude self if needed)
        async Task<bool> ExistsAsync(string candidate)
        {
            var q = query.AsNoTracking();

            // x => x.Slug == candidate
            var param = Expression.Parameter(typeof(T), "x");
            var slugBody = Expression.Invoke(slugSelector, param);
            var eqSlug = Expression.Equal(
                slugBody,
                Expression.Constant(candidate, typeof(string))
            );

            Expression body = eqSlug;

            if (excludeId.HasValue)
            {
                // x => x.Id != excludeId
                var idBody = Expression.Invoke(idSelector, param);
                var neqId = Expression.NotEqual(
                    idBody,
                    Expression.Constant(excludeId.Value)
                );

                body = Expression.AndAlso(body, neqId);
            }

            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return await q.AnyAsync(lambda, ct);
        }

        // nếu baseSlug chưa tồn tại => dùng luôn
        if (!await ExistsAsync(baseSlug))
            return baseSlug;

        // tồn tại => thêm -2, -3...
        var i = 2;
        while (true)
        {
            var suffix = "-" + i;
            var cutLen = Math.Max(1, maxLen - suffix.Length);
            var candidate = (baseSlug.Length > cutLen ? baseSlug[..cutLen] : baseSlug).Trim('-') + suffix;

            if (!await ExistsAsync(candidate))
                return candidate;

            i++;
        }
    }
}
