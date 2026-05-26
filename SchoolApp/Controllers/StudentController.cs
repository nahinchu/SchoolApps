using Microsoft.AspNetCore.Mvc;
using SchoolApp.Filters;
using SchoolApp.Models;
using SchoolApp.Services.ExcelExportService;
using SchoolApp.Services.HashPassService;
using SchoolApp.UnitOfWork;
using System.Globalization;
using X.PagedList;
using X.PagedList.Extensions;

namespace SchoolApp.Controllers
{
    public class StudentController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordService _passwordService;
        private readonly IExcelExportService _excel;
        private readonly IExcelImportService _excelImport;

        // ====== DI: inject thêm IPasswordService ======
        public StudentController(IUnitOfWork uow, IPasswordService passwordService, IExcelExportService excel, IExcelImportService excelImport)
        {
            _uow = uow;
            _passwordService = passwordService;
            _excel = excel;
            _excelImport = excelImport;
        }

        public IActionResult Index(string searchTerm, int page = 1)
        {
            int pageSize = 5;

            var result = _uow.Students.Search(searchTerm)
                .ToPagedList(page, pageSize);

            ViewData["SearchTerm"] = searchTerm;
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_StudentTable", result);
            }
            return View(result);
        }

        [HttpGet]
        [AuthorizeAdmin]
        [AuthorizeManager]
        public IActionResult Create()
        {
            return View(new Student());
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Student student)
        {
            if (!ModelState.IsValid)
                return View(student);

            if (_uow.Students.Any(s => s.Email == student.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng");
                return View(student);
            }

            student.Password = _passwordService.Hash(student.Password);
            student.Role = "student";  
            student.RegisteredDate = DateTime.Now;
            _uow.Students.Add(student);
            _uow.SaveChanges();

            TempData["Success"] = "Thêm học viên thành công!";
            return RedirectToAction("Index");
        }

        [AuthorizeAdmin]
        [AuthorizeManager]
        public IActionResult Edit(int id)
        {
            var student = _uow.Students.GetById(id);
            if (student == null) return NotFound();
            return View(student);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Student student)
        {
            if (!ModelState.IsValid)
                return View(student);

            var existing = _uow.Students.GetById(student.StudentId);
            if (existing == null)
            {
                TempData["Error"] = "Không tìm thấy học viên";
                return RedirectToAction("Index");
            }

            existing.FullName = student.FullName;
            existing.Phone = student.Phone;
            existing.DateOfBirth = student.DateOfBirth;
            existing.Address = student.Address;

            _uow.SaveChanges();

            TempData["Success"] = "Cập nhật học viên thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var student = _uow.Students.GetById(id);
            if (student == null)
            {
                TempData["Error"] = "Không tìm thấy học viên";
                return RedirectToAction("Index");
            }

            if (_uow.Enrollments.Any(e => e.StudentId == id))
            {
                TempData["Error"] = "Không thể xóa: học viên đã đăng ký khóa học.";
                return RedirectToAction("Index");
            }

            _uow.Students.Delete(student);
            _uow.SaveChanges();

            TempData["Success"] = "Đã xóa học viên!";
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            var student = _uow.Students.GetWithEnrollments(id);
            if (student == null) return NotFound();
            return View(student);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAjax(int id)
        {
            var student = _uow.Students.GetById(id);
            if (student == null)
                return Json(new { success = false, message = "Không tìm thấy học viên" });

            if (_uow.Enrollments.Any(e => e.StudentId == id))
                return Json(new { success = false, message = "Không thể xóa: học viên đã đăng ký khóa học." });

            _uow.Students.Delete(student);
            _uow.SaveChanges();
            return Json(new { success = true, message = "Đã xóa học viên!" });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAjax(Student student)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }

            if (_uow.Students.Any(s => s.Email == student.Email))
                return Json(new
                {
                    success = false,
                    errors = new Dictionary<string, string[]> {
                        { "Email", new[] { "Email này đã được sử dụng" } }
                    }
                });

            student.Password = _passwordService.Hash(student.Password);

            student.Role = "student";

            student.RegisteredDate = DateTime.Now;
            _uow.Students.Add(student);
            _uow.SaveChanges();
            return Json(new { success = true, message = "Thêm học viên thành công!" });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult EditAjax(Student student)
        {
            ModelState.Remove("Password");
            ModelState.Remove("Email");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }

            var existing = _uow.Students.GetById(student.StudentId);
            if (existing == null)
                return Json(new { success = false, message = "Không tìm thấy học viên" });

            existing.FullName = student.FullName;
            existing.Phone = student.Phone;
            existing.DateOfBirth = student.DateOfBirth;
            existing.Address = student.Address;

            _uow.SaveChanges();
            return Json(new { success = true, message = "Cập nhật học viên thành công!" });
        }

        [HttpGet]
        [AuthorizeAdmin]
        [AuthorizeManager]
        public IActionResult GetStudent(int id)
        {
            var student = _uow.Students.GetById(id);
            if (student == null) return NotFound();
            return Json(new
            {
                studentId = student.StudentId,
                fullName = student.FullName,
                email = student.Email,
                phone = student.Phone,
                dateOfBirth = student.DateOfBirth?.ToString("yyyy-MM-dd"),
                address = student.Address
            });
        }
        [HttpGet]
        [AuthorizeAdmin]
        [AuthorizeManager]
        public IActionResult GetStudentDetails(int id)
        {
            var student = _uow.Students.GetWithEnrollments(id);
            if (student == null) return NotFound();
            return Json(new
            {
                studentId = student.StudentId,
                fullName = student.FullName,
                email = student.Email,
                phone = student.Phone,
                dateOfBirth = student.DateOfBirth?.ToString("dd/MM/yyyy"),
                address = student.Address ?? "Chưa cập nhật",
                registeredDate = student.RegisteredDate.ToString("dd/MM/yyyy HH:mm"),
                enrollments = student.Enrollments.Select(e => new
                {
                    courseName = e.Course != null ? e.Course.CourseName : "N/A",
                    enrollDate = e.EnrollDate.ToString("dd/MM/yyyy"),
                    grade = e.Grade.HasValue ? e.Grade.Value.ToString("0.00") : "Chưa có"
                }).ToList()
            });
        }

        [HttpGet]
        [AuthorizeUser]
        public IActionResult MyProfile()
        {
            var myId = HttpContext.Session.GetInt32("StudentId");
            if (myId == null) return Unauthorized();

            var student = _uow.Students.GetById(myId.Value);
            if (student == null) return NotFound();

            return Json(new
            {
                studentId = student.StudentId,
                fullName = student.FullName,
                email = student.Email,
                phone = student.Phone,
                dateOfBirth = student.DateOfBirth?.ToString("yyyy-MM-dd"),
                address = student.Address ?? "Chưa cập nhật"
            });
        }



        [AuthorizeAdmin]
        [AuthorizeManager]
        public IActionResult ExportExcel(string? searchTerm)
        {
            var students = _uow.Students.Search(searchTerm).ToList();

            var columns = new List<ExcelColumnDef<Student>>
            {
                new() { Header = "ID",           ValueSelector = s => s.StudentId,    Width = 8  },
                new() { Header = "Họ và tên",    ValueSelector = s => s.FullName,    Width = 35 },
                new() { Header = "Email",        ValueSelector = s => s.Email,       Width = 45 },
                new() { Header = "Số điện thoại",ValueSelector = s => s.Phone,       Width = 12 },
                new() { Header = "Ngày sinh",    ValueSelector = s => s.DateOfBirth?.ToString("dd/MM/yyyy") ?? "Chưa cập nhật", Width = 18 },
                new() { Header = "Trạng thái",   ValueSelector = s => s.Address,  Width = 14 },
                new() { Header = "Ngày tạo",     ValueSelector = s => s.RegisteredDate, Width = 16, Format = "dd/MM/yyyy" },
            };

            var bytes = _excel.Export(students, columns, "Học viên", "DANH SÁCH HỌC VIÊN");
            var fileName = $"DanhSachHocVien_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn file Excel." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".xlsx")
                return Json(new { success = false, message = "Chỉ chấp nhận file .xlsx." });

            // Định nghĩa ánh xạ cột — tên Header phải khớp với file Excel
            var defaultPassword = _passwordService.Hash("SchoolApp@123");

            var columns = new List<ExcelColumnMap<Student>>
            {
                new() {
                    Header = "Họ và tên", Required = true,
                    Setter = (s, v) => s.FullName = v
                },
                new() {
                    Header = "Email", Required = true,
                    Setter = (s, v) => {
                        if (!v.Contains('@')) throw new Exception("Không đúng định dạng email");
                        s.Email = v;
                    }
                },
                new() {
                    Header = "Số điện thoại", Required = true,
                    Setter = (s, v) => {
                        if (v.Length < 9 || v.Length > 15)
                            throw new Exception("Phải từ 9 đến 15 ký tự");
                        s.Phone = v;
                    }
                },
                new() {
                    Header = "Ngày sinh", Required = false,
                    Setter = (s, v) => {
                        if (!DateTime.TryParseExact(v, new[]{ "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" },
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob))
                            throw new Exception("Định dạng phải là dd/MM/yyyy");
                        s.DateOfBirth = dob;
                    }
                },
                new() {
                    Header = "Địa chỉ", Required = false,
                    Setter = (s, v) => s.Address = v
                },
            };

            using var stream = file.OpenReadStream();
            var importResult = _excelImport.Import(stream, columns, () => new Student
            {
                Role = "student",
                Password = defaultPassword,
                RegisteredDate = DateTime.Now
            });

            // Lỗi cấp file (cột bắt buộc thiếu) → trả về ngay
            var fileErrors = importResult.Errors.Where(e => e.RowNumber == 0).ToList();
            if (fileErrors.Any())
                return Json(new { success = false, message = fileErrors.First().Message });

            int added = 0;
            var skipped = new List<object>();

            // Lỗi từng dòng trong Excel
            foreach (var err in importResult.Errors.Where(e => e.RowNumber > 0))
                skipped.Add(new { row = err.RowNumber, reason = err.Message });

            // Kiểm tra nghiệp vụ & lưu các dòng hợp lệ
            foreach (var student in importResult.ValidRows)
            {
                if (_uow.Students.Any(s => s.Email == student.Email))
                {
                    skipped.Add(new { row = -1, reason = $"Email \"{student.Email}\" đã tồn tại" });
                    continue;
                }

                _uow.Students.Add(student);
                added++;
            }

            if (added > 0) _uow.SaveChanges();

            return Json(new { success = true, added, skipped });
        }

        // ── TẢI FILE MẪU ──────────────────────────────────────────────────
        [AuthorizeAdmin]
        [AuthorizeManager]
        public IActionResult DownloadImportTemplate()
        {
            var sample = new[]
            {
                new {
                    HoVaTen      = "Nguyễn Văn A",
                    Email        = "nguyenvana@example.com",
                    SoDienThoai  = "0901234567",
                    NgaySinh     = "01/01/2000",
                    DiaChi       = "123 Lê Lợi, Hà Nội"
                }
            };

            // Dùng lại ExcelExportService với anonymous type để tạo template
            var columns = new List<ExcelColumnDef<dynamic>>
            {
                new() { Header = "Họ và tên",     ValueSelector = r => r.HoVaTen,     Width = 30 },
                new() { Header = "Email",           ValueSelector = r => r.Email,       Width = 32 },
                new() { Header = "Số điện thoại",  ValueSelector = r => r.SoDienThoai, Width = 18 },
                new() { Header = "Ngày sinh",       ValueSelector = r => r.NgaySinh,    Width = 16 },
                new() { Header = "Địa chỉ",         ValueSelector = r => r.DiaChi,      Width = 35 },
            };

            var bytes = _excel.Export(
                sample.Cast<dynamic>(),
                columns,
                "Học viên",
                "FILE MẪU NHẬP HỌC VIÊN — Mật khẩu mặc định: SchoolApp@123");

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "FileMau_NhapHocVien.xlsx");
        }
    }
}