using SchoolApp.Data;
using SchoolApp.Models;
using Microsoft.EntityFrameworkCore;

namespace SchoolApp.Repositories
{
    public class QuizAttemptRepository : Repository<QuizAttempt>, IQuizAttemptRepository
    {
        public QuizAttemptRepository(AppDbContext context) : base(context) { }

        public int GetAttemptCount(int quizId, int studentId)
        {
            return _dbSet.Count(a =>
                a.QuizId == quizId &&
                a.StudentId == studentId &&
                a.FinishedAt != null);  // chỉ đếm các lần đã nộp
        }

        public QuizAttempt? GetAttemptWithDetails(int attemptId)
        {
            return _dbSet
                .Include(a => a.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(q => q.Options)
                .Include(a => a.Answers)
                    .ThenInclude(ans => ans.SelectedOption)
                .Include(a => a.Answers)
                    .ThenInclude(ans => ans.Question)
                .FirstOrDefault(a => a.QuizAttemptId == attemptId);
        }

        public IQueryable<QuizAttempt> GetByStudentAndQuiz(int studentId, int quizId)
        {
            return _dbSet
                .Where(a => a.StudentId == studentId && a.QuizId == quizId)
                .OrderByDescending(a => a.StartedAt);
        }

        public QuizAttempt? GetBestAttempt(int studentId, int quizId)
        {
            return _dbSet
                .Where(a => a.StudentId == studentId
                            && a.QuizId == quizId
                            && a.FinishedAt != null)
                .OrderByDescending(a => a.Score)
                .FirstOrDefault();
        }
    }
}