using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models
{
    [Table("Quizzes")]
    public class Quiz
    {
        [Key]
        [Display(Name = "Mã bài kiểm tra")]
        public int QuizId { get; set; }

        [Required(ErrorMessage = "Tiêu đề bài kiểm tra là bắt buộc")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Tiêu đề phải từ 3 đến 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô tả / Hướng dẫn")]
        public string? Description { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Điểm đạt phải từ 0 đến 100%")]
        [Display(Name = "Điểm đạt (%)")]
        public int PassingScore { get; set; } = 70;

        [Range(0, 180, ErrorMessage = "Thời gian giới hạn phải từ 0 đến 180 phút (0 = không giới hạn)")]
        [Display(Name = "Giới hạn thời gian (phút)")]
        public int TimeLimitMinutes { get; set; } = 0;

        [Display(Name = "Cho phép làm lại")]
        public bool AllowRetry { get; set; } = true;

        [Range(0, 10, ErrorMessage = "Số lần làm lại tối đa từ 0 đến 10 (0 = không giới hạn)")]
        [Display(Name = "Số lần làm lại tối đa")]
        public int MaxAttempts { get; set; } = 0;

        [Display(Name = "Hiển thị đáp án sau khi nộp")]
        public bool ShowAnswersAfterSubmit { get; set; } = true;

        [Display(Name = "Trạng thái mở")]
        public bool IsPublished { get; set; } = false;

        // ── FK: thuộc về Lesson nào (1-1) ──
        [Required(ErrorMessage = "Bài học là bắt buộc")]
        [Display(Name = "Bài học")]
        public int LessonId { get; set; }

        [ForeignKey(nameof(LessonId))]
        public virtual Lesson Lesson { get; set; } = null!;

        // ── Navigation ──
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        public virtual ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
    }
}