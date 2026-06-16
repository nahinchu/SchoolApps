using Microsoft.AspNetCore.Mvc;
using SchoolApp.Services.MinioService;

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
        [HttpGet]
        public async Task<IActionResult> Download(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest();

            var (stream, contentType) = await _storage.DownloadAsync(path);
            return File(stream, contentType);
        }

        // GET /File/Video?path=videos/guid.mp4
        [HttpGet]
        public async Task<IActionResult> Video(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest();

            var (stream, contentType) = await _storage.DownloadAsync(path);
            return File(stream, contentType, enableRangeProcessing: true);
        }
    }
}
