namespace SchoolApp.Services
{
    public class BCryptPasswordService : IPasswordService
    {
        // WorkFactor 11–12 là cân bằng tốt giữa bảo mật và hiệu năng.
        // Mỗi lần +1 = chậm gấp đôi. 12 ≈ 250ms trên CPU desktop trung bình.
        private const int WorkFactor = 12;

        public string Hash(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
                throw new ArgumentException("Mật khẩu không được rỗng", nameof(plainPassword));

            return BCrypt.Net.BCrypt.HashPassword(plainPassword, WorkFactor);
        }

        public bool Verify(string plainPassword, string hashedPassword)
        {
            if (string.IsNullOrEmpty(plainPassword) || string.IsNullOrEmpty(hashedPassword))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // hash không hợp lệ (ví dụ password cũ chưa migrate)
                return false;
            }
        }

        public bool IsHashed(string value)
        {
            // BCrypt hash luôn bắt đầu bằng $2a$, $2b$, $2x$, hoặc $2y$ và dài 60 ký tự
            return !string.IsNullOrEmpty(value)
                && value.Length == 60
                && (value.StartsWith("$2a$") || value.StartsWith("$2b$")
                    || value.StartsWith("$2x$") || value.StartsWith("$2y$"));
        }
    }
}
