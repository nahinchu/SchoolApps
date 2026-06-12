using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolApp.Filters;
using SchoolApp.Models;
using SchoolApp.Services.ExcelExportService;
using SchoolApp.Services.MinioService;
using SchoolApp.UnitOfWork;
using X.PagedList;
using X.PagedList.Extensions;
using System.Globalization;


namespace SchoolApp.Controllers
{
    public class CourseController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IExcelExportService _excel;
        private readonly IFileStorageService _storage;
        private readonly IExcelImportService _excelImport;

        public CourseController(IUnitOfWork uow, IExcelExportService excel, IFileStorageService storage, IExcelImportService excelImport)
        {
            _uow = uow;
            _excel = excel;
            _storage = storage;
            _excelImport = excelImport;
        }

        // GET: /Course
        public IActionResult Index(string searchTerm, int page = 1, int? level = null, string sort = null, string status = null)
        {
            int pageSize = 5;

            var query = _uow.Courses.SearchByName(searchTerm);

            if (level.HasValue)
                query = query.Where(c => (int)c.Level == level.Value);

            if (status == "active")
                query = query.Where(c => c.IsActive);
            else if (status == "inactive")
                query = query.Where(c => !c.IsActive);

            query = sort switch
            {
                "price_asc" => query.OrderBy(c => c.Fee),
                "price_desc" => query.OrderByDescending(c => c.Fee),
                "name_az" => query.OrderBy(c => c.CourseName),
                _ => query
            };

            var result = query.ToPagedList(page, pageSize);

            ViewData["SearchTerm"] = searchTerm;
            ViewData["Level"] = level?.ToString() ?? "";
            ViewData["Sort"] = sort ?? "";
            ViewData["Status"] = status ?? "";

            var studentId = HttpContext.Session.GetInt32("UserId");
            if (studentId.HasValue)
            {
                var enrolledIds = _uow.Enrollments.GetByStudent(studentId.Value)
                    .Select(e => e.CourseId)
                    .ToHashSet();
                ViewBag.EnrolledCourseIds = enrolledIds;
            }

            // Enrollment counts + average ratings cho admin/manager table
            var role = HttpContext.Session.GetString("Role");
            if (role == "Admin" || role == "Manager")
            {
                var courseIds = result.Select(c => c.CourseId).ToList();
                ViewBag.EnrollmentCounts = _uow.Enrollments.GetAll()
                    .Where(e => courseIds.Contains(e.CourseId))
                    .GroupBy(e => e.CourseId)
                    .ToDictionary(g => g.Key, g => g.Count());
                ViewBag.AverageRatings = courseIds.ToDictionary(
                    id => id,
                    id => _uow.Reviews.GetAverageRating(id));
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_CourseTable", result);

            return View(result);
        }

        // GET: /Course/Detail/{id}
        public IActionResult Detail(int id)
        {
            var course = _uow.Courses.GetCourseWithFullTree(id);
            if (course == null) return NotFound();

            var studentId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");

            ViewBag.IsEnrolled = false;
            ViewBag.StudentId = studentId;
            ViewBag.Role = role;

            if (studentId.HasValue)
            {
                ViewBag.IsEnrolled = _uow.Enrollments
                    .GetByStudent(studentId.Value)
                    .Any(e => e.CourseId == id);
                ViewBag.UserReview = _uow.Reviews.GetByStudentAndCourse(studentId.Value, id);
            }

            ViewBag.EnrollmentCount = _uow.Enrollments.GetAll()
                .Count(e => e.CourseId == id);

            ViewBag.Reviews = _uow.Reviews.GetByCourse(id).ToList();
            ViewBag.AverageRating = _uow.Reviews.GetAverageRating(id);

            return View(course);
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
            existing.Level = course.Level;
            existing.IsActive = course.IsActive;
            existing.UpdatedDate = DateTime.Now;

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
        public async Task<IActionResult> CreateAjax(Course course, IFormFile? thumbnail)
        {
            ModelState.Remove(nameof(Course.ThumbnailUrl));
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                   .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }

            bool nameExists = _uow.Courses.SearchByName(course.CourseName)
                .Any(c => c.CourseName == course.CourseName);
            if (nameExists)
                return Json(new { success = false, message = "Tên khóa học đã tồn tại." });

            course.CreatedDate = DateTime.Now;

            if (thumbnail != null && thumbnail.Length > 0)
            {
                var url = await _storage.UploadThumbnailAsync(thumbnail);
                if (url == null)
                    return Json(new { success = false, message = "Ảnh không hợp lệ (chỉ chấp nhận PNG/JPG/GIF/WEBP, tối đa 5MB)." });
                course.ThumbnailUrl = url;
            }

            _uow.Courses.Add(course);
            _uow.SaveChanges();
            return Json(new { success = true, message = "Thêm khóa học thành công!" });
        }

        [HttpPost]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAjax(Course course, IFormFile? thumbnail)
        {
            ModelState.Remove(nameof(Course.ThumbnailUrl));
            ModelState.Remove(nameof(Course.UpdatedDate));
            ModelState.Remove(nameof(Course.CreatedDate));
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                   .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }

            try
            {
                var existing = _uow.Courses.GetById(course.CourseId);
                if (existing == null)
                    return Json(new { success = false, message = "Không tìm thấy khóa học" });

                bool nameConflict = _uow.Courses.SearchByName(course.CourseName)
                    .Any(c => c.CourseName == course.CourseName && c.CourseId != course.CourseId);
                if (nameConflict)
                    return Json(new { success = false, message = "Tên khóa học đã tồn tại." });

                existing.CourseName = course.CourseName;
                existing.Description = course.Description;
                existing.Credits = course.Credits;
                existing.Fee = course.Fee;
                existing.Level = course.Level;
                existing.IsActive = course.IsActive;
                existing.UpdatedDate = DateTime.Now;

                if (thumbnail != null && thumbnail.Length > 0)
                {
                    var url = await _storage.UploadThumbnailAsync(thumbnail);
                    if (url == null)
                        return Json(new { success = false, message = "Ảnh không hợp lệ (chỉ chấp nhận PNG/JPG/GIF/WEBP, tối đa 5MB)." });

                    if (!string.IsNullOrEmpty(existing.ThumbnailUrl))
                        await _storage.DeleteAsync(existing.ThumbnailUrl);

                    existing.ThumbnailUrl = url;
                }

                _uow.SaveChanges();
                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = $"Lỗi: {inner}" });
            }
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
                level = (int)course.Level,
                isActive = course.IsActive,
                thumbnailUrl = course.ThumbnailUrl
            });
        }

        [HttpPost]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleActive(int id)
        {
            var course = _uow.Courses.GetById(id);
            if (course == null)
                return Json(new { success = false, message = "Không tìm thấy khóa học" });

            course.IsActive = !course.IsActive;
            course.UpdatedDate = DateTime.Now;
            _uow.SaveChanges();

            var label = course.IsActive ? "Đang mở" : "Đã đóng";
            return Json(new { success = true, isActive = course.IsActive, label });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            var course = _uow.Courses.GetById(id);
            if (course == null)
                return Json(new { success = false, message = "Không tìm thấy khóa học" });

            bool hasEnrollments = _uow.Enrollments.Any(e => e.CourseId == id);
            if (hasEnrollments)
                return Json(new { success = false, message = "Không thể xóa: đã có học viên đăng ký." });

            if (!string.IsNullOrEmpty(course.ThumbnailUrl))
                await _storage.DeleteAsync(course.ThumbnailUrl);

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
                new() { Header = "Cấp độ",       ValueSelector = c => c.Level switch {
                    CourseLevel.Basic        => "Cơ bản",
                    CourseLevel.Intermediate => "Trung cấp",
                    CourseLevel.Advanced     => "Nâng cao",
                    _                        => c.Level.ToString()
                }, Width = 14 },
                new() { Header = "Mô tả",        ValueSelector = c => c.Description, Width = 45 },
                new() { Header = "Tín chỉ",      ValueSelector = c => c.Credits,     Width = 12 },
                new() { Header = "Học phí (VNĐ)",ValueSelector = c => c.Fee,         Width = 18, Format = "#,##0" },
                new() { Header = "Trạng thái",   ValueSelector = c => c.IsActive ? "Đang mở" : "Đóng", Width = 14 },
                new() { Header = "Ngày tạo",     ValueSelector = c => c.CreatedDate, Width = 16, Format = "dd/MM/yyyy" },
                new() { Header = "Cập nhật",     ValueSelector = c => c.UpdatedDate, Width = 16, Format = "dd/MM/yyyy" },
            };

            var bytes = _excel.Export(courses, columns, "Khóa học", "DANH SÁCH KHÓA HỌC");
            var fileName = $"DanhSachKhoaHoc_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn file Excel." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".xlsx")
                return Json(new { success = false, message = "Chỉ chấp nhận file .xlsx." });

            static decimal ParseFee(string v)
            {
                var cleaned = v.Trim().Replace(" ", "").Replace(",", "").Replace(".", "");
                if (decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var fee))
                    return fee;

                if (decimal.TryParse(v, NumberStyles.Number, CultureInfo.GetCultureInfo("vi-VN"), out fee))
                    return fee;

                throw new Exception("Không đúng định dạng học phí");
            }

            static bool ParseIsActive(string v)
            {
                var s = v.Trim().ToLowerInvariant();
                return s switch
                {
                    "1" or "true" or "yes" or "y" or "mở" or "dang mo" or "đang mở" or "đang mo" => true,
                    "0" or "false" or "no" or "n" or "đóng" or "dong" => false,
                    _ => throw new Exception("Giá trị hợp lệ: Đang mở/Đóng (hoặc 1/0, true/false)")
                };
            }

            static CourseLevel ParseLevel(string v)
            {
                var s = v.Trim().ToLowerInvariant();
                return s switch
                {
                    "0" or "basic" or "cơ bản" or "co ban" or "căn bản" or "can ban" => CourseLevel.Basic,
                    "1" or "intermediate" or "trung cấp" or "trung cap" or "trung bình" or "trung binh" => CourseLevel.Intermediate,
                    "2" or "advanced" or "nâng cao" or "nang cao" or "khó" or "kho" => CourseLevel.Advanced,
                    _ => throw new Exception("Giá trị hợp lệ: Cơ bản / Trung cấp / Nâng cao (hoặc 0/1/2)")
                };
            }

            var columns = new List<ExcelColumnMap<Course>>
            {
                new()
                {
                    Header = "Tên khóa học", Required = true,
                    Setter = (c, v) => c.CourseName = v
                },
                new()
                {
                    Header = "Mô tả", Required = false,
                    Setter = (c, v) => c.Description = v
                },
                new()
                {
                    Header = "Cấp độ", Required = false,
                    Setter = (c, v) => c.Level = string.IsNullOrWhiteSpace(v)
                        ? CourseLevel.Basic
                        : ParseLevel(v)
                },
                new()
                {
                    Header = "Tín chỉ", Required = true,
                    Setter = (c, v) =>
                    {
                        if (!int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var credits))
                            throw new Exception("Phải là số nguyên");
                        if (credits < 1 || credits > 10)
                            throw new Exception("Phải từ 1 đến 10");
                        c.Credits = credits;
                    }
                },
                new()
                {
                    Header = "Học phí (VNĐ)", Required = true,
                    Setter = (c, v) =>
                    {
                        var fee = ParseFee(v);
                        if (fee < 0 || fee > 100000000)
                            throw new Exception("Phải từ 0 đến 100,000,000");
                        c.Fee = fee;
                    }
                },
                new()
                {
                    Header = "Trạng thái", Required = false,
                    Setter = (c, v) => c.IsActive = ParseIsActive(v)
                },
            };

            using var stream = file.OpenReadStream();
            var importResult = _excelImport.Import(stream, columns, () => new Course
            {
                IsActive = true,
                Level = CourseLevel.Basic,
                CreatedDate = DateTime.Now
            });

            var fileErrors = importResult.Errors.Where(e => e.RowNumber == 0).ToList();
            if (fileErrors.Any())
                return Json(new { success = false, message = fileErrors.First().Message });

            int added = 0;
            var skipped = new List<object>();

            foreach (var err in importResult.Errors.Where(e => e.RowNumber > 0))
                skipped.Add(new { row = err.RowNumber, reason = err.Message });

            foreach (var course in importResult.ValidRows)
            {
                var name = (course.CourseName ?? "").Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    skipped.Add(new { row = -1, reason = "Tên khóa học không được để trống" });
                    continue;
                }

                if (_uow.Courses.Any(c => c.CourseName.ToLower() == name.ToLower()))
                {
                    skipped.Add(new { row = -1, reason = $"Khóa học \"{name}\" đã tồn tại" });
                    continue;
                }

                course.CourseName = name;
                _uow.Courses.Add(course);
                added++;
            }

            if (added > 0) _uow.SaveChanges();

            return Json(new { success = true, added, skipped });
        }

        [AuthorizeManager]
        public IActionResult DownloadImportTemplate()
        {
            var sample = new[]
            {
                new {
                    TenKhoaHoc = "Lập trình C# cơ bản",
                    MoTa       = "Khóa học nền tảng cho người mới bắt đầu",
                    CapDo      = "Cơ bản",
                    TinChi     = 3,
                    HocPhi     = "1,500,000",
                    TrangThai  = "Đang mở"
                }
            };

            var columns = new List<ExcelColumnDef<dynamic>>
            {
                new() { Header = "Tên khóa học",  ValueSelector = r => r.TenKhoaHoc, Width = 32 },
                new() { Header = "Mô tả",         ValueSelector = r => r.MoTa,       Width = 45 },
                new() { Header = "Cấp độ",        ValueSelector = r => r.CapDo,      Width = 16 },
                new() { Header = "Tín chỉ",       ValueSelector = r => r.TinChi,     Width = 10 },
                new() { Header = "Học phí (VNĐ)", ValueSelector = r => r.HocPhi,     Width = 16 },
                new() { Header = "Trạng thái",    ValueSelector = r => r.TrangThai,  Width = 14 },
            };

            var bytes = _excel.Export(
                sample.Cast<dynamic>(),
                columns,
                "Khóa học",
                "FILE MẪU NHẬP KHÓA HỌC");

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "FileMau_NhapKhoaHoc.xlsx");
        }

        [AuthorizeManager]
        public IActionResult Modules(int id)
        {
            var course = _uow.Courses.GetById(id);
            if (course == null) return NotFound();

            ViewData["CourseName"] = course.CourseName;
            return RedirectToAction("Index", "Module", new { courseId = id });
        }

        [HttpGet]
        [AuthorizeManager]
        public IActionResult GetActiveCourses()
        {
            var courses = _uow.Courses.GetAll()
                .Where(c => c.IsActive)
                .OrderBy(c => c.CourseName)
                .Select(c => new { c.CourseId, c.CourseName })
                .ToList();
            return Json(courses);
        }
    }
}