using Minio;
using Minio.DataModel.Args;
using SchoolApp.Services;

public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _client;
    private readonly string _bucket;

    public MinioFileStorageService(IMinioClient client, IConfiguration config)
    {
        _client = client;
        _bucket = config["MinIO:BucketName"] ?? "uploads";
    }

    public async Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
    {
        // Tự tạo bucket nếu chưa có
        var exists = await _client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_bucket), ct);
        if (!exists)
        {
            await _client.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(_bucket), ct);
        }

        // Đặt tên object: folder/guid.ext để tránh trùng tên
        var ext = Path.GetExtension(file.FileName);
        var objectName = $"{folder}/{Guid.NewGuid()}{ext}";

        await using var stream = file.OpenReadStream();
        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(file.Length)
            .WithContentType(file.ContentType), ct);

        return objectName; // Lưu cái này vào DB
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

    public async Task DeleteAsync(string objectName, CancellationToken ct = default)
    {
        await _client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_bucket).WithObject(objectName), ct);
    }
}