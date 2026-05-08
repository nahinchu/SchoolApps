using SchoolApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchoolApp.DTOs
{
    public class QuizSaveDto
    {
        public int QuizId { get; set; }

        [Required]
        public int LessonId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(1, 100)]
        public int PassingScore { get; set; } = 70;

        public int TimeLimitMinutes { get; set; } = 0;

        public int MaxAttempts { get; set; } = 0;

        public bool IsPublished { get; set; } = true;

        public bool AllowRetry { get; set; } = true;

        public bool ShowAnswersAfterSubmit { get; set; } = true;
    }
    public class QuestionDto
    {
        public int QuestionId { get; set; }
        public int? QuizId { get; set; }        
        public string Content { get; set; } = string.Empty;
        public int Type { get; set; }
        public int Points { get; set; } = 1;
        public string? Explanation { get; set; }
        public string? Tag { get; set; }     
        public List<OptionDto> Options { get; set; } = new();
    }

    public class OptionDto
    {
        public int AnswerOptionId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    public class SubmitQuizDto
    {
        public int QuizId { get; set; }
        // Key = QuestionId, Value = List of selected AnswerOptionIds
        public Dictionary<int, List<int>> Answers { get; set; } = new();
    }
   public class ImportFromBankDto
{
    public int QuizId { get; set; }
    public List<int> BankQuestionIds { get; set; } = new();
}
}
