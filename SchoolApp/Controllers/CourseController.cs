using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolApp.Filters;
using SchoolApp.Models;
using SchoolApp.UnitOfWork;
using X.PagedList;
using X.PagedList.Extensions;

namespace SchoolApp.Controllers
{
    public class CourseController : Controller
    {
        private readonly IUnitOfWork _uow;

        public CourseController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: /Course
        public IActionResult Index(string searchTerm, int page = 1)
        {
            int pageSize = 5;

            var result = _uow.Courses.SearchByName(searchTerm)
                .ToPagedList(page, pageSize);

            ViewData["SearchTerm"] = searchTerm;
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CourseTable", result);
            }
            return View(result);
        }

        [HttpGet]
        [AuthorizeAdmin]
        public IActionResult Create()
        {
            return View(new Course { IsActive = true });
        }

        [HttpPost]
        [AuthorizeAdmin]
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

        [AuthorizeAdmin]
        public IActionResult Edit(int id)
        {
            var course = _uow.Courses.GetById(id);
            if (course == null) return NotFound();
            return View(course);
        }

        [HttpPost]
        [AuthorizeAdmin]
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
                //return Json(new { success = true, message = "Đã xóa khóa học!" });
            }

            bool hasEnrollments = _uow.Enrollments.Any(e => e.CourseId == id);
            if (hasEnrollments)
            {
                TempData["Error"] = "Không thể xóa: đã có học viên đăng ký.";
                return RedirectToAction("Index");
                //return Json(new { success = false, message = "Không thể xóa: đã có học viên đăng ký." });

            }

            _uow.Courses.Delete(course);
            _uow.SaveChanges();

            TempData["Success"] = "Đã xóa khóa học!";
            return RedirectToAction("Index");
            //return Json(new { success = true, message = "Đã xóa khóa học!" });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAjax(Course course)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(X => X.Value.Errors.Count > 0)
                   .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }
            course.CreatedDate = DateTime.Now;
            _uow.Courses.Add(course);
            _uow.SaveChanges();
            return Json(new { success = true, message = "Thêm khóa học thành công!" });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult EditAjax(Course course)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(X => X.Value.Errors.Count > 0)
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
            return Json(new { success = true, message = "Đã xóa khóa học!" });}
        }



}