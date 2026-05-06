using SchoolApp.Repositories;

namespace SchoolApp.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        ICourseRepository Courses { get; }
        IStudentRepository Students { get; }
        IEnrollmentRepository Enrollments { get; }
        //IRepository<Module> Modules { get; }
        //IRepository<Lesson> Lessons { get; }
        //IRepository<Quiz> Quizzes { get; }
        //IRepository<Question> Questions { get; }
        //IRepository<AnswerOption> AnswerOptions { get; }
        //IRepository<LessonProgress> LessonProgresses { get; }
        //IRepository<QuizAttempt> QuizAttempts { get; }
        //IRepository<QuizAnswer> QuizAnswers { get; }
        int SaveChanges();
    }
}
