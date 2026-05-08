using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Repositories
{
    public class QuizAnswerRepository : Repository<QuizAnswer>, IQuizAnswerRepository
    {
        public QuizAnswerRepository(AppDbContext context) : base(context) { }

        public IQueryable<QuizAnswer> GetByAttempt(int attemptId)
        {
            return _dbSet.Where(a => a.QuizAttemptId == attemptId);
        }

        public bool HasAnyForOption(int answerOptionId)
        {
            return _dbSet.Any(a => a.SelectedOptionId == answerOptionId);
        }
    }
}