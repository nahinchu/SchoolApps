using SchoolApp.Models;

namespace SchoolApp.Repositories
{
    public interface IQuestionRepository : IRepository<Question>
    {
        IQueryable<Question> GetQuestionsByQuiz(int quizId);
    }
}