using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models
{
    [Table("Enrollments")]
    public class Enrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        [Display(Name = "Học viên")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khóa học")]
        [Display(Name = "Khóa học")]
        public int CourseId { get; set; }

        [Display(Name = "Ngày đăng ký")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime EnrollDate { get; set; } = DateTime.Now;

        [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
        [Display(Name = "Điểm")]
        [Column(TypeName = "decimal(4,2)")]
        public decimal? Grade { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        // Navigation properties — EF tự động JOIN
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
    }
}
