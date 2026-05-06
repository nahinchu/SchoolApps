using System.ComponentModel.DataAnnotations;

namespace SchoolApp.Models.Enums
{
    public enum QuestionType
    {
        [Display(Name = "Một đáp án")]
        SingleChoice = 0,

        [Display(Name = "Nhiều đáp án")]
        MultiChoice = 1,

        [Display(Name = "Đúng / Sai")]
        TrueFalse = 2
    }
}