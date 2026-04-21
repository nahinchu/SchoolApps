using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Repositories
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(AppDbContext context) : base(context) { }

        public IQueryable<Course> SearchByName(string keyword)
        {
            var query = _dbSet.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(c => c.CourseName.Contains(keyword));
            }
            return query.OrderByDescending(c => c.CreatedDate);
        }
    }
}
