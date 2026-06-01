namespace SchoolApp.Services.NotificationService
{
    public interface INotificationService
    {
        void Create(int studentId, string title, string message, string? link = null);
    }
}
