using Microsoft.AspNetCore.Mvc;
using SchoolApp.DTOs;
using SchoolApp.Filters;
using SchoolApp.Models;
using SchoolApp.Models.Enums;
using SchoolApp.Services.MinioService;
using SchoolApp.UnitOfWork;
using X.PagedList;
using X.PagedList.Extensions;

namespace SchoolApp.Controllers
{
    public class LessonController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;
        private readonly IFileStorageService _storage;

        private const long MaxFileSize = 50 * 1024 * 1024; // 50 MB
        private const long MaxVideoSize = 500 * 1024 * 1024; // 500 MB

        public LessonController(IUnitOfWork uow, IWebHostEnvironment env, IFileStorageService storage)
        {
            _uow = uow;
            _env = env;
            _storage = storage;
        }
        public IActionResult Index(int moduleId, string searchTerm, int page = 1)
        {
            int pageSize = 10;
            var lessons = _uow.Lessons
                .SearchByTitle(searchTerm, moduleId)
                .ToPagedList(page, pageSize);

            var module = _uow.Modules.GetById(moduleId);
            if (module == null) return NotFound();

            ViewData["ModuleId"] = moduleId;
            ViewData["ModuleName"] = module.Title;
            ViewData["CourseId"] = module.CourseId;
            ViewData["SearchTerm"] = searchTerm;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_LessonTable", lessons);
            }

