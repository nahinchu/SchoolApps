using SchoolApp.Models;

namespace SchoolApp.Repositories
{
    public interface IQuizAttemptRepository : IRepository<QuizAttempt>
    {
        int GetAttemptCount(int quizId, int studentId);

        //Lấy 1 lượt làm bài kèm answers + options + question
        QuizAttempt? GetAttemptWithDetails(int attemptId);

        IQueryable<QuizAttempt> GetByStudentAndQuiz(int studentId, int quizId);

        QuizAttempt? GetBestAttempt(int studentId, int quizId);
    }
}