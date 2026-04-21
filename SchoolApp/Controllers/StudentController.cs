    using Microsoft.AspNetCore.Mvc;
    using SchoolApp.Filters;
    using SchoolApp.Models;
    using SchoolApp.UnitOfWork;
    using X.PagedList;
    using X.PagedList.Extensions;

    namespace SchoolApp.Controllers
    {
        public class StudentController : Controller
        {
            private readonly IUnitOfWork _uow;

            public StudentController(IUnitOfWork uow)
            {
                _uow = uow;
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
            public IActionResult Create()
            {
                return View(new Student());
            }

            [HttpPost]
            [AuthorizeAdmin]
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

                student.RegisteredDate = DateTime.Now;
                _uow.Students.Add(student);
                _uow.SaveChanges();

                TempData["Success"] = "Thêm học viên thành công!";
                return RedirectToAction("Index");
            }

            [AuthorizeAdmin]
            public IActionResult Edit(int id)
            {
                var student = _uow.Students.GetById(id);
                if (student == null) return NotFound();
                return View(student);
            }

            [HttpPost]
            [AuthorizeAdmin]
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

            student.RegisteredDate = DateTime.Now;
            _uow.Students.Add(student);
            _uow.SaveChanges();
            return Json(new { success = true, message = "Thêm học viên thành công!" });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult EditAjax(Student student)
        {
            // Bỏ qua validate Password và Email khi sửa
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
                dateOfBirth = student.DateOfBirth.ToString("yyyy-MM-dd"), // format cho input date
                address = student.Address
            });
        }
        [HttpGet]
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
                dateOfBirth = student.DateOfBirth.ToString("dd/MM/yyyy"),
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
    }
    }