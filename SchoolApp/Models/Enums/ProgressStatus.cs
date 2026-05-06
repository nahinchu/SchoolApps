using System.ComponentModel.DataAnnotations;

namespace SchoolApp.Models.Enums
{
    public enum ProgressStatus
    {
        [Display(Name = "Chưa bắt đầu")]
        NotStarted = 0,

        [Display(Name = "Đang học")]
        InProgress = 1,

        [Display(Name = "Hoàn thành")]
        Completed = 2
    }
}