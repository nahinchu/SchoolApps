using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SchoolApp.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendPaymentSuccessEmailAsync(string toEmail, string userName, string courseName, decimal amount, long orderCode, DateTime paidAt)
        {
            var smtp = _config.GetSection("Smtp");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtp["SenderName"]!, smtp["Username"]!));
            message.To.Add(new MailboxAddress(userName, toEmail));
            message.Subject = $"Thanh toán thành công cho khóa học {courseName}";
            message.Body = new BodyBuilder
            {
                HtmlBody = BuildPaymentHtmlBody(userName, courseName, amount, orderCode, paidAt)
            }.ToMessageBody();

            await SendAsync(message);
        }

        public async Task SendOtpEmailAsync(string toEmail, string userName, string otp, string purpose)
        {
            var smtp = _config.GetSection("Smtp");

            bool isRegister = purpose == "register";
            string subject = isRegister
                ? "Xác thực email đăng ký SchoolApp"
                : "Mã OTP đặt lại mật khẩu SchoolApp";

            string title = isRegister ? "Xác thực địa chỉ email" : "Đặt lại mật khẩu";
            string desc = isRegister
                ? "Bạn vừa đăng ký tài khoản SchoolApp. Nhập mã OTP bên dưới để xác thực email:"
                : "Chúng tôi nhận được yêu cầu đặt lại mật khẩu. Nhập mã OTP bên dưới để tiếp tục:";

            var html = $$"""
                <!DOCTYPE html><html lang="vi"><head><meta charset="utf-8"/>
                <style>
                body{font-family:Arial,sans-serif;background:#f4f6f8;margin:0;padding:0;}
                .card{max-width:520px;margin:40px auto;background:#fff;border-radius:12px;padding:36px;box-shadow:0 4px 24px rgba(0,0,0,.08);}
                .otp{display:inline-block;background:#1b2d42;color:#c9a84c;font-size:32px;font-weight:700;letter-spacing:8px;padding:14px 28px;border-radius:8px;margin:20px 0;}
                .note{font-size:13px;color:#888;margin-top:16px;}
                </style></head>
                <body><div class="card">
                <h2 style="color:#c9a84c;margin-top:0;">🔐 {{title}}</h2>
                <p>Xin chào <strong>{{(string.IsNullOrEmpty(userName) ? toEmail : userName)}}</strong>,</p>
                <p>{{desc}}</p>
                <div style="text-align:center;"><span class="otp">{{otp}}</span></div>
                <p class="note">⏱ Mã có hiệu lực trong <strong>10 phút</strong>.</p>
                <p class="note">Nếu bạn không thực hiện thao tác này, hãy bỏ qua email này.</p>
                </div></body></html>
                """;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtp["SenderName"]!, smtp["Username"]!));
            message.To.Add(new MailboxAddress(userName, toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = html }.ToMessageBody();

            await SendAsync(message);
        }

        private async Task SendAsync(MimeMessage message)
        {
            var smtp = _config.GetSection("Smtp");
            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(smtp["Host"]!, int.Parse(smtp["Port"]!), SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtp["Username"]!, smtp["Password"]!);
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", message.To.ToString());
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }

        private static string BuildPaymentHtmlBody(
          string userName,
          string courseName,
          decimal amount,
          long orderCode,
          DateTime paidAt)
        {
            return $$"""
            <!DOCTYPE html>
            <html lang="vi">
            <head>
              <meta charset="utf-8"/>
              <style>
                body { font-family: Arial, sans-serif; background:#f4f6f8; margin:0; padding:0; }
                .wrap { max-width:600px; margin:32px auto; background:#fff; border-radius:10px;
                        box-shadow:0 2px 12px rgba(0,0,0,.10); overflow:hidden; }
                .header { background:#22c55e; color:#fff; padding:36px 30px; text-align:center; }
                .header .icon { font-size:48px; line-height:1; }
                .header h1 { margin:12px 0 4px; font-size:22px; }
                .header p { margin:0; opacity:.9; font-size:14px; }
                .body { padding:30px; }
                .body p { color:#444; line-height:1.6; }
                table.details { width:100%; border-collapse:collapse; margin:20px 0; }
                table.details td { padding:11px 14px; font-size:14px; }
                table.details tr:nth-child(odd) td { background:#f9fafb; }
                table.details .label { color:#6b7280; width:45%; }
                table.details .value { color:#111827; font-weight:600; }
                .amount { color:#16a34a; font-size:18px; }
                .footer { background:#f9fafb; padding:20px 30px; text-align:center;
                          color:#9ca3af; font-size:12px; border-top:1px solid #e5e7eb; }
              </style>
            </head>
            <body>
              <div class="wrap">
                <div class="header">
                  <div class="icon">&#10003;</div>
                  <h1>Thanh toán thành công!</h1>
                  <p>Cảm ơn bạn đã đăng ký khóa học tại SchoolApp</p>
                </div>
                <div class="body">
                  <p>Xin chào <strong>{{userName}}</strong>,</p>
                  <p>Chúng tôi xác nhận thanh toán của bạn đã được xử lý thành công.</p>
                  <table class="details">
                    <tr>
                      <td class="label">Khóa học</td>
                      <td class="value">{{courseName}}</td>
                    </tr>
                    <tr>
                      <td class="label">Mã đơn hàng</td>
                      <td class="value">#{{orderCode}}</td>
                    </tr>
                    <tr>
                      <td class="label">Số tiền thanh toán</td>
                      <td class="value amount">{{amount:N0}} VNĐ</td>
                    </tr>
                    <tr>
                      <td class="label">Thời gian thanh toán</td>
                      <td class="value">{{paidAt:dd/MM/yyyy HH:mm}}</td>
                    </tr>
                    <tr>
                      <td class="label">Trạng thái</td>
                      <td class="value" style="color:#16a34a">&#10003; Đã thanh toán</td>
                    </tr>
                  </table>
                  <p>Bạn có thể bắt đầu học ngay bây giờ. Chúc bạn học tập hiệu quả!</p>
                </div>
                <div class="footer">
                  <p>Email này được gửi tự động. Vui lòng không trả lời trực tiếp.</p>
                  <p>&copy; {{DateTime.Now.Year}} SchoolApp. All rights reserved.</p>
                </div>
              </div>
            </body>
            </html>
            """;
        }
    }
}
