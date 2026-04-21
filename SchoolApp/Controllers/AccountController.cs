using Microsoft.AspNetCore.Mvc;
using SchoolApp.Models;
using SchoolApp.UnitOfWork;

namespace SchoolApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _uow;

        public AccountController(IUnitOfWork uow)
        {
            _uow = uow;
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

            var student = _uow.Students.GetByEmailAndPassword(model.Email, model.Password);
            if (student == null)
            {
                ModelState.AddModelError("", "Sai email hoặc mật khẩu");
                return View(model);
            }

            HttpContext.Session.SetString("StudentName", student.FullName);
            HttpContext.Session.SetInt32("StudentId", student.StudentId);

            const string ADMIN_EMAIL = "admin@gmail.com";
            string role = student.Email.Equals(ADMIN_EMAIL, StringComparison.OrdinalIgnoreCase)
                            ? "Admin"
                            : "Student";
            HttpContext.Session.SetString("Role", role);

            TempData["Success"] = "Đăng nhập thành công!";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}