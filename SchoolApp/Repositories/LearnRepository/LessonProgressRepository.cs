using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Repositories.LearnRepository
{
    public class LessonProgressRepository : Repository<LessonProgress>, ILessonProgressRepository
    {
        public LessonProgressRepository(AppDbContext context) : base(context) { }

        public LessonProgress? GetProgress(int userId, int lessonId)
        {
            return _dbSet.FirstOrDefault(p => p.UserId == userId && p.LessonId == lessonId);
        }

        public IQueryable<LessonProgress> GetByStudentAndCourse(int userId, int courseId)
        {
            return _dbSet
                .Include(p => p.Lesson)
                .Where(p => p.UserId == userId
                            && p.Lesson.Module.CourseId == courseId);
        }
    }
}
