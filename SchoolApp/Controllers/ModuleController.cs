using Microsoft.AspNetCore.Mvc;
using SchoolApp.DTOs;
using SchoolApp.Filters;
using SchoolApp.Models;
using SchoolApp.UnitOfWork;
using X.PagedList;
using X.PagedList.Extensions;

namespace SchoolApp.Controllers
{
    public class ModuleController : Controller
    {
        private readonly IUnitOfWork _uow;

        public ModuleController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: /Module?courseId=5
        public IActionResult Index(int courseId, string searchTerm, int page = 1)
        {
            int pageSize = 10;
            var modules = _uow.Modules
                .SearchByTitle(searchTerm, courseId)
                .ToPagedList(page, pageSize);

            ViewData["CourseId"] = courseId;
            ViewData["SearchTerm"] = searchTerm;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ModuleTable", modules);
            }

            return View(modules);
        }

        [HttpGet]
        [AuthorizeAdmin]
        public IActionResult Create(int courseId)
        {
            return View(new Module
            {
                CourseId = courseId,
                IsPublished = true,
                OrderIndex = 0
            });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Module module)
        {
            if (!ModelState.IsValid)
                return View(module);

            // Tự động set OrderIndex nếu chưa có
            if (module.OrderIndex == 0)
            {
                var maxOrder = _uow.Modules
                    .Find(m => m.CourseId == module.CourseId)
                    .Max(m => (int?)m.OrderIndex) ?? 0;
                module.OrderIndex = maxOrder + 1;
            }

            _uow.Modules.Add(module);
            _uow.SaveChanges();

            TempData["Success"] = "Thêm chương thành công!";
            return RedirectToAction("Index", new { courseId = module.CourseId });
        }

        [AuthorizeAdmin]
        public IActionResult Edit(int id)
        {
            var module = _uow.Modules.GetById(id);
            if (module == null) return NotFound();
            return View(module);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Module module)
        {
            if (!ModelState.IsValid)
                return View(module);

            var existing = _uow.Modules.GetById(module.ModuleId);
            if (existing == null)
            {
                TempData["Error"] = "Không tìm thấy chương";
                return RedirectToAction("Index", new { courseId = module.CourseId });
            }

            existing.Title = module.Title;
            existing.Description = module.Description;
            existing.OrderIndex = module.OrderIndex;
            existing.IsPublished = module.IsPublished;

            _uow.SaveChanges();
            TempData["Success"] = "Cập nhật chương thành công!";
            return RedirectToAction("Index", new { courseId = existing.CourseId });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var module = _uow.Modules.GetById(id);
            if (module == null) return Json(new { success = false, message = "Không tìm thấy chương" });

            // Kiểm tra có Lesson không
            bool hasLessons = _uow.Lessons.Any(l => l.ModuleId == id); // sau khi thêm Lesson repo
            if (hasLessons)
            {
                return Json(new { success = false, message = "Không thể xóa: chương đang chứa bài học." });
            }

            _uow.Modules.Delete(module);
            _uow.SaveChanges();

            return Json(new { success = true, message = "Đã xóa chương!" });
        }

        // ==================== AJAX ====================

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAjax([FromForm] ModuleSaveDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }

            var module = new Module
            {
                CourseId = dto.CourseId,
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                OrderIndex = dto.OrderIndex == 0
                    ? (_uow.Modules.Find(m => m.CourseId == dto.CourseId).Max(m => (int?)m.OrderIndex) ?? 0) + 1
                    : dto.OrderIndex,
                IsPublished = dto.IsPublished
            };

            _uow.Modules.Add(module);
            _uow.SaveChanges();

            return Json(new { success = true, message = "Thêm chương thành công!", moduleId = module.ModuleId });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult EditAjax([FromForm] ModuleSaveDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

                return Json(new { success = false, errors });
            }

            var existing = _uow.Modules.GetById(dto.ModuleId);
            if (existing == null)
                return Json(new { success = false, message = "Không tìm thấy chương" });

            existing.Title = dto.Title.Trim();
            existing.Description = dto.Description?.Trim();
            existing.OrderIndex = dto.OrderIndex;
            existing.IsPublished = dto.IsPublished;

            // Không cho thay đổi CourseId khi edit (an toàn)
            // existing.CourseId = dto.CourseId;

            _uow.SaveChanges();

            return Json(new { success = true, message = "Cập nhật chương thành công!" });
        }

        [HttpGet]
        public IActionResult GetModule(int id)
        {
            var module = _uow.Modules.GetById(id);
            if (module == null) return NotFound();

            return Json(new
            {
                moduleId = module.ModuleId,
                title = module.Title,
                description = module.Description,
                orderIndex = module.OrderIndex,
                isPublished = module.IsPublished,
                courseId = module.CourseId
            });
        }
    }
}