using SchoolApp.Data;
using SchoolApp.Models;
using Microsoft.EntityFrameworkCore;

namespace SchoolApp.Repositories.ModuleRepository
{
    public class ModuleRepository : Repository<Module>, IModuleRepository
    {
        public ModuleRepository(AppDbContext context) : base(context) { }

        public IQueryable<Module> GetModulesByCourse(int courseId)
        {
            return _dbSet
                .Where(m => m.CourseId == courseId)
                .OrderBy(m => m.OrderIndex)
                .ThenBy(m => m.ModuleId)
                .Include(m => m.Lessons);
        }

        public IQueryable<Module> SearchByTitle(string keyword, int? courseId = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(m => m.Title.Contains(keyword));

            if (courseId.HasValue)
                query = query.Where(m => m.CourseId == courseId.Value);

            return query
                .OrderBy(m => m.CourseId)
                .ThenBy(m => m.OrderIndex);
        }

        public bool ExistsInCourse(int moduleId, int courseId)
        {
            return _dbSet.Any(m => m.ModuleId == moduleId && m.CourseId == courseId);
        }
    }
}