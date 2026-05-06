using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models
{
    [Table("AnswerOptions")]
    public class AnswerOption
    {
        [Key]
        [Display(Name = "Mã đáp án")]
        public int AnswerOptionId { get; set; }

        [Required(ErrorMessage = "Nội dung đáp án là bắt buộc")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Nội dung đáp án phải từ 1 đến 1000 ký tự")]
        [Display(Name = "Nội dung đáp án")]
        public string Content { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Là đáp án đúng")]
        public bool IsCorrect { get; set; } = false;

        [Range(0, 200)]
        [Display(Name = "Thứ tự")]
        public int OrderIndex { get; set; }

        // ── FK: thuộc về Question nào ──
        [Required(ErrorMessage = "Câu hỏi là bắt buộc")]
        [Display(Name = "Câu hỏi")]
        public int QuestionId { get; set; }

        [ForeignKey(nameof(QuestionId))]
        public virtual Question Question { get; set; } = null!;
    }
}