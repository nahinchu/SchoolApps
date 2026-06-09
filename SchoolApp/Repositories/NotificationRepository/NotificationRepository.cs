using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Repositories.NotificationRepository
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context) { }

        public IQueryable<Notification> GetByStudent(int userId)
        {
            return _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);
        }

        public int GetUnreadCount(int userId)
        {
            return _context.Notifications.Count(n => n.UserId == userId && !n.IsRead);
        }
    }
}
