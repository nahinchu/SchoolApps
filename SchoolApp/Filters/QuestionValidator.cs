using SchoolApp.DTOs;
using SchoolApp.Models.Enums;

namespace SchoolApp.Filters
{
    public static class QuestionValidator
    {
        public static string? Validate(QuestionDto dto)
        {
            if (dto == null) return "Dữ liệu không hợp lệ"; 

            if (string.IsNullOrWhiteSpace(dto.Content))
                return "Nội dung câu hỏi không được để trống";
            if (dto.Points < 1)
                return "Điểm phải >= 1";

            var validOptions = dto.Options?
                .Where(o => !string.IsNullOrWhiteSpace(o.Content))
                .ToList() ?? new List<OptionDto>();  

            if (validOptions.Count < 2)
                return "Cần ít nhất 2 đáp án có nội dung";
            if (!validOptions.Any(o => o.IsCorrect))
                return "Cần đánh dấu ít nhất 1 đáp án đúng";

            var type = (QuestionType)dto.Type;
            int correctCount = validOptions.Count(o => o.IsCorrect);

            if (type != QuestionType.MultiChoice && correctCount > 1)
                return "Loại câu hỏi này chỉ được có 1 đáp án đúng";
            if (type == QuestionType.TrueFalse && validOptions.Count != 2)
                return "Câu hỏi Đúng/Sai phải có đúng 2 đáp án";

            return null;
        }
    }
}
