using SchoolApp.Data;
using SchoolApp.Models;
using SchoolApp.Repositories;

namespace SchoolApp.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public ICourseRepository Courses { get; private set; }
        public IStudentRepository Students { get; private set; }
        public IEnrollmentRepository Enrollments { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Courses = new CourseRepository(context);
            Students = new StudentRepository(context);
            Enrollments = new EnrollmentRepository(context);
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
