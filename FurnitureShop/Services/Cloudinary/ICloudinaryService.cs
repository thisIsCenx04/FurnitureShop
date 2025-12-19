using Microsoft.AspNetCore.Http;

namespace FurnitureShop.Services.Upload;

public interface ICloudinaryService
{
    Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file, string folder, CancellationToken ct = default);
    Task DeleteAsync(string publicId, CancellationToken ct = default);
}
