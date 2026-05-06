namespace SchoolApp.Services
{
    public interface IPasswordService
    {
        //Băm mật khẩu plaintext thành hash để lưu DB.
        string Hash(string plainPassword);

        //So sánh plaintext với hash (lúc đăng nhập).
        bool Verify(string plainPassword, string hashedPassword);

        //Kiểm tra xem 1 chuỗi đã là BCrypt hash chưa (để migrate dữ liệu cũ)
        bool IsHashed(string value);
    }
}
