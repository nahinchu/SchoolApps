using SchoolApp.Repositories;
using SchoolApp.Repositories.CourseRepository;
using SchoolApp.Repositories.EnrollmentRepository;
using SchoolApp.Repositories.LessonRepository;
using SchoolApp.Repositories.ModuleRepository;
using SchoolApp.Repositories.StudentRepository;
using SchoolApp.Repositories.LearnRepository;
using SchoolApp.Repositories.PaymentRepository;

namespace SchoolApp.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        ICourseRepository Courses { get; }
        IStudentRepository Students { get; }
        IEnrollmentRepository Enrollments { get; }
        IModuleRepository Modules { get; }
        ILessonRepository Lessons { get; }
        IQuizRepository Quizzes { get; }
        IQuestionRepository Questions { get; }
        IAnswerOptionRepository AnswerOptions { get; }
        ILessonProgressRepository LessonProgresses { get; }
        IQuizAttemptRepository QuizAttempts { get; }
        IQuizAnswerRepository  QuizAnswers  { get; }
        IPaymentRepository Payments { get; }
        int SaveChanges();
    }
}
