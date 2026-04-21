using SchoolApp.Models;

namespace SchoolApp.Repositories
{
    public interface IEnrollmentRepository : IRepository<Enrollment>
    {
        IQueryable<Enrollment> SearchWithDetails(string keyword);
        IQueryable<Enrollment> GetByStudent(int studentId);
        Enrollment GetWithDetails(int id);
    }
}
