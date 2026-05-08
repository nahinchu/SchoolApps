using SchoolApp.Models;

namespace SchoolApp.Repositories
{
    public interface IQuestionRepository : IRepository<Question>
    {
        IQueryable<Question> GetQuestionsByQuiz(int quizId);

        // ✅ Mới: lấy 1 câu hỏi kèm Options
        Question? GetQuestionWithOptions(int questionId);

        // ✅ Mới: lấy max OrderIndex (nhẹ hơn việc Include cả Quiz)
        int GetMaxOrderIndex(int quizId);
        IEnumerable<Question> GetBankQuestions(string? tag = null);
    }
}