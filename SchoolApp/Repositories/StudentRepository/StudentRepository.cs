using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Repositories.StudentRepository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) { }

        public IQueryable<User> Search(string keyword)
        {
            var query = _dbSet.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(u =>
                    (u.FullName != null && u.FullName.Contains(keyword)) ||
                    u.Email.Contains(keyword) ||
                    (u.Phone != null && u.Phone.Contains(keyword)));
            }
            return query.OrderByDescending(u => u.RegisteredDate);
        }

        public User GetWithEnrollments(int id)
        {
            return _dbSet
                .Include(u => u.Enrollments)
                    .ThenInclude(e => e.Course)
                .FirstOrDefault(u => u.UserId == id);
        }

        public User? GetByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return _context.Users
                .FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
        }

        public User? GetByGoogleId(string googleId)
        {
            return _dbSet.FirstOrDefault(u => u.GoogleId == googleId);
        }
    }
}