            return View(lessons);
        }
        [HttpGet]
        [AuthorizeManager]
        public IActionResult Create(int moduleId)
        {
            return View(new Lesson
            {
                ModuleId = moduleId,
                Type = LessonType.Video,
                IsPublished = true,
                OrderIndex = 0,
                DurationMinutes = 0
            });
        }

        [HttpPost]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Lesson lesson)
        {
            if (!ModelState.IsValid) return View(lesson);

            if (lesson.OrderIndex == 0)
                lesson.OrderIndex = _uow.Lessons.GetMaxOrderIndex(lesson.ModuleId) + 1;

            lesson.CreatedAt = DateTime.UtcNow;

            _uow.Lessons.Add(lesson);
            _uow.SaveChanges();

            TempData["Success"] = "Thêm bài học thành công!";
            return RedirectToAction("Index", new { moduleId = lesson.ModuleId });
        }

        [AuthorizeManager]
        public IActionResult Edit(int id)
        {
            var lesson = _uow.Lessons.GetById(id);
            if (lesson == null) return NotFound();
            return View(lesson);
        }

        [HttpPost]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Lesson lesson)
        {
            if (!ModelState.IsValid) return View(lesson);

            var existing = _uow.Lessons.GetById(lesson.LessonId);
            if (existing == null) return NotFound();

            existing.Title = lesson.Title;
            existing.Type = lesson.Type;
            existing.VideoUrl = lesson.VideoUrl;
            existing.HtmlContent = lesson.HtmlContent;
            existing.AttachmentPath = lesson.AttachmentPath;
            existing.DurationMinutes = lesson.DurationMinutes;
            existing.OrderIndex = lesson.OrderIndex;
            existing.IsPublished = lesson.IsPublished;

            _uow.SaveChanges();
            TempData["Success"] = "Cập nhật bài học thành công!";
            return RedirectToAction("Index", new { moduleId = existing.ModuleId });
        }

        [HttpPost]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var lesson = _uow.Lessons.GetById(id);
            if (lesson == null)
                return Json(new { success = false, message = "Không tìm thấy bài học" });

            await _storage.DeleteAsync(lesson.VideoUrl ?? "");
            await _storage.DeleteAsync(lesson.AttachmentPath ?? "");

            _uow.Lessons.Delete(lesson);
            _uow.SaveChanges();

            return Json(new { success = true, message = "Đã xóa bài học thành công!" });
        }

        // ==================== AJAX ====================

        [HttpPost]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAjax([FromForm] LessonSaveDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }

            if (dto.AttachmentFile != null && dto.AttachmentFile.Length > MaxFileSize)
                return Json(new { success = false, message = "File đính kèm không được vượt quá 50MB." });

            if (dto.VideoFile != null && dto.VideoFile.Length > MaxVideoSize)
                return Json(new { success = false, message = "Video không được vượt quá 500MB." });

            string? attachmentPath = null;
            if (dto.AttachmentFile != null && dto.AttachmentFile.Length > 0)
            {
                attachmentPath = await _storage.UploadAttachmentAsync(dto.AttachmentFile);
                if (attachmentPath == null)
                    return Json(new { success = false, message = "Định dạng file đính kèm không hỗ trợ." });
            }

            string? videoUrl = dto.VideoUrl?.Trim();
            if (dto.VideoInputMode == "file" && dto.VideoFile != null && dto.VideoFile.Length > 0)
            {
                videoUrl = await _storage.UploadVideoAsync(dto.VideoFile);
                if (videoUrl == null)
                    return Json(new { success = false, message = "Định dạng video không hỗ trợ. Dùng MP4, WebM, OGG hoặc MOV." });
            }

            var lesson = new Lesson
            {
                ModuleId = dto.ModuleId,
                Title = dto.Title.Trim(),
                Type = dto.Type,
                VideoUrl = videoUrl,
                HtmlContent = dto.HtmlContent?.Trim(),
                AttachmentPath = attachmentPath,
                OrderIndex = dto.OrderIndex == 0
                            ? _uow.Lessons.GetMaxOrderIndex(dto.ModuleId) + 1
                            : dto.OrderIndex,
                DurationMinutes = dto.DurationMinutes,
                IsPublished = dto.IsPublished,
                CreatedAt = DateTime.UtcNow
            };  

            _uow.Lessons.Add(lesson);
            _uow.SaveChanges();

            return Json(new { success = true, message = "Thêm bài học thành công!", lessonId = lesson.LessonId });
        }

        [HttpPost]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAjax([FromForm] LessonSaveDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

                return Json(new { success = false, errors });
            }

            var existing = _uow.Lessons.GetById(dto.LessonId);
            if (existing == null)
                return Json(new { success = false, message = "Không tìm thấy bài học" });

            if (dto.AttachmentFile != null && dto.AttachmentFile.Length > MaxFileSize)
                return Json(new { success = false, message = "File đính kèm không được vượt quá 50MB." });

            if (dto.VideoFile != null && dto.VideoFile.Length > MaxVideoSize)
                return Json(new { success = false, message = "Video không được vượt quá 500MB." });

            // Xử lý file đính kèm
            if (dto.AttachmentFile != null && dto.AttachmentFile.Length > 0)
            {
                var newPath = await _storage.UploadAttachmentAsync(dto.AttachmentFile);
                if (newPath == null)
                    return Json(new { success = false, message = "Định dạng file đính kèm không hỗ trợ." });
                await _storage.DeleteAsync(existing.AttachmentPath ?? "");
                existing.AttachmentPath = newPath;
            }
            else if (dto.RemoveAttachment)
            {
                await _storage.DeleteAsync(existing.AttachmentPath ?? "");
                existing.AttachmentPath = null;
            }

            // Xử lý video
            if (dto.VideoInputMode == "file")
            {
                if (dto.VideoFile != null && dto.VideoFile.Length > 0)
                {
                    var newVideoPath = await _storage.UploadVideoAsync(dto.VideoFile);
                    if (newVideoPath == null)
                        return Json(new { success = false, message = "Định dạng video không hỗ trợ. Dùng MP4, WebM, OGG hoặc MOV." });
                    await _storage.DeleteAsync(existing.VideoUrl ?? "");
                    existing.VideoUrl = newVideoPath;
                }
                else if (dto.RemoveVideo)
                {
                    await _storage.DeleteAsync(existing.VideoUrl ?? "");
                    existing.VideoUrl = null;
                }
                // Nếu không upload file mới và không xóa → giữ nguyên VideoUrl cũ
            }
            else // mode = "url"
            {
                var newUrl = dto.VideoUrl?.Trim();
                // Nếu đang dùng file upload cũ mà giờ chuyển sang URL → xóa file cũ
                await _storage.DeleteAsync(existing.VideoUrl ?? "");
                existing.VideoUrl = newUrl;
            }

            existing.Title = dto.Title.Trim();
            existing.Type = dto.Type;
            existing.HtmlContent = dto.HtmlContent?.Trim();
            existing.DurationMinutes = dto.DurationMinutes;
            existing.OrderIndex = dto.OrderIndex;
            existing.IsPublished = dto.IsPublished;

            _uow.SaveChanges();

            return Json(new { success = true, message = "Cập nhật bài học thành công!" });
        }

        [HttpPost]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult ReorderAjax([FromBody] ReorderDto dto)
        {
            if (dto?.Ids == null || dto.Ids.Length == 0)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            for (int i = 0; i < dto.Ids.Length; i++)
            {
                var l = _uow.Lessons.GetById(dto.Ids[i]);
                if (l != null) { l.OrderIndex = i + 1; _uow.Lessons.Update(l); }
            }
            _uow.SaveChanges();
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult GetLesson(int id)
        {
            var lesson = _uow.Lessons.GetById(id);
            if (lesson == null) return NotFound();

            return Json(new
            {
                lessonId = lesson.LessonId,
                title = lesson.Title,
                type = lesson.Type.ToString(),
                videoUrl = lesson.VideoUrl,
                htmlContent = lesson.HtmlContent,
                attachmentPath = lesson.AttachmentPath,
                durationMinutes = lesson.DurationMinutes,
                orderIndex = lesson.OrderIndex,
                isPublished = lesson.IsPublished,
                moduleId = lesson.ModuleId
            });
        }

        //Lưu local

        //private async Task<string?> SaveAttachmentAsync(IFormFile file)
        //{
        //    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        //    if (!AllowedExtensions.Contains(ext))
        //        return null;

        //    var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "lessons");
        //    Directory.CreateDirectory(uploadsDir);

        //    var fileName = $"{Guid.NewGuid()}{ext}";
        //    var filePath = Path.Combine(uploadsDir, fileName);

        //    using var stream = new FileStream(filePath, FileMode.Create);
        //    await file.CopyToAsync(stream);

        //    return $"/uploads/lessons/{fileName}";
        //}

        //private void DeleteAttachmentFile(string? relativePath)
        //{
        //    if (string.IsNullOrEmpty(relativePath)) return;
        //    var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
        //    if (System.IO.File.Exists(fullPath))
        //        System.IO.File.Delete(fullPath);
        //}

        //private async Task<string?> SaveVideoAsync(IFormFile file)
        //{
        //    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        //    if (!AllowedVideoExtensions.Contains(ext))
        //        return null;

        //    var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "videos");
        //    Directory.CreateDirectory(uploadsDir);

        //    var fileName = $"{Guid.NewGuid()}{ext}";
        //    var filePath = Path.Combine(uploadsDir, fileName);

        //    using var stream = new FileStream(filePath, FileMode.Create);
        //    await file.CopyToAsync(stream);

        //    return $"/uploads/videos/{fileName}";
        //}

        //private static bool IsUploadedVideo(string? url) =>
        //    !string.IsNullOrEmpty(url) && url.StartsWith("/uploads/videos/");

       

    }
}