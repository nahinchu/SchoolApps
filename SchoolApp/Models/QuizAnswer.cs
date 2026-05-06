using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models
{
    [Table("QuizAnswers")]
    public class QuizAnswer
    {
        [Key]
        [Display(Name = "Mã câu trả lời")]
        public int QuizAnswerId { get; set; }

        [Required]
        [Display(Name = "Là đáp án đúng")]
        public bool IsCorrect { get; set; } = false;

        // ── FK: thuộc lượt làm nào ──
        [Required]
        [Display(Name = "Lượt làm")]
        public int QuizAttemptId { get; set; }

        [ForeignKey(nameof(QuizAttemptId))]
        public virtual QuizAttempt Attempt { get; set; } = null!;

        // ── FK: câu hỏi nào ──
        [Required]
        [Display(Name = "Câu hỏi")]
        public int QuestionId { get; set; }

        [ForeignKey(nameof(QuestionId))]
        public virtual Question Question { get; set; } = null!;

        // ── FK: đáp án đã chọn ──
        [Required]
        [Display(Name = "Đáp án đã chọn")]
        public int SelectedOptionId { get; set; }

        [ForeignKey(nameof(SelectedOptionId))]
        public virtual AnswerOption SelectedOption { get; set; } = null!;
    }
}