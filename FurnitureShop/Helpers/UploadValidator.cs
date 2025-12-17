namespace FurnitureShop.Helpers;

public static class UploadValidator
{
    public static (bool Ok, string Error) ValidateImage(
        IFormFile? file,
        long maxBytes = 5 * 1024 * 1024,
        string[]? allowedExtensions = null,
        string[]? allowedContentTypes = null)
    {
        if (file == null || file.Length == 0)
            return (false, "File rỗng hoặc chưa chọn file.");

        if (file.Length > maxBytes)
            return (false, $"File quá lớn. Tối đa {maxBytes / (1024 * 1024)}MB.");

        allowedExtensions ??= new[] { ".jpg", ".jpeg", ".png", ".webp" };
        allowedContentTypes ??= new[] { "image/jpeg", "image/png", "image/webp" };

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
            return (false, $"Định dạng không hỗ trợ: {ext}");

        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return (false, $"Content-Type không hợp lệ: {file.ContentType}");

        return (true, "");
    }
}
