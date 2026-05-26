using Microsoft.AspNetCore.Mvc;
using SchoolApp.Filters;
using SchoolApp.Models;
using SchoolApp.Services.ExcelExportService;
using SchoolApp.UnitOfWork;
using X.PagedList;
using X.PagedList.Extensions;

namespace SchoolApp.Controllers
{
    public class CourseController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IExcelExportService _excel;

        public CourseController(IUnitOfWork uow, IExcelExportService excel)
        {
            _uow = uow;
            _excel = excel;
        }

        // GET: /Course
        public IActionResult Index(string searchTerm, int page = 1)
        {
            int pageSize = 5;

            var result = _uow.Courses.SearchByName(searchTerm)
                .ToPagedList(page, pageSize);

            ViewData["SearchTerm"] = searchTerm;

            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId.HasValue)
            {
                var enrolledIds = _uow.Enrollments.GetByStudent(studentId.Value)
                    .Select(e => e.CourseId)
                    .ToHashSet();
                ViewBag.EnrolledCourseIds = enrolledIds;
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CourseTable", result);
            }
            return View(result);
        }

        [HttpGet]
        [AuthorizeManager]
        public IActionResult Create()
        {
            return View(new Course { IsActive = true });
        }

        [HttpPost]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Course course)
        {
            if (!ModelState.IsValid)
                return View(course);

            course.CreatedDate = DateTime.Now;
            _uow.Courses.Add(course);
            _uow.SaveChanges();

            TempData["Success"] = "Thêm khóa học thành công!";
            return RedirectToAction("Index");
        }

        [AuthorizeManager]
        public IActionResult Edit(int id)
        {
            var course = _uow.Courses.GetById(id);
            if (course == null) return NotFound();
            return View(course);
        }

        [HttpPost]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Course course)
        {
            if (!ModelState.IsValid)
                return View(course);

            var existing = _uow.Courses.GetById(course.CourseId);
            if (existing == null)
            {
                TempData["Error"] = "Không tìm thấy khóa học";
                return RedirectToAction("Index");
            }

            existing.CourseName = course.CourseName;
            existing.Description = course.Description;
            existing.Credits = course.Credits;
            existing.Fee = course.Fee;
            existing.IsActive = course.IsActive;

            _uow.SaveChanges();

            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var course = _uow.Courses.GetById(id);
            if (course == null)
            {
                TempData["Error"] = "Không tìm thấy khóa học";
                return RedirectToAction("Index");
            }

            bool hasEnrollments = _uow.Enrollments.Any(e => e.CourseId == id);
            if (hasEnrollments)
            {
                TempData["Error"] = "Không thể xóa: đã có học viên đăng ký.";
                return RedirectToAction("Index");
            }

            _uow.Courses.Delete(course);
            _uow.SaveChanges();

            TempData["Success"] = "Đã xóa khóa học!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAjax(Course course)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                   .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }
            course.CreatedDate = DateTime.Now;
            _uow.Courses.Add(course);
            _uow.SaveChanges();
            return Json(new { success = true, message = "Thêm khóa học thành công!" });
        }

        [HttpPost]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult EditAjax(Course course)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                   .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }
            var existing = _uow.Courses.GetById(course.CourseId);
            if (existing == null)
            {
                return Json(new { success = false, message = "Không tìm thấy khóa học" });
            }
            existing.CourseName = course.CourseName;
            existing.Description = course.Description;
            existing.Credits = course.Credits;
            existing.Fee = course.Fee;
            existing.IsActive = course.IsActive;
            _uow.SaveChanges();
            return Json(new { success = true, message = "Cập nhật thành công!" });
        }


        [HttpGet]
        [AuthorizeUser]
        public IActionResult GetCourse(int id)
        {
            var course = _uow.Courses.GetById(id);
            if (course == null) return NotFound();
            return Json(new
            {
                courseId = course.CourseId,
                courseName = course.CourseName,
                description = course.Description,
                credits = course.Credits,
                fee = course.Fee,
                isActive = course.IsActive
            });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAjax(int id)
        {
            var course = _uow.Courses.GetById(id);
            if (course == null)
            {
                return Json(new { success = false, message = "Không tìm thấy khóa học" });
            }
            bool hasEnrollments = _uow.Enrollments.Any(e => e.CourseId == id);
            if (hasEnrollments)
            {
                return Json(new { success = false, message = "Không thể xóa: đã có học viên đăng ký." });
            }
            _uow.Courses.Delete(course);
            _uow.SaveChanges();
            return Json(new { success = true, message = "Đã xóa khóa học!" });
        }

        [AuthorizeManager]
        public IActionResult ExportExcel(string? searchTerm)
        {
            var courses = _uow.Courses.SearchByName(searchTerm).ToList();

            var columns = new List<ExcelColumnDef<Course>>
            {
                new() { Header = "ID",           ValueSelector = c => c.CourseId,    Width = 8  },
                new() { Header = "Tên khóa học", ValueSelector = c => c.CourseName,  Width = 35 },
                new() { Header = "Mô tả",        ValueSelector = c => c.Description, Width = 45 },
                new() { Header = "Tín chỉ",      ValueSelector = c => c.Credits,     Width = 12 },
                new() { Header = "Học phí (VNĐ)",ValueSelector = c => c.Fee,         Width = 18, Format = "#,##0" },
                new() { Header = "Trạng thái",   ValueSelector = c => c.IsActive ? "Đang mở" : "Đóng", Width = 14 },
                new() { Header = "Ngày tạo",     ValueSelector = c => c.CreatedDate, Width = 16, Format = "dd/MM/yyyy" },
            };

            var bytes = _excel.Export(courses, columns, "Khóa học", "DANH SÁCH KHÓA HỌC");
            var fileName = $"DanhSachKhoaHoc_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }



        [AuthorizeManager]
        public IActionResult Modules(int id)
        {
            var course = _uow.Courses.GetById(id);
            if (course == null) return NotFound();

            ViewData["CourseName"] = course.CourseName;
            return RedirectToAction("Index", "Module", new { courseId = id });
        }
    }
}