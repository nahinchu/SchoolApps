using SchoolApp.Data;
using SchoolApp.Models;
using SchoolApp.Repositories;
using SchoolApp.Repositories.CourseRepository;
using SchoolApp.Repositories.EnrollmentRepository;
using SchoolApp.Repositories.LessonRepository;
using SchoolApp.Repositories.ModuleRepository;
using SchoolApp.Repositories.StudentRepository;
using SchoolApp.Repositories.LearnRepository;   
using System.Reflection;

namespace SchoolApp.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public ICourseRepository Courses { get; private set; }
        public IStudentRepository Students { get; private set; }
        public IEnrollmentRepository Enrollments { get; private set; }
        public IModuleRepository Modules { get; private set; }
       public  ILessonRepository Lessons { get; private set; }
        public IQuizRepository Quizzes { get; private set; }
        public IQuestionRepository Questions { get; private set; }
        public IAnswerOptionRepository AnswerOptions { get; private set; }
        public ILessonProgressRepository LessonProgresses { get; private set; }
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Courses = new CourseRepository(context);
            Students = new StudentRepository(context);
            Enrollments = new EnrollmentRepository(context);
            Modules = new ModuleRepository(context);
            Lessons = new LessonRepository(context);
            Quizzes = new QuizRepository(context);
            Questions = new QuestionRepository(context);
            AnswerOptions = new AnswerOptionRepository(context);
            LessonProgresses = new LessonProgressRepository(context);
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
