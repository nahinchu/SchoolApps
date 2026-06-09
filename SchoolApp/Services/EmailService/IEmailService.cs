namespace SchoolApp.Services
{
    public interface IEmailService
    {
        Task SendPaymentSuccessEmailAsync(
            string toEmail,
            string userName,
            string courseName,
            decimal amount,
            long orderCode,
            DateTime paidAt);

        Task SendOtpEmailAsync(
            string toEmail,
            string userName,
            string otp,
            string purpose);
    }
}
