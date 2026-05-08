using Microsoft.AspNetCore.Mvc;
using SchoolApp.DTOs;
using SchoolApp.Filters;
using SchoolApp.Models;
using SchoolApp.Models.Enums;
using SchoolApp.UnitOfWork;
using X.PagedList;
using X.PagedList.Extensions;

namespace SchoolApp.Controllers
{
    public class LessonController : Controller
    {
        private readonly IUnitOfWork _uow;

        public LessonController(IUnitOfWork uow)
        {
            _uow = uow;
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
        [AuthorizeAdmin]
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
        [AuthorizeAdmin]
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

        [AuthorizeAdmin]
        public IActionResult Edit(int id)
        {
            var lesson = _uow.Lessons.GetById(id);
            if (lesson == null) return NotFound();
            return View(lesson);
        }

        [HttpPost]
        [AuthorizeAdmin]
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
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var lesson = _uow.Lessons.GetById(id);
            if (lesson == null)
                return Json(new { success = false, message = "Không tìm thấy bài học" });

            _uow.Lessons.Delete(lesson);
            _uow.SaveChanges();

            return Json(new { success = true, message = "Đã xóa bài học thành công!" });
        }

        // ==================== AJAX ====================

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAjax([FromForm] LessonSaveDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }

            var lesson = new Lesson
            {
                ModuleId = dto.ModuleId,
                Title = dto.Title.Trim(),
                Type = dto.Type,
                VideoUrl = dto.VideoUrl?.Trim(),
                HtmlContent = dto.HtmlContent?.Trim(),
                AttachmentPath = dto.AttachmentPath?.Trim(),
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
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult EditAjax([FromForm] LessonSaveDto dto)
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

            // Cập nhật thông tin
            existing.Title = dto.Title.Trim();
            existing.Type = dto.Type;
            existing.VideoUrl = dto.VideoUrl?.Trim();
            existing.HtmlContent = dto.HtmlContent?.Trim();
            existing.AttachmentPath = dto.AttachmentPath?.Trim();
            existing.DurationMinutes = dto.DurationMinutes;
            existing.OrderIndex = dto.OrderIndex;
            existing.IsPublished = dto.IsPublished;

            // KHÔNG thay đổi ModuleId khi edit (an toàn hơn)
            // existing.ModuleId = dto.ModuleId;

            _uow.SaveChanges();

            return Json(new { success = true, message = "Cập nhật bài học thành công!" });
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
    }
}