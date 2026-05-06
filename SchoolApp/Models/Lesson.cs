using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolApp.Models.Enums;

namespace SchoolApp.Models
{
    [Table("Lessons")]
    public class Lesson
    {
        [Key]
        [Display(Name = "Mã bài học")]
        public int LessonId { get; set; }

        [Required(ErrorMessage = "Tiêu đề bài học là bắt buộc")]
        [StringLength(300, MinimumLength = 3, ErrorMessage = "Tiêu đề phải từ 3 đến 300 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại bài học là bắt buộc")]
        [Display(Name = "Loại bài học")]
        public LessonType Type { get; set; } = LessonType.Text;

        [StringLength(500, ErrorMessage = "URL video không được vượt quá 500 ký tự")]
        [Url(ErrorMessage = "URL video không hợp lệ")]
        [Display(Name = "URL Video")]
        public string? VideoUrl { get; set; }

        [Display(Name = "Nội dung bài viết")]
        [DataType(DataType.Html)]
        public string? HtmlContent { get; set; }

        [StringLength(500)]
        [Display(Name = "File đính kèm")]
        public string? AttachmentPath { get; set; }

        [Range(0, 600, ErrorMessage = "Thời lượng phải từ 0 đến 600 phút")]
        [Display(Name = "Thời lượng (phút)")]
        public int DurationMinutes { get; set; }

        [Required]
        [Range(0, 200, ErrorMessage = "Thứ tự phải từ 0 đến 200")]
        [Display(Name = "Thứ tự")]
        public int OrderIndex { get; set; }

        [Display(Name = "Trạng thái mở")]
        public bool IsPublished { get; set; } = false;

        [Display(Name = "Ngày tạo")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── FK: thuộc về Module nào ──
        [Required(ErrorMessage = "Chương là bắt buộc")]
        [Display(Name = "Chương")]
        public int ModuleId { get; set; }

        [ForeignKey(nameof(ModuleId))]
        public virtual Module Module { get; set; } = null!;

        // ── Navigation ──
        public virtual Quiz? Quiz { get; set; }
        public virtual ICollection<LessonProgress> Progresses { get; set; } = new List<LessonProgress>();
    }
}