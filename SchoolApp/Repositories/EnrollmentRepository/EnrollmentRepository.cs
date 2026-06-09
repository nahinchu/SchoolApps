using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Repositories.EnrollmentRepository
{
    public class EnrollmentRepository : Repository<Enrollment>, IEnrollmentRepository
    {
        public EnrollmentRepository(AppDbContext context) : base(context) { }

        public IQueryable<Enrollment> SearchWithDetails(string keyword)
        {
            var query = _dbSet
                .Include(e => e.User)
                .Include(e => e.Course)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(e =>
                    (e.User.FullName != null && e.User.FullName.Contains(keyword)) ||
                    e.User.Email.Contains(keyword) ||
                    e.Course.CourseName.Contains(keyword));
            }

            return query.OrderByDescending(e => e.EnrollDate);
        }

        public IQueryable<Enrollment> GetByStudent(int userId)
        {
            return _dbSet
                .Include(e => e.Course)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.EnrollDate);
        }

        public Enrollment GetWithDetails(int id)
        {
            return _dbSet
                .Include(e => e.User)
                .Include(e => e.Course)
                .FirstOrDefault(e => e.EnrollmentId == id);
        }

        public bool IsEnrolled(int userId, int courseId)
        {
            return _dbSet.Any(e => e.UserId == userId && e.CourseId == courseId);
        }
    }
}
