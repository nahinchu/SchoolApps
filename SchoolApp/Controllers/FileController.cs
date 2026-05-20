using Microsoft.AspNetCore.Mvc;
using SchoolApp.Services;

namespace SchoolApp.Controllers
{
    public class FileController : Controller
    {
        private readonly IFileStorageService _storage;

        public FileController(IFileStorageService storage)
        {
            _storage = storage;
        }

        // GET /File/Download?path=attachments/guid.pdf
        // Tạo presigned URL tạm thời và redirect về MinIO
        [HttpGet]
        public async Task<IActionResult> Download(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest();

            var url = await _storage.GetPresignedUrlAsync(path, expirySeconds: 3600);
            return Redirect(url);
        }

        // GET /File/Video?path=videos/guid.mp4
        // Redirect về presigned URL để trình duyệt có thể stream video
        [HttpGet]
        public async Task<IActionResult> Video(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest();

            var url = await _storage.GetPresignedUrlAsync(path, expirySeconds: 7200);
            return Redirect(url);
        }
    }
}