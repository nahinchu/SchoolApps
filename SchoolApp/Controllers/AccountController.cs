using Microsoft.AspNetCore.Mvc;
using SchoolApp.DTOs;
using SchoolApp.Filters;
using SchoolApp.Models;
using SchoolApp.Services;
using SchoolApp.UnitOfWork;

namespace SchoolApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordService _passwordService;

        public AccountController(IUnitOfWork uow, IPasswordService passwordService)
        {
            _uow = uow;
            _passwordService = passwordService;
        }

        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var student = _uow.Students.GetByEmail(model.Email);

            if (student == null)
            {
                _passwordService.Verify(model.Password,
                    "$2a$12$abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUV");
                ModelState.AddModelError("", "Sai email hoặc mật khẩu");
                return View(model);
            }

            if (!_passwordService.Verify(model.Password, student.Password))
            {
                ModelState.AddModelError("", "Sai email hoặc mật khẩu");
                return View(model);
            }

            HttpContext.Session.SetString("StudentName", student.FullName);
            HttpContext.Session.SetInt32("StudentId", student.StudentId);

            string role = student.Role.Equals("admin", StringComparison.OrdinalIgnoreCase)
                ? "Admin"
                : "Student";
            HttpContext.Session.SetString("Role", role);

            TempData["Success"] = "Đăng nhập thành công!";
            return RedirectToAction("Index", "Home");
        }
        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("Role") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Kiểm tra email đã tồn tại
            bool emailExists = _uow.Students.GetAll()
                .Any(s => s.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase));

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                return View(model);
            }


            var student = new Student
            {
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim().ToLower(),
                Password = _passwordService.Hash(model.Password),
                Phone = model.Phone?.Trim(),
                DateOfBirth = model.DateOfBirth
            };

            _uow.Students.Add(student);
            _uow.SaveChanges();

            // Tự động đăng nhập sau khi đăng ký
            HttpContext.Session.SetString("StudentName", student.FullName);
            HttpContext.Session.SetInt32("StudentId", student.StudentId);
            HttpContext.Session.SetString("Role", "Student");

            TempData["Success"] = "Đăng ký thành công! Chào mừng " + student.FullName;
            return RedirectToAction("Index", "Course");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AuthorizeUser]
        public IActionResult EditProfile()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            var student = _uow.Students.GetById(studentId!.Value);
            if (student == null) return NotFound();

            var model = new EditProfileViewModel
            {
                FullName = student.FullName,
                Phone = student.Phone,
                DateOfBirth = student.DateOfBirth,
                Address = student.Address
            };
            return View(model);
        }

        [HttpPost]
        [AuthorizeUser]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var studentId = HttpContext.Session.GetInt32("StudentId");
            var student = _uow.Students.GetById(studentId!.Value);
            if (student == null) return NotFound();

            student.FullName = model.FullName.Trim();
            student.Phone = model.Phone?.Trim();
            student.DateOfBirth = model.DateOfBirth;
            student.Address = model.Address?.Trim();

            _uow.SaveChanges();

            HttpContext.Session.SetString("StudentName", student.FullName);

            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("EditProfile");
        }

        [HttpPost]
        [AuthorizeUser]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            TempData["ActiveTab"] = "password";

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value!.Errors.Count > 0)
                    .ToDictionary(k => k.Key, v => v.Value!.Errors[0].ErrorMessage);
                TempData["PwdErrors"] = System.Text.Json.JsonSerializer.Serialize(errors);
                return RedirectToAction("EditProfile");
            }

            var studentId = HttpContext.Session.GetInt32("StudentId");
            var student = _uow.Students.GetById(studentId!.Value);
            if (student == null) return NotFound();

            if (!_passwordService.Verify(model.CurrentPassword, student.Password))
            {
                TempData["PwdErrors"] = System.Text.Json.JsonSerializer.Serialize(
                    new Dictionary<string, string> { { "CurrentPassword", "Mật khẩu hiện tại không đúng" } });
                return RedirectToAction("EditProfile");
            }

            student.Password = _passwordService.Hash(model.NewPassword);
            _uow.SaveChanges();

            TempData["ActiveTab"] = null;
            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("EditProfile");
        }
    }
}