namespace SchoolApp.Services
{
    public interface ICloudinaryService
    {
        Task<string?> UploadAttachmentAsync(IFormFile file);
        Task<string?> UploadVideoAsync(IFormFile file);
        Task DeleteAsync(string? cloudinaryUrl);
        bool IsCloudinaryUrl(string? url);
    }
}
