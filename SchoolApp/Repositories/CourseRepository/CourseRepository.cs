using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Repositories.CourseRepository
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
        public Course? GetCourseWithFullTree(int courseId)
        {
            return _dbSet
                .Include(c => c.Modules.OrderBy(m => m.OrderIndex))
                    .ThenInclude(m => m.Lessons.OrderBy(l => l.OrderIndex))
                        .ThenInclude(l => l.Quiz)
                .FirstOrDefault(c => c.CourseId == courseId);
        }
    }
}
