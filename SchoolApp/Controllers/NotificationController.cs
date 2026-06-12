using Microsoft.AspNetCore.Mvc;
using SchoolApp.UnitOfWork;

namespace SchoolApp.Controllers
{
    public class NotificationController : Controller
    {
        private readonly IUnitOfWork _uow;

        public NotificationController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public IActionResult Index()
        {
            var studentId = HttpContext.Session.GetInt32("UserId");
            if (studentId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập";
                return RedirectToAction("Login", "Account");
            }

            var notifications = _uow.Notifications.GetByStudent(studentId.Value).ToList();
            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkRead(int id)
        {
            var studentId = HttpContext.Session.GetInt32("UserId");
            if (studentId == null)
                return Json(new { success = false });

            var notif = _uow.Notifications.GetById(id);
            if (notif == null || notif.UserId != studentId)
                return Json(new { success = false });

            notif.IsRead = true;
            _uow.SaveChanges();
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult UnreadCount()
        {
            var studentId = HttpContext.Session.GetInt32("UserId");
            if (studentId == null)
                return Json(new { count = 0 });

            int count = _uow.Notifications.GetUnreadCount(studentId.Value);
            return Json(new { count });
        }

        [HttpGet]
        public IActionResult GetDropdown()
        {
            var studentId = HttpContext.Session.GetInt32("UserId");
            if (studentId == null)
                return Json(new { items = Array.Empty<object>(), unread = 0 });

            var notifications = _uow.Notifications.GetByStudent(studentId.Value)
                .Take(8)
                .ToList();

            var items = notifications.Select(n => new
            {
                n.NotificationId,
                n.Title,
                n.Message,
                n.IsRead,
                n.Link,
                CreatedAt = n.CreatedAt.ToString("dd/MM/yyyy HH:mm")
            });

            int unread = notifications.Count(n => !n.IsRead);
            return Json(new { items, unread });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAllRead()
        {
            var studentId = HttpContext.Session.GetInt32("UserId");
            if (studentId == null)
                return Json(new { success = false });

            var unread = _uow.Notifications.Find(n => n.UserId == studentId && !n.IsRead).ToList();
            foreach (var n in unread) n.IsRead = true;
            _uow.SaveChanges();
            return Json(new { success = true });
        }
    }
}
