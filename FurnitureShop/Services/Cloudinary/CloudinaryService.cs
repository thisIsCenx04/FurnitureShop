using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace FurnitureShop.Services.Upload;

public sealed class CloudinarySettings
{
    public string CloudName { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string ApiSecret { get; set; } = "";
}

public sealed class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> opt)
    {
        var s = opt.Value;
        var account = new Account(s.CloudName, s.ApiKey, s.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file, string folder, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("File rỗng.");

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder,
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var res = await _cloudinary.UploadAsync(uploadParams, ct);
        if (res.Error != null) throw new InvalidOperationException(res.Error.Message);

        return new CloudinaryUploadResult
        {
            Url = res.SecureUrl?.ToString() ?? res.Url?.ToString() ?? "",
            PublicId = res.PublicId
        };
    }

    public async Task DeleteAsync(string publicId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(publicId)) return;

        var del = new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Image
        };

        var res = await _cloudinary.DestroyAsync(del);

        if (res.Error != null &&
            !res.Error.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(res.Error.Message);
        }
    }

}
