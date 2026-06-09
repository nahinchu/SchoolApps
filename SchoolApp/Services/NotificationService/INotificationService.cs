namespace SchoolApp.Services.NotificationService
{
    public interface INotificationService
    {
        void Create(int userId, string title, string message, string? link = null);
    }
}
