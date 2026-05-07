using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Repositories
{
    public class AnswerOptionRepository : Repository<AnswerOption>, IAnswerOptionRepository
    {
        public AnswerOptionRepository(AppDbContext context) : base(context) { }

        public IQueryable<AnswerOption> GetOptionsByQuestion(int questionId)
        {
            return _dbSet.Where(o => o.QuestionId == questionId).OrderBy(o => o.OrderIndex);
        }
    }
}