using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Repositories.NotificationRepository
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context) { }

        public IQueryable<Notification> GetByStudent(int studentId)
        {
            return _context.Notifications
                .Where(n => n.StudentId == studentId)
                .OrderByDescending(n => n.CreatedAt);
        }

        public int GetUnreadCount(int studentId)
        {
            return _context.Notifications.Count(n => n.StudentId == studentId && !n.IsRead);
        }
    }
}
