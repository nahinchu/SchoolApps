using System.ComponentModel.DataAnnotations;

namespace SchoolApp.DTOs
{
    public class ModuleSaveDto
    {
        public int ModuleId { get; set; }

        [Required(ErrorMessage = "CourseId là bắt buộc")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int OrderIndex { get; set; }

        public bool IsPublished { get; set; } = true;
    }
}
