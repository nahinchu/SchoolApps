namespace SchoolApp.Services
{
    public interface IEmailService
    {
        Task SendPaymentSuccessEmailAsync(
            string toEmail,
            string studentName,
            string courseName,
            decimal amount,
            long orderCode,
            DateTime paidAt);
    }
}
