using SchoolApp.Models;

namespace SchoolApp.Repositories.NotificationRepository
{
    public interface INotificationRepository : IRepository<Notification>
    {
        IQueryable<Notification> GetByStudent(int studentId);
        int GetUnreadCount(int studentId);
    }
}
