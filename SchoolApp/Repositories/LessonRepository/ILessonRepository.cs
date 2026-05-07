using SchoolApp.Models;

namespace SchoolApp.Repositories.LessonRepository
{
    public interface ILessonRepository : IRepository<Lesson>
    {
        IQueryable<Lesson> GetLessonsByModule(int moduleId);
        IQueryable<Lesson> SearchByTitle(string keyword, int? moduleId = null);
        bool ExistsInModule(int lessonId, int moduleId);
        int GetMaxOrderIndex(int moduleId);
    }
}
