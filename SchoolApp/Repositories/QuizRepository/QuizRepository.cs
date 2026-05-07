using SchoolApp.Data;
using SchoolApp.Models;
using Microsoft.EntityFrameworkCore;

namespace SchoolApp.Repositories
{
    public class QuizRepository : Repository<Quiz>, IQuizRepository
    {
        public QuizRepository(AppDbContext context) : base(context) { }

        public Quiz? GetQuizWithQuestions(int quizId)
        {
            return _dbSet
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefault(q => q.QuizId == quizId);
        }

        public IQueryable<Quiz> GetByLesson(int lessonId)
        {
            return _dbSet
                .Where(q => q.LessonId == lessonId)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                .OrderBy(q => q.QuizId);
        }

        public bool HasQuiz(int lessonId)
        {
            return _dbSet.Any(q => q.LessonId == lessonId);
        }
    }
}