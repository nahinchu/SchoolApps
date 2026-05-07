using SchoolApp.Models;

namespace SchoolApp.Repositories
{
    public interface IQuizRepository : IRepository<Quiz>
    {
        Quiz? GetQuizWithQuestions(int quizId);
        IQueryable<Quiz> GetByLesson(int lessonId);
        bool HasQuiz(int lessonId);
    }
}