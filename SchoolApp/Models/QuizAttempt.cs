using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models
{
    [Table("QuizAttempts")]
    public class QuizAttempt
    {
        [Key]
        [Display(Name = "Mã lượt làm")]
        public int QuizAttemptId { get; set; }

        [Required]
        [Display(Name = "Bắt đầu lúc")]
        [DataType(DataType.DateTime)]
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Kết thúc lúc")]
        [DataType(DataType.DateTime)]
        public DateTime? FinishedAt { get; set; }

        [Required]
        [Range(0, 10000)]
        [Display(Name = "Điểm đạt được")]
        public int Score { get; set; } = 0;

        [Required]
        [Range(0, 10000)]
        [Display(Name = "Điểm tối đa")]
        public int MaxScore { get; set; } = 0;

        [Display(Name = "Phần trăm")]
        [NotMapped]
        public int ScorePercent => MaxScore > 0 ? (Score * 100 / MaxScore) : 0;

        [Required]
        [Display(Name = "Đạt")]
        public bool Passed { get; set; } = false;

        [Range(1, 100)]
        [Display(Name = "Lần thứ")]
        public int AttemptNumber { get; set; } = 1;

        // ── FK: Student ──
        [Required]
        [Display(Name = "Học viên")]
        public int StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; } = null!;

        // ── FK: Quiz ──
        [Required]
        [Display(Name = "Bài kiểm tra")]
        public int QuizId { get; set; }

        [ForeignKey(nameof(QuizId))]
        public virtual Quiz Quiz { get; set; } = null!;

        // ── Navigation ──
        public virtual ICollection<QuizAnswer> Answers { get; set; } = new List<QuizAnswer>();
    }
}