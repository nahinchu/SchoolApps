using SchoolApp.Models;

namespace SchoolApp.Repositories
{
    public interface IAnswerOptionRepository : IRepository<AnswerOption>
    {
        IQueryable<AnswerOption> GetOptionsByQuestion(int questionId);
    }
}