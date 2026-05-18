using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
namespace SchoolApp.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private static readonly string[] AllowedAttachmentExtensions = [".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".zip", ".rar", ".mp4", ".png", ".jpg", ".jpeg", ".gif"];
        private static readonly string[] AllowedVideoExtensions = [".mp4", ".webm", ".ogg", ".mov"];

        public CloudinaryService(IConfiguration config)
        {
            var section = config.GetSection("Cloudinary");
            var account = new Account(
                section["CloudName"]!,
                section["ApiKey"]!,
                section["ApiSecret"]!);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadAttachmentAsync(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedAttachmentExtensions.Contains(ext)) return null;

            using var stream = file.OpenReadStream();
            var isImage = ext is ".png" or ".jpg" or ".jpeg" or ".gif";

            if (isImage)
            {
                var UploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "schoolapp/lessons",
                    UseFilename = false,
                    UniqueFilename = true
                };
                var result = await _cloudinary.UploadAsync(UploadParams);
                return result.Error == null ? result.SecureUrl.ToString() : null;
            }
            else
            {
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "schoolapp/lessons",
                    UseFilename = false,
                    UniqueFilename = true
                };
                var result = await _cloudinary.UploadAsync(uploadParams);
                return result.Error == null ? result.SecureUrl.ToString() : null;
            }


        }

        public async Task<string> UploadVideoAsync(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedVideoExtensions.Contains(ext)) return null;

            using var srtream = file.OpenReadStream();
            var UploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, srtream),
                Folder = "schoolapp/videos",
                UseFilename = false,
                UniqueFilename = true
            };
            var result = await _cloudinary.UploadAsync(UploadParams);
            return result.Error == null ? result.SecureUrl.ToString() : null;
        }

        public async Task DeleteAsync(string? cloudinaryUrl)
        {
            if (string.IsNullOrEmpty(cloudinaryUrl) || !IsCloudinaryUrl(cloudinaryUrl)) return;

            var publicId = ExtractPublicId(cloudinaryUrl);
            if (publicId == null) return;

            foreach (var resourceType in new[] { ResourceType.Video, ResourceType.Image, ResourceType.Raw })
            {
                var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId) { ResourceType = resourceType });
                if (result.Result == "ok") break;
            }
        }
        public bool IsCloudinaryUrl(string? url) =>
           !string.IsNullOrEmpty(url) && url.Contains("res.cloudinary.com");

        private string? ExtractPublicId(string url)
        {
            // URL dạng: https://res.cloudinary.com/{cloud}/{type}/upload/v{version}/{folder}/{filename}.{ext}
            try
            {
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Split('/');

                var uploadIdx = Array.IndexOf(segments, "upload");
                if (uploadIdx < 0) return null;

                var start = uploadIdx + 1;

                // Bỏ phần version nếu có (v123456789)
                if (start < segments.Length && System.Text.RegularExpressions.Regex.IsMatch(segments[start], @"^v\d+$"));
                start++;
                var pathParts = segments[start..];
                var publicIdWithExt = string.Join("/", pathParts);

                // Bỏ phần extension
                var dotIdx = publicIdWithExt.LastIndexOf('.');
                return dotIdx >= 0 ? publicIdWithExt[..dotIdx] : publicIdWithExt;
            }
            catch
            {
                return null;
            }
        }
    }
}