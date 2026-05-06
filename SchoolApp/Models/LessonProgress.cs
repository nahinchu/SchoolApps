using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SchoolApp.Models.Enums;

namespace SchoolApp.Models
{
    [Table("LessonProgresses")]
    [Index(nameof(StudentId), nameof(LessonId), IsUnique = true, Name = "IX_LessonProgress_Student_Lesson")]
    public class LessonProgress
    {
        [Key]
        [Display(Name = "Mã tiến độ")]
        public int LessonProgressId { get; set; }

        [Required]
        [Display(Name = "Trạng thái")]
        public ProgressStatus Status { get; set; } = ProgressStatus.NotStarted;

        [Required]
        [Range(0, 100, ErrorMessage = "Tiến độ phải từ 0 đến 100%")]
        [Display(Name = "Tiến độ (%)")]
        public int ProgressPercent { get; set; } = 0;

        [Display(Name = "Ngày hoàn thành")]
        [DataType(DataType.DateTime)]
        public DateTime? CompletedAt { get; set; }

        [Required]
        [Display(Name = "Lần truy cập cuối")]
        [DataType(DataType.DateTime)]
        public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;

        // ── FK: Student ──
        [Required]
        [Display(Name = "Học viên")]
        public int StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; } = null!;

        // ── FK: Lesson ──
        [Required]
        [Display(Name = "Bài học")]
        public int LessonId { get; set; }

        [ForeignKey(nameof(LessonId))]
        public virtual Lesson Lesson { get; set; } = null!;
    }
}