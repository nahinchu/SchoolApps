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

            var result = _uow.Users.Search(searchTerm)
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
            return View(new User());
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult Create(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            if (_uow.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng");
                return View(user);
            }

            user.Password = _passwordService.Hash(
                string.IsNullOrWhiteSpace(user.Password) ? "SchoolApp@123" : user.Password);
            user.Role = "student";
            user.RegisteredDate = DateTime.Now;
            user.IsEmailVerified = true;
            _uow.Users.Add(user);
            _uow.SaveChanges();

            TempData["Success"] = "Thêm học viên thành công!";
            return RedirectToAction("Index");
        }

        [AuthorizeAdmin]
        [AuthorizeManager]
        public IActionResult Edit(int id)
        {
            var user = _uow.Users.GetById(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            var existing = _uow.Users.GetById(user.UserId);
            if (existing == null)
            {
                TempData["Error"] = "Không tìm thấy học viên";
                return RedirectToAction("Index");
            }

            existing.FullName = user.FullName;
            existing.Phone = user.Phone;
            existing.DateOfBirth = user.DateOfBirth;
            existing.Address = user.Address;

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
            var user = _uow.Users.GetById(id);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy học viên";
                return RedirectToAction("Index");
            }

            if (_uow.Enrollments.Any(e => e.UserId == id))
            {
                TempData["Error"] = "Không thể xóa: học viên đã đăng ký khóa học.";
                return RedirectToAction("Index");
            }

            _uow.Users.Delete(user);
            _uow.SaveChanges();

            TempData["Success"] = "Đã xóa học viên!";
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            var user = _uow.Users.GetWithEnrollments(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAjax(int id)
        {
            var user = _uow.Users.GetById(id);
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy học viên" });

            if (_uow.Enrollments.Any(e => e.UserId == id))
                return Json(new { success = false, message = "Không thể xóa: học viên đã đăng ký khóa học." });

            _uow.Users.Delete(user);
            _uow.SaveChanges();
            return Json(new { success = true, message = "Đã xóa học viên!" });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAjax(User user)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }

            if (_uow.Users.Any(u => u.Email == user.Email))
                return Json(new
                {
                    success = false,
                    errors = new Dictionary<string, string[]> {
                        { "Email", new[] { "Email này đã được sử dụng" } }
                    }
                });

            user.Password = _passwordService.Hash(
                string.IsNullOrWhiteSpace(user.Password) ? "SchoolApp@123" : user.Password);
            user.Role = "student";
            user.RegisteredDate = DateTime.Now;
            user.IsEmailVerified = true;
            _uow.Users.Add(user);
            _uow.SaveChanges();
            return Json(new { success = true, message = "Thêm học viên thành công!" });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AuthorizeManager]
        [ValidateAntiForgeryToken]
        public IActionResult EditAjax(User user)
        {
            ModelState.Remove("Password");
            ModelState.Remove("Email");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }

            var existing = _uow.Users.GetById(user.UserId);
            if (existing == null)
                return Json(new { success = false, message = "Không tìm thấy học viên" });

            existing.FullName = user.FullName;
            existing.Phone = user.Phone;
            existing.DateOfBirth = user.DateOfBirth;
            existing.Address = user.Address;

            _uow.SaveChanges();
            return Json(new { success = true, message = "Cập nhật học viên thành công!" });
        }

        [HttpGet]
        [AuthorizeAdmin]
        [AuthorizeManager]
        public IActionResult GetStudent(int id)
        {
            var user = _uow.Users.GetById(id);
            if (user == null) return NotFound();
            return Json(new
            {
                userId = user.UserId,
                fullName = user.FullName,
                email = user.Email,
                phone = user.Phone,
                dateOfBirth = user.DateOfBirth?.ToString("yyyy-MM-dd"),
                address = user.Address
            });
        }

        [HttpGet]
        [AuthorizeAdmin]
        [AuthorizeManager]
        public IActionResult GetStudentDetails(int id)
        {
            var user = _uow.Users.GetWithEnrollments(id);
            if (user == null) return NotFound();
            return Json(new
            {
                userId = user.UserId,
                fullName = user.FullName,
                email = user.Email,
                phone = user.Phone,
                dateOfBirth = user.DateOfBirth?.ToString("dd/MM/yyyy"),
                address = user.Address ?? "Chưa cập nhật",
                registeredDate = user.RegisteredDate.ToString("dd/MM/yyyy HH:mm"),
                enrollments = user.Enrollments.Select(e => new
                {
                    courseName = e.Course != null ? e.Course.CourseName : "N/A",
                    enrollDate = e.EnrollDate.ToString("dd/MM/yyyy")
                }).ToList()
            });
        }

        [HttpGet]
        [AuthorizeUser]
        public IActionResult MyProfile()
        {
            var myId = HttpContext.Session.GetInt32("UserId");
            if (myId == null) return Unauthorized();

            var user = _uow.Users.GetById(myId.Value);
            if (user == null) return NotFound();

            return Json(new
            {
                userId = user.UserId,
                fullName = user.FullName,
                email = user.Email,
                phone = user.Phone,
                dateOfBirth = user.DateOfBirth?.ToString("yyyy-MM-dd"),
                address = user.Address ?? "Chưa cập nhật"
            });
        }

        [AuthorizeAdmin]
        [AuthorizeManager]
        public IActionResult ExportExcel(string? searchTerm)
        {
            var users = _uow.Users.Search(searchTerm).ToList();

            var columns = new List<ExcelColumnDef<User>>
            {
                new() { Header = "ID",           ValueSelector = u => u.UserId,    Width = 8  },
                new() { Header = "Họ và tên",    ValueSelector = u => u.FullName,    Width = 35 },
                new() { Header = "Email",        ValueSelector = u => u.Email,       Width = 45 },
                new() { Header = "Số điện thoại",ValueSelector = u => u.Phone,       Width = 12 },
                new() { Header = "Ngày sinh",    ValueSelector = u => u.DateOfBirth?.ToString("dd/MM/yyyy") ?? "Chưa cập nhật", Width = 18 },
                new() { Header = "Địa chỉ",      ValueSelector = u => u.Address,  Width = 14 },
                new() { Header = "Ngày tạo",     ValueSelector = u => u.RegisteredDate, Width = 16, Format = "dd/MM/yyyy" },
            };

            var bytes = _excel.Export(users, columns, "Học viên", "DANH SÁCH HỌC VIÊN");
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

            var defaultPassword = _passwordService.Hash("SchoolApp@123");

            var columns = new List<ExcelColumnMap<User>>
            {
                new() {
                    Header = "Họ và tên", Required = false,
                    Setter = (u, v) => u.FullName = v
                },
                new() {
                    Header = "Email", Required = true,
                    Setter = (u, v) => {
                        if (!v.Contains('@')) throw new Exception("Không đúng định dạng email");
                        u.Email = v;
                    }
                },
                new() {
                    Header = "Số điện thoại", Required = false,
                    Setter = (u, v) => {
                        if (v.Length < 9 || v.Length > 15)
                            throw new Exception("Phải từ 9 đến 15 ký tự");
                        u.Phone = v;
                    }
                },
                new() {
                    Header = "Ngày sinh", Required = false,
                    Setter = (u, v) => {
                        if (!DateTime.TryParseExact(v, new[]{ "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" },
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob))
                            throw new Exception("Định dạng phải là dd/MM/yyyy");
                        u.DateOfBirth = dob;
                    }
                },
                new() {
                    Header = "Địa chỉ", Required = false,
                    Setter = (u, v) => u.Address = v
                },
            };

            using var stream = file.OpenReadStream();
            var importResult = _excelImport.Import(stream, columns, () => new User
            {
                Role = "student",
                Password = defaultPassword,
                RegisteredDate = DateTime.Now,
                IsEmailVerified = true
            });

            var fileErrors = importResult.Errors.Where(e => e.RowNumber == 0).ToList();
            if (fileErrors.Any())
                return Json(new { success = false, message = fileErrors.First().Message });

            int added = 0;
            var skipped = new List<object>();

            foreach (var err in importResult.Errors.Where(e => e.RowNumber > 0))
                skipped.Add(new { row = err.RowNumber, reason = err.Message });

            foreach (var user in importResult.ValidRows)
            {
                if (_uow.Users.Any(u => u.Email == user.Email))
                {
                    skipped.Add(new { row = -1, reason = $"Email \"{user.Email}\" đã tồn tại" });
                    continue;
                }

                _uow.Users.Add(user);
                added++;
            }

            if (added > 0) _uow.SaveChanges();

            return Json(new { success = true, added, skipped });
        }

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
