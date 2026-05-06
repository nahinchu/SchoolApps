using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models
{
    [Table("Modules")]
    public class Module
    {
        [Key]
        [Display(Name = "Mã chương")]
        public int ModuleId { get; set; }

        [Required(ErrorMessage = "Tên chương là bắt buộc")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Tên chương phải từ 3 đến 200 ký tự")]
        [Display(Name = "Tên chương")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Thứ tự phải từ 0 đến 100")]
        [Display(Name = "Thứ tự")]
        public int OrderIndex { get; set; }

        [Display(Name = "Trạng thái mở")]
        public bool IsPublished { get; set; } = false;

        // ── FK: thuộc về Course nào ──
        [Required(ErrorMessage = "Khóa học là bắt buộc")]
        [Display(Name = "Khóa học")]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public virtual Course Course { get; set; } = null!;

        // ── Navigation ──
        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}