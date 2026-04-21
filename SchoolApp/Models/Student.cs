using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models
{
    [Table("Students")]
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(150)]
        [Display(Name = "Email")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, MinimumLength = 9,
            ErrorMessage = "Số điện thoại phải từ 9 đến 15 ký tự")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}",
                       ApplyFormatInEditMode = true)]
        public DateTime DateOfBirth { get; set; }

        [StringLength(300)]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Display(Name = "Ngày đăng ký")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime RegisteredDate { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<Enrollment> Enrollments { get; set; }
            = new HashSet<Enrollment>();
    }
}
