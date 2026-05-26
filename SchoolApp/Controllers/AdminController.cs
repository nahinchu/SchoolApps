using Microsoft.AspNetCore.Mvc;
using SchoolApp.Filters;
using SchoolApp.Models.Enums;
using SchoolApp.Services.HashPassService;
using SchoolApp.UnitOfWork;
using SchoolApp.DTOs;
using SchoolApp.Models;
namespace SchoolApp.Controllers
{
    [AuthorizeManager]   // Admin + Manager đều vào được
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordService _passwordService;

        public AdminController(IUnitOfWork uow, IPasswordService passwordService)
        {
            _uow = uow;
            _passwordService = passwordService;
        }

        public IActionResult Dashboard()
        {
            var now = DateTime.Now;
            var months = Enumerable.Range(0, 12).Select(i => now.AddMonths(-11 + i)).ToList();
            var monthLabels = months.Select(m => m.ToString("MM/yyyy")).ToArray();
            var allStudents = _uow.Students.GetAll().ToList();
            var allEnrollments = _uow.Enrollments.GetAll().ToList();
            var allPayments = _uow.Payments.GetAll().ToList();
            var allCourses = _uow.Courses.GetAll().ToList();
            var studentDict = allStudents.ToDictionary(s => s.StudentId, s => s.FullName);
            var courseDict = allCourses.ToDictionary(c => c.CourseId, c => c.CourseName);

            var vm = new AdminDashboardViewModel
            {
                TotalStudents = allStudents.Count(s => s.Role.Equals("student", StringComparison.OrdinalIgnoreCase)),
                TotalManagers = allStudents.Count(s => s.Role.Equals("manager", StringComparison.OrdinalIgnoreCase)),
                TotalCourses = allCourses.Count(),
                TotalEnrollments = allEnrollments.Count(),
                TotalRevenue = allPayments.Where(p => p.Status == PaymentStatus.PAID).Sum(p => p.Amount),

                MonthLabels = monthLabels,

                //chart 1
                NewStudentsByMonth = months.Select(m => allStudents.Count(s =>
                s.Role.Equals("student", StringComparison.OrdinalIgnoreCase) &&
                s.RegisteredDate.Year == m.Year &&
                s.RegisteredDate.Month == m.Month)).ToArray(),

                //chart 2
                RevenueByMonth = months.Select(m => allPayments.Where(p =>
                p.Status == PaymentStatus.PAID &&
                p.PaidAt.HasValue &&
                p.PaidAt.Value.Year == m.Year &&
                p.PaidAt.Value.Month == m.Month).Sum(p => p.Amount)).ToArray(),

                //chart 3
                TopCourseNames = allCourses.OrderByDescending(c => allEnrollments.Count(e => e.CourseId == c.CourseId))
                                           .Take(8)
                                           .Select(c => c.CourseName)
                                           .ToArray(),
                TopCourseEnrollments = allCourses.OrderByDescending(c => allEnrollments.Count(e => e.CourseId == c.CourseId))
                                                 .Take(8)
                                                 .Select(c => allEnrollments.Count(e => e.CourseId == c.CourseId))
                                                 .ToArray(),
                //chart 4
                PaidCount = allPayments.Count(p => p.Status == PaymentStatus.PAID),
                PendingCount = allPayments.Count(p => p.Status == PaymentStatus.PENDING),
                CancelledCount = allPayments.Count(p => p.Status == PaymentStatus.CANCELLED),

                //chart 5
                RecentEnrollments = _uow.Enrollments.SearchWithDetails("")
                .OrderByDescending(e => e.EnrollDate)
                .Take(10)
                .Select(e => new RecentEnrollmentRow
                {
                    StudentName = e.Student != null ? e.Student.FullName : "N/A",
                    CourseName = e.Course != null ? e.Course.CourseName : "N/A",
                    EnrollDate = e.EnrollDate,
                    Grade = e.Grade
                }).ToList(),

                RecentPayments = allPayments
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .Select(p => new RecentPaymentRow
                {
                    StudentName = studentDict.GetValueOrDefault(p.StudentId, "N/A"),
                    CourseName = courseDict.GetValueOrDefault(p.CourseId, "N/A"),
                    Amount = p.Amount,
                    Status = p.Status.ToString(),
                    CreatedAt = p.CreatedAt
                }).ToList()
            };

            return View(vm);
        }

        [AuthorizeAdmin]
        public IActionResult Managers()
        {
            var managers = _uow.Students.GetAll()
                .Where(s => s.Role.ToLower() == "manager")
                .ToList();
            return View(managers);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult CreateManager(Student model)
        {
            ModelState.Remove("Role");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin hợp lệ.";
                return RedirectToAction("Managers");
            }

            if (_uow.Students.Any(s => s.Email == model.Email))
            {
                TempData["Error"] = "Email đã tồn tại. Vui lòng chọn email khác.";
                return RedirectToAction("Managers");
            }

            model.Role = "manager";
            model.Password = _passwordService.Hash(model.Password);
            model.RegisteredDate = DateTime.Now;
            _uow.Students.Add(model);
            _uow.SaveChanges();

            TempData["Success"] = $"Dang ký quản lý {model.FullName} thành công!";
            return RedirectToAction("Managers");

        }
        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteManager(int id)
        {
            var manager = _uow.Students.GetById(id);
            if (manager == null || !manager.Role.Equals("manager", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Quản lý không tồn tại.";
                return RedirectToAction("Managers");
            }
            _uow.Students.Delete(manager);
            _uow.SaveChanges();
            TempData["Success"] = $"Xóa quản lý {manager.FullName} thành công!";
            return RedirectToAction("Managers");
        }
    }
}