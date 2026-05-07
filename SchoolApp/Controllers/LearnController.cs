using Microsoft.AspNetCore.Mvc;
using SchoolApp.Filters;
using SchoolApp.Models;
using SchoolApp.Models.Enums;
using SchoolApp.UnitOfWork;

namespace SchoolApp.Controllers
{
    [AuthorizeUser]
    public class LearnController : Controller
    {
        private readonly IUnitOfWork _uow;

        public LearnController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: /Learn/Course/5?lessonId=12
        public IActionResult Course(int id, int? lessonId)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null) return RedirectToAction("Login", "Account");

            // 1) Phải đăng ký mới được học
            var isEnrolled = _uow.Enrollments.IsEnrolled(studentId.Value, id);
            if (!isEnrolled)
            {
                TempData["Error"] = "Bạn chưa đăng ký khóa học này";
                return RedirectToAction("Index", "Course");
            }

            // 2) Lấy course với full tree (Modules → Lessons → Quiz)
            var course = _uow.Courses.GetCourseWithFullTree(id);
            if (course == null) return NotFound();

            // 3) Chỉ giữ những module/lesson đã publish
            var visibleModules = course.Modules
                .Where(m => m.IsPublished)
                .OrderBy(m => m.OrderIndex)
                .ToList();
            foreach (var m in visibleModules)
            {
                m.Lessons = m.Lessons
                    .Where(l => l.IsPublished)
                    .OrderBy(l => l.OrderIndex)
                    .ToList();
            }
            course.Modules = visibleModules;

            // 4) Tải tiến độ của học viên trong khoá này
            var progressList = _uow.LessonProgresses
                .GetByStudentAndCourse(studentId.Value, id)
                .ToList();
            var progressMap = progressList.ToDictionary(p => p.LessonId);

            // 5) Xác định lesson đang chọn (ưu tiên lessonId từ URL, không thì lesson đầu tiên)
            var allLessons = visibleModules.SelectMany(m => m.Lessons).ToList();
            Lesson? currentLesson = null;
            if (lessonId.HasValue)
            {
                currentLesson = allLessons.FirstOrDefault(l => l.LessonId == lessonId.Value);
            }
            currentLesson ??= allLessons.FirstOrDefault();

            // 6) Tự động tạo/cập nhật LessonProgress khi truy cập
            if (currentLesson != null)
            {
                if (progressMap.TryGetValue(currentLesson.LessonId, out var p))
                {
                    p.LastAccessedAt = DateTime.UtcNow;
                    if (p.Status == ProgressStatus.NotStarted)
                    {
                        p.Status = ProgressStatus.InProgress;
                        if (p.ProgressPercent == 0) p.ProgressPercent = 10;
                    }
                }
                else
                {
                    var np = new LessonProgress
                    {
                        StudentId = studentId.Value,
                        LessonId = currentLesson.LessonId,
                        Status = ProgressStatus.InProgress,
                        ProgressPercent = 10,
                        LastAccessedAt = DateTime.UtcNow
                    };
                    _uow.LessonProgresses.Add(np);
                    progressMap[currentLesson.LessonId] = np;
                }
                _uow.SaveChanges();
            }

            // 7) Tính prev/next để render nút điều hướng
            int currentIndex = currentLesson != null
                ? allLessons.FindIndex(l => l.LessonId == currentLesson.LessonId)
                : -1;
            var prevLesson = currentIndex > 0 ? allLessons[currentIndex - 1] : null;
            var nextLesson = (currentIndex >= 0 && currentIndex < allLessons.Count - 1)
                ? allLessons[currentIndex + 1] : null;

            ViewData["CurrentLesson"] = currentLesson;
            ViewData["ProgressMap"] = progressMap;
            ViewData["PrevLesson"] = prevLesson;
            ViewData["NextLesson"] = nextLesson;
            ViewData["TotalLessons"] = allLessons.Count;
            ViewData["CompletedCount"] = progressMap.Values
                .Count(p => p.Status == ProgressStatus.Completed);

            return View(course);
        }

        // POST: /Learn/MarkComplete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkComplete(int lessonId)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return Json(new { success = false, message = "Vui lòng đăng nhập" });

            var lesson = _uow.Lessons.GetById(lessonId);
            if (lesson == null)
                return Json(new { success = false, message = "Không tìm thấy bài học" });

            var progress = _uow.LessonProgresses.GetProgress(studentId.Value, lessonId);
            if (progress == null)
            {
                progress = new LessonProgress
                {
                    StudentId = studentId.Value,
                    LessonId = lessonId,
                    Status = ProgressStatus.Completed,
                    ProgressPercent = 100,
                    CompletedAt = DateTime.UtcNow,
                    LastAccessedAt = DateTime.UtcNow
                };
                _uow.LessonProgresses.Add(progress);
            }
            else
            {
                progress.Status = ProgressStatus.Completed;
                progress.ProgressPercent = 100;
                progress.CompletedAt = DateTime.UtcNow;
                progress.LastAccessedAt = DateTime.UtcNow;
            }
            _uow.SaveChanges();

            return Json(new { success = true, message = "Đã đánh dấu hoàn thành!" });
        }
    }
}