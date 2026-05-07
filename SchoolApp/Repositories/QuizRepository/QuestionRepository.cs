using SchoolApp.Data;
using SchoolApp.Models;
using Microsoft.EntityFrameworkCore;

namespace SchoolApp.Repositories
{
    public class QuestionRepository : Repository<Question>, IQuestionRepository
    {
        public QuestionRepository(AppDbContext context) : base(context) { }

        public IQueryable<Question> GetQuestionsByQuiz(int quizId)
        {
            return _dbSet
                .Where(q => q.QuizId == quizId)
                .Include(q => q.Options)
                .OrderBy(q => q.OrderIndex);
        }
    }
}