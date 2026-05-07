using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Repositories.LearnRepository
{
    public class LessonProgressRepository : Repository<LessonProgress>, ILessonProgressRepository
    {
        public LessonProgressRepository(AppDbContext context) : base(context) { }

        public LessonProgress? GetProgress(int studentId, int lessonId)
        {
            return _dbSet.FirstOrDefault(p => p.StudentId == studentId && p.LessonId == lessonId);
        }

        public IQueryable<LessonProgress> GetByStudentAndCourse(int studentId, int courseId)
        {
            return _dbSet
                .Include(p => p.Lesson)
                .Where(p => p.StudentId == studentId
                            && p.Lesson.Module.CourseId == courseId);
        }
    }
}
