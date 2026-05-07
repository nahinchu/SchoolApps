using SchoolApp.Models;

namespace SchoolApp.Repositories.LearnRepository
{
    public interface ILessonProgressRepository : IRepository<LessonProgress>
    {
        LessonProgress? GetProgress(int studentId, int lessonId);
        IQueryable<LessonProgress> GetByStudentAndCourse(int studentId, int courseId);
    }
}
