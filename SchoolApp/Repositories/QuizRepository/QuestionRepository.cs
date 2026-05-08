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

        public Question? GetQuestionWithOptions(int questionId)
        {
            return _dbSet
                .Include(q => q.Options)
                .FirstOrDefault(q => q.QuestionId == questionId);
        }

        public int GetMaxOrderIndex(int quizId)
        {
            var anyQ = _dbSet.Where(q => q.QuizId == quizId);
            if (!anyQ.Any()) return 0;
            return anyQ.Max(q => q.OrderIndex);
        }

        public IEnumerable<Question> GetBankQuestions(string? tag = null)
        {
            var q = _dbSet                     
                       .Include(x => x.Options)
                       .Where(x => x.QuizId == null)
                       .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tag))
                q = q.Where(x => x.Tag == tag);

            return q.OrderByDescending(x => x.QuestionId).ToList();
        }
    }
}