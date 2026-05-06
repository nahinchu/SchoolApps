using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Repositories
{
    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        public StudentRepository(AppDbContext context) : base(context) { }

        public IQueryable<Student> Search(string keyword)
        {
            var query = _dbSet.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(s =>
                    s.FullName.Contains(keyword) ||
                    s.Email.Contains(keyword) ||
                    s.Phone.Contains(keyword));
            }
            return query.OrderByDescending(s => s.RegisteredDate);
        }

        public Student GetWithEnrollments(int id)
        {
            return _dbSet
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .FirstOrDefault(s => s.StudentId == id);
        }

        public Student GetByEmailAndPassword(string email, string password)
        {
            return _dbSet.FirstOrDefault(s => s.Email == email && s.Password == password);
        }

        public Student? GetByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return _context.Students
                .FirstOrDefault(s => s.Email.ToLower() == email.ToLower());
        }
    }
}
