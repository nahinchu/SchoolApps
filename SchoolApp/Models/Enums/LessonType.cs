using System.ComponentModel.DataAnnotations;

namespace SchoolApp.Models.Enums
{
    public enum LessonType
    {
        [Display(Name = "Video")]
        Video = 0,

        [Display(Name = "Bài viết")]
        Text = 1,

        [Display(Name = "Kết hợp")]
        Mixed = 2
    }
}