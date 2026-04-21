using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Repositories
{
    public class EnrollmentRepository : Repository<Enrollment>, IEnrollmentRepository
    {
        public EnrollmentRepository(AppDbContext context) : base(context) { }

        public IQueryable<Enrollment> SearchWithDetails(string keyword)
        {
            var query = _dbSet
                .Include(e => e.Student)
                .Include(e => e.Course)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(e =>
                    e.Student.FullName.Contains(keyword) ||
                    e.Course.CourseName.Contains(keyword));
            }

            return query.OrderByDescending(e => e.EnrollDate);
        }

        public IQueryable<Enrollment> GetByStudent(int studentId)
        {
            return _dbSet
                .Include(e => e.Course)
                .Where(e => e.StudentId == studentId)
                .OrderByDescending(e => e.EnrollDate);
        }

        public Enrollment GetWithDetails(int id)
        {
            return _dbSet
                .Include(e => e.Student)
                .Include(e => e.Course)
                .FirstOrDefault(e => e.EnrollmentId == id);
        }
    }
}
