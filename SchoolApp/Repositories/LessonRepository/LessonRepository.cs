using SchoolApp.Data;
using SchoolApp.Models;
using Microsoft.EntityFrameworkCore;

namespace SchoolApp.Repositories.LessonRepository
{
    public class LessonRepository : Repository<Lesson>, ILessonRepository
    {
        public LessonRepository(AppDbContext context) : base(context) { }

        public IQueryable<Lesson> GetLessonsByModule(int moduleId)
        {
            return _dbSet
                .Where(l => l.ModuleId == moduleId)
                .OrderBy(l => l.OrderIndex)
                .ThenBy(l => l.LessonId)
                .Include(l => l.Quiz);
        }

        public IQueryable<Lesson> SearchByTitle(string keyword, int? moduleId = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(l => l.Title.Contains(keyword));

            if (moduleId.HasValue)
                query = query.Where(l => l.ModuleId == moduleId.Value);

            return query.OrderBy(l => l.ModuleId).ThenBy(l => l.OrderIndex);
        }

        public bool ExistsInModule(int lessonId, int moduleId)
        {
            return _dbSet.Any(l => l.LessonId == lessonId && l.ModuleId == moduleId);
        }

        public int GetMaxOrderIndex(int moduleId)
        {
            return _dbSet
                .Where(l => l.ModuleId == moduleId)
                .Max(l => (int?)l.OrderIndex) ?? 0;
        }

        public Lesson? GetWithModule(int lessonId)
        {
            return _dbSet
                .Include(l => l.Module)
                .FirstOrDefault(l => l.LessonId == lessonId);
        }
    }
}