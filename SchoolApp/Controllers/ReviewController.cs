using Microsoft.AspNetCore.Mvc;
using SchoolApp.Filters;
using SchoolApp.Models;
using SchoolApp.Services.NotificationService;
using SchoolApp.UnitOfWork;

namespace SchoolApp.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notifService;

        public ReviewController(IUnitOfWork uow, INotificationService notifService)
        {
            _uow = uow;
            _notifService = notifService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int courseId, int rating, string? comment)
        {
            var studentId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (studentId == null)
                return Json(new { success = false, message = "Vui lòng đăng nhập" });

            if (role != "Student")
                return Json(new { success = false, message = "Chỉ học viên mới có thể đánh giá" });

            bool enrolled = _uow.Enrollments.Any(e => e.UserId == studentId && e.CourseId == courseId);
            if (!enrolled)
                return Json(new { success = false, message = "Bạn chưa đăng ký khóa học này" });

            bool alreadyReviewed = _uow.Reviews.Any(r => r.UserId == studentId && r.CourseId == courseId);
            if (alreadyReviewed)
                return Json(new { success = false, message = "Bạn đã đánh giá khóa học này rồi" });

            if (rating < 1 || rating > 5)
                return Json(new { success = false, message = "Đánh giá không hợp lệ" });

            var review = new CourseReview
            {
                CourseId = courseId,
                UserId = studentId.Value,
                Rating = rating,
                Comment = comment?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _uow.Reviews.Add(review);
            _uow.SaveChanges();

            var user = _uow.Users.GetById(studentId.Value);
            return Json(new
            {
                success = true,
                message = "Cảm ơn bạn đã đánh giá!",
                review = new
                {
                    rating,
                    comment = review.Comment,
                    studentName = user?.FullName ?? user?.Email ?? "Ẩn danh",
                    createdAt = review.CreatedAt.ToString("dd/MM/yyyy")
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int reviewId, int courseId)
        {
            var studentId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (studentId == null)
                return Json(new { success = false, message = "Vui lòng đăng nhập" });

            var review = _uow.Reviews.GetById(reviewId);
            if (review == null)
                return Json(new { success = false, message = "Không tìm thấy đánh giá" });

            bool isOwner = review.UserId == studentId;
            bool isAdminOrMgr = role == "Admin" || role == "Manager";

            if (!isOwner && !isAdminOrMgr)
                return Json(new { success = false, message = "Không có quyền xóa đánh giá này" });

            _uow.Reviews.Delete(review);
            _uow.SaveChanges();

            return Json(new { success = true, message = "Đã xóa đánh giá" });
        }
    }
}
