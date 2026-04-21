
using System.ComponentModel.DataAnnotations;
namespace SchoolApp.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu bắt buộc")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
