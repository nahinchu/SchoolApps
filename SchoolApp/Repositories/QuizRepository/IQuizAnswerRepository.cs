using SchoolApp.Models;

namespace SchoolApp.Repositories
{
    public interface IQuizAnswerRepository : IRepository<QuizAnswer>
    {
        IQueryable<QuizAnswer> GetByAttempt(int attemptId);


        bool HasAnyForOption(int answerOptionId);
    }
}