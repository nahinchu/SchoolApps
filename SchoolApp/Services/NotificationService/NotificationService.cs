using SchoolApp.Models;
using SchoolApp.UnitOfWork;

namespace SchoolApp.Services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;

        public NotificationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public void Create(int studentId, string title, string message, string? link = null)
        {
            var notification = new Notification
            {
                StudentId = studentId,
                Title = title,
                Message = message,
                Link = link,
                CreatedAt = DateTime.UtcNow
            };
            _uow.Notifications.Add(notification);
            _uow.SaveChanges();
        }
    }
}
