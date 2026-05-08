using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolApp.Models.Enums;

namespace SchoolApp.Models
{
    [Table("Questions")]
    public class Question
    {
        [Key]
        [Display(Name = "Mã câu hỏi")]
        public int QuestionId { get; set; }

        [Required(ErrorMessage = "Nội dung câu hỏi là bắt buộc")]
        [StringLength(2000, MinimumLength = 5, ErrorMessage = "Nội dung câu hỏi phải từ 5 đến 2000 ký tự")]
        [Display(Name = "Nội dung câu hỏi")]
        public string Content { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Giải thích đáp án")]
        public string? Explanation { get; set; }
        [StringLength(100)]
        [Display(Name = "Tag / Chủ đề")]
        public string? Tag { get; set; }
        [Required(ErrorMessage = "Loại câu hỏi là bắt buộc")]
        [Display(Name = "Loại câu hỏi")]
        public QuestionType Type { get; set; } = QuestionType.SingleChoice;

        [Required]
        [Range(1, 20, ErrorMessage = "Điểm phải từ 1 đến 20")]
        [Display(Name = "Điểm")]
        public int Points { get; set; } = 1;

        [Required]
        [Range(0, 200, ErrorMessage = "Thứ tự phải từ 0 đến 200")]
        [Display(Name = "Thứ tự")]
        public int OrderIndex { get; set; }

        // ── FK: thuộc về Quiz nào ──
        [Display(Name = "Bài kiểm tra")]
        public int? QuizId { get; set; }

        [ForeignKey(nameof(QuizId))]
        public virtual Quiz? Quiz { get; set; }

        // ── Navigation ──
        public virtual ICollection<AnswerOption> Options { get; set; } = new List<AnswerOption>();
    }
}