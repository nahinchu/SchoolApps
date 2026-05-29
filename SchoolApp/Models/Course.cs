using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models
{
    [Table("Courses")]
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Tên khóa học là bắt buộc")]
        [StringLength(200, MinimumLength = 3,
            ErrorMessage = "Tên khóa học phải từ 3 đến 200 ký tự")]
        [Display(Name = "Tên khóa học")]
        public string CourseName { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự")]
        [Display(Name = "Mô tả")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Số tín chỉ là bắt buộc")]
        [Range(1, 10, ErrorMessage = "Số tín chỉ phải từ 1 đến 10")]
        [Display(Name = "Số tín chỉ")]
        public int Credits { get; set; }

        [Required(ErrorMessage = "Học phí là bắt buộc")]
        [Range(0, 100000000, ErrorMessage = "Học phí phải từ 0 đến 100,000,000 VNĐ")]
        [Display(Name = "Học phí (VNĐ)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Fee { get; set; }

        [Display(Name = "Cấp độ")]
        public CourseLevel Level { get; set; } = CourseLevel.Basic;

        [Display(Name = "Đang mở")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Cập nhật lần cuối")]
        public DateTime? UpdatedDate { get; set; }

        [StringLength(500)]
        public string? ThumbnailUrl { get; set; }

        public virtual ICollection<Module> Modules { get; set; } = new List<Module>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new HashSet<Enrollment>();
    }
}