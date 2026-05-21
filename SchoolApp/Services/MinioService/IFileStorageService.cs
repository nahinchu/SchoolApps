namespace SchoolApp.Services.MinioService
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default);
        Task<(Stream Stream, string ContentType)> DownloadAsync(string objectName, CancellationToken ct = default);
        Task<string> GetPresignedUrlAsync(string objectName, int expirySeconds = 3600);
        Task DeleteAsync(string objectName, CancellationToken ct = default);
        Task<string?> UploadAttachmentAsync(IFormFile file);
        Task<string?> UploadVideoAsync(IFormFile file);
    }
}