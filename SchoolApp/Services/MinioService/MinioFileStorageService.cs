using Minio;
using Minio.DataModel.Args;
using SchoolApp.Services.MinioService;

public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _client;
    private readonly string _bucket;

    private static readonly string[] AllowedAttachmentExtensions =
        [".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".zip", ".rar", ".png", ".jpg", ".jpeg", ".gif"];

    private static readonly string[] AllowedVideoExtensions =
        [".mp4", ".webm", ".ogg", ".mov"];

    private static readonly string[] AllowedImageExtensions =
        [".png", ".jpg", ".jpeg", ".gif", ".webp"];

    public MinioFileStorageService(IMinioClient client, IConfiguration config)
    {
        _client = client;
        _bucket = string.IsNullOrWhiteSpace(config["MinIO:BucketName"])
            ? "uploads"
            : config["MinIO:BucketName"]!;
    }

    public async Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
    {
        await EnsureBucketAsync(ct);

        var ext = Path.GetExtension(file.FileName);
        var objectName = $"{folder}/{Guid.NewGuid()}{ext}";

        await using var stream = file.OpenReadStream();
        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(file.Length)
            .WithContentType(file.ContentType), ct);

        return objectName;
    }

    public async Task<string?> UploadAttachmentAsync(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedAttachmentExtensions.Contains(ext))
            return null;

        var objectName = await UploadAsync(file, "attachments");
        return $"/File/Download?path={Uri.EscapeDataString(objectName)}";
    }

    public async Task<string?> UploadVideoAsync(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedVideoExtensions.Contains(ext))
            return null;

        var objectName = await UploadAsync(file, "videos");
        return $"/File/Video?path={Uri.EscapeDataString(objectName)}";
    }

    public async Task<string?> UploadThumbnailAsync(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(ext))
            return null;
        if (file.Length > 5 * 1024 * 1024)
            return null;

        var objectName = await UploadAsync(file, "thumbnails");
        return $"/File/Download?path={Uri.EscapeDataString(objectName)}";
    }

    public async Task<string?> UploadAvatarAsync(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(ext))
            return null;
        if (file.Length > 5 * 1024 * 1024)
            return null;

        var objectName = await UploadAsync(file, "avatars");
        return $"/File/Download?path={Uri.EscapeDataString(objectName)}";
    }

    public async Task<(Stream Stream, string ContentType)> DownloadAsync(
        string objectName, CancellationToken ct = default)
    {
        var stat = await _client.StatObjectAsync(new StatObjectArgs()
            .WithBucket(_bucket).WithObject(objectName), ct);

        var ms = new MemoryStream();
        await _client.GetObjectAsync(new GetObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithCallbackStream(s => s.CopyTo(ms)), ct);

        ms.Position = 0;
        return (ms, stat.ContentType);
    }

    public async Task<string> GetPresignedUrlAsync(string objectName, int expirySeconds = 3600)
    {
        return await _client.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithExpiry(expirySeconds));
    }

    public async Task DeleteAsync(string url, CancellationToken ct = default)
    {
        var objectName = ExtractObjectName(url);
        if (string.IsNullOrEmpty(objectName)) return;

        try
        {
            await _client.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_bucket).WithObject(objectName), ct);
        }
        catch { }
    }

    private async Task EnsureBucketAsync(CancellationToken ct)
    {
        var exists = await _client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_bucket), ct);
        if (!exists)
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket), ct);
    }

    // Trích xuất objectName từ URL dạng /File/Download?path=... hoặc /File/Video?path=...
    private static string? ExtractObjectName(string? url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        if (!url.StartsWith("/File/")) return null;

        var idx = url.IndexOf("?path=", StringComparison.Ordinal);
        if (idx < 0) return null;

        return Uri.UnescapeDataString(url[(idx + 6)..]);
    }
}