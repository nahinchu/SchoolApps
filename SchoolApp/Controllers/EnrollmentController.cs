using Microsoft.AspNetCore.Mvc;
using SchoolApp.Filters;
using SchoolApp.Models;
using SchoolApp.UnitOfWork;
using X.PagedList;
using X.PagedList.Extensions;

namespace SchoolApp.Controllers
{
    public class EnrollmentController : Controller
    {
        private readonly IUnitOfWork _uow;

        public EnrollmentController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        [AuthorizeAdmin]
        public IActionResult Index(string searchTerm, int page = 1)
        {
            int pageSize = 5;

            var result = _uow.Enrollments.SearchWithDetails(searchTerm)
                .ToPagedList(page, pageSize);

            ViewData["SearchTerm"] = searchTerm;
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EnrollmentTable", result);
            }
            return View(result);
        }

        [AuthorizeAdmin]
        public IActionResult Edit(int id)
        {
            var enrollment = _uow.Enrollments.GetWithDetails(id);
            if (enrollment == null) return NotFound();
            return View(enrollment);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Enrollment enrollment)
        {
            var existing = _uow.Enrollments.GetById(enrollment.EnrollmentId);
            if (existing == null)
            {
                TempData["Error"] = "Không tìm thấy đăng ký";
                return RedirectToAction("Index");
            }

            existing.Grade = enrollment.Grade;
            existing.Notes = enrollment.Notes;

            _uow.SaveChanges();

            TempData["Success"] = "Chấm điểm thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var enrollment = _uow.Enrollments.GetById(id);
            if (enrollment == null)
            {
                TempData["Error"] = "Không tìm thấy đăng ký";
                return RedirectToAction("Index");
            }

            _uow.Enrollments.Delete(enrollment);
            _uow.SaveChanges();

            TempData["Success"] = "Đã xóa đăng ký!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(int courseId)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            var role = HttpContext.Session.GetString("Role");

            if (studentId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            if (role == "Admin")
            {
                return Json(new { success = false, message = "Admin không thể đăng ký khóa học" });
            }

            var course = _uow.Courses.GetById(courseId);
            if (course == null || !course.IsActive)
            {
                return Json(new { success = false, message = "Khóa học không tồn tại hoặc đã đóng" });
            }

            bool exists = _uow.Enrollments.Any(e =>
                e.StudentId == studentId && e.CourseId == courseId);

            if (exists)
            {
                return Json(new { success = false, message = "Bạn đã đăng ký khóa học này rồi" });
            }

            var enrollment = new Enrollment
            {
                StudentId = studentId.Value,
                CourseId = courseId,
                EnrollDate = DateTime.Now,
                Notes = ""
            };

            _uow.Enrollments.Add(enrollment);
            _uow.SaveChanges();

            return Json(new { success = true, message = $"Đăng ký khóa học \"{course.CourseName}\" thành công!" });
        }

        public IActionResult MyEnrollments(int page = 1)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập";
                return RedirectToAction("Login", "Account");
            }

            int pageSize = 5;
            var myEnrollments = _uow.Enrollments.GetByStudent(studentId.Value)
                .ToPagedList(page, pageSize);

            return View(myEnrollments);
        }
        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAjax(int id)
        {
            var enrollment = _uow.Enrollments.GetById(id);
            if (enrollment == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đăng ký" });
            }
            _uow.Enrollments.Delete(enrollment);
            _uow.SaveChanges();
            return Json(new { success = true, message = "Đã xóa đăng ký!" });
        }
        [HttpGet]
        [AuthorizeAdmin]
        public IActionResult GetEnrollment(int id)
        {
            var enrollment = _uow.Enrollments.GetWithDetails(id);
            if (enrollment == null) return NotFound();
            return Json(new
            {
                enrollmentId = enrollment.EnrollmentId,
                studentName = enrollment.Student?.FullName,
                courseName = enrollment.Course?.CourseName,
                enrollDate = enrollment.EnrollDate.ToString("dd/MM/yyyy"),
                grade = enrollment.Grade,
                notes = enrollment.Notes ?? ""
            });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult EditAjax(Enrollment enrollment)
        {
            ModelState.Remove("StudentId");
            ModelState.Remove("CourseId");
            ModelState.Remove("Student");
            ModelState.Remove("Course");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }

            var existing = _uow.Enrollments.GetById(enrollment.EnrollmentId);
            if (existing == null)
                return Json(new { success = false, message = "Không tìm thấy đăng ký" });

            existing.Grade = enrollment.Grade;
            existing.Notes = enrollment.Notes;

            _uow.SaveChanges();
            return Json(new { success = true, message = "Chấm điểm thành công!" });
        }
    }
}