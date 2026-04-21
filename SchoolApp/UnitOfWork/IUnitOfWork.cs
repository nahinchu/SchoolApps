using SchoolApp.Repositories;

namespace SchoolApp.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        ICourseRepository Courses { get; }
        IStudentRepository Students { get; }
        IEnrollmentRepository Enrollments { get; }
        int SaveChanges();
    }
}
