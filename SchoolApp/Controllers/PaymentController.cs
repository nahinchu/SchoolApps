using Microsoft.AspNetCore.Mvc;
using SchoolApp.DTOs;
using SchoolApp.Filters;
using SchoolApp.Models;
using SchoolApp.Models.Enums;
using SchoolApp.Services;
using SchoolApp.UnitOfWork;

namespace SchoolApp.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PayOSService _payOS;
        private readonly IUnitOfWork _uow;

        public PaymentController(IUnitOfWork uow, PayOSService payOS)
        {
            _uow = uow;
            _payOS = payOS;
        }

        [AuthorizeUser]
        public IActionResult Checkout(int courseID)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            var role = HttpContext.Session.GetString("Role");

            if (role == "Admin")
                return RedirectToAction("Index", "Course");

            var course = _uow.Courses.GetById(courseID);
            if (course == null || !course.IsActive)
            {
                TempData["Error"] = "Khóa học không tồn tại hoặc đã đóng.";
                return RedirectToAction("Index", "Course");
            }
            bool alreadyEnrolled = _uow.Enrollments.GetAll()
                .Any(e => e.StudentId == studentId && e.CourseId == courseID);
            if (alreadyEnrolled)
            {
                TempData["Error"] = "Bạn đã đăng ký khóa học này.";
                return RedirectToAction("Index", "Course");
            }
            if (course.Fee == 0)
            {
                _uow.Enrollments.Add(new Enrollment
                {
                    StudentId = studentId!.Value,
                    CourseId = courseID,
                    EnrollDate = DateTime.Now,
                    Notes = ""
                });
                _uow.SaveChanges();
                TempData["Success"] = $"Đăng ký khóa học \"{course.CourseName}\" thành công!";
                return RedirectToAction("MyEnrollments", "Enrollment");
            }
            return View(course);
        }

        [HttpPost]
        [AuthorizeUser]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePaymentLink(int courseId)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            var role = HttpContext.Session.GetString("Role");

            if (role == "Admin")
                return Json(new { success = false, message = "Admin không thể thanh toán" });

            var course = _uow.Courses.GetById(courseId);
            if (course == null || !course.IsActive)
                return Json(new { success = false, message = "Khóa học không tồn tại" });

            bool alreadyEnrolled = _uow.Enrollments.Any(e =>
                e.StudentId == studentId && e.CourseId == courseId);
            if (alreadyEnrolled)
                return Json(new { success = false, message = "Bạn đã đăng ký khóa học này rồi" });

            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var payment = new Payment
            {
                StudentId = studentId!.Value,
                CourseId = courseId,
                OrderCode = orderCode,
                Amount = course.Fee,
                Status = PaymentStatus.PENDING,
                CreatedAt = DateTime.Now
            };
            _uow.Payments.Add(payment);
            _uow.SaveChanges();

            var returnUrl = Url.Action("Success", "Payment", new { orderCode }, Request.Scheme)!;
            var cancelUrl = Url.Action("Cancel", "Payment", new { orderCode }, Request.Scheme)!;

            var description = $"Hoc phi {course.CourseName}";
            if (description.Length > 25) description = description[..25];

            var items = new List<PayOSItemDto>
            {
                new PayOSItemDto { name = course.CourseName, quantity = 1, price = (int)course.Fee }
            };

            try
            {
                var result = await _payOS.CreatePaymentLinkAsync(
                    orderCode, (int)course.Fee, description, returnUrl, cancelUrl, items);

                if (result.code == "00" && result.data != null)
                    return Json(new { success = true, checkoutUrl = result.data.checkoutUrl });

                // Rollback payment record nếu tạo link thất bại
                var saved = _uow.Payments.GetByOrderCode(orderCode);
                if (saved != null) _uow.Payments.Delete(saved);
                _uow.SaveChanges();

                return Json(new { success = false, message = result.desc ?? "Không thể tạo link thanh toán" });
            }
            catch (Exception ex)
            {
                var saved = _uow.Payments.GetByOrderCode(orderCode);
                if (saved != null) _uow.Payments.Delete(saved);
                _uow.SaveChanges();

                return Json(new { success = false, message = "Lỗi kết nối: " + ex.Message });
            }
        }
        public async Task<IActionResult> Success(long orderCode)
        {
            var payment = _uow.Payments.GetByOrderCode(orderCode);
            if (payment == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin thanh toán";
                return RedirectToAction("Index", "Course");
            }

            if (payment.Status == PaymentStatus.PAID)
            {
                TempData["Success"] = "Thanh toán đã được xử lý trước đó";
                return RedirectToAction("MyEnrollments", "Enrollment");
            }

            try
            {
                var info = await _payOS.GetPaymentInfoAsync(orderCode);

                if (info.code == "00" && info.data?.status == "PAID")
                {
                    payment.Status = PaymentStatus.PAID;
                    payment.PaidAt = DateTime.Now;
                    _uow.Payments.Update(payment);

                    bool alreadyEnrolled = _uow.Enrollments.Any(e =>
                        e.StudentId == payment.StudentId && e.CourseId == payment.CourseId);

                    if (!alreadyEnrolled)
                    {
                        _uow.Enrollments.Add(new Enrollment
                        {
                            StudentId = payment.StudentId,
                            CourseId = payment.CourseId,
                            EnrollDate = DateTime.Now,
                            Notes = $"Thanh toán PayOS #{orderCode}"
                        });
                    }

                    _uow.SaveChanges();
                    return View("Success", payment);
                }
            }
            catch { }

            TempData["Error"] = "Thanh toán chưa được xác nhận. Liên hệ hỗ trợ nếu đã bị trừ tiền.";
            return RedirectToAction("Index", "Course");
        }

        public async Task<IActionResult> Cancel(long orderCode)
        {
            var payment = _uow.Payments.GetByOrderCode(orderCode);
            if (payment != null && payment.Status == PaymentStatus.PENDING)
            {
                payment.Status = PaymentStatus.CANCELLED;
                _uow.Payments.Update(payment);
                _uow.SaveChanges();

                try { await _payOS.CancelPaymentAsync(orderCode, "Người dùng hủy"); } catch { }
            }

            TempData["Error"] = "Bạn đã hủy thanh toán";
            return RedirectToAction("Index", "Course");
        }
    }


}