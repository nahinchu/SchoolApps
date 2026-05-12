using SchoolApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchoolApp.DTOs
{
    public class LessonSaveDto
    {
        public int LessonId { get; set; }

        [Required(ErrorMessage = "ModuleId là bắt buộc")]
        public int ModuleId { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? HtmlContent { get; set; }
        public string? VideoUrl { get; set; }
        public string? AttachmentPath { get; set; }

        public int OrderIndex { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsPublished { get; set; } = true;

        public LessonType Type { get; set; } = LessonType.Video;
        public IFormFile? AttachmentFile { get; set; }
        public bool RemoveAttachment { get; set; }
        public string VideoInputMode { get; set; } = "url";
        public IFormFile? VideoFile { get; set; }
        public bool RemoveVideo { get; set; }
    }
}

