using SchoolApp.Models;
using SchoolApp.Models.Enums;
using SchoolApp.UnitOfWork;

namespace SchoolApp.Services.CompletionService
{
    public class CourseCompletionService : ICourseCompletionService
    {
        private readonly IUnitOfWork _uow;

        public CourseCompletionService(IUnitOfWork uow) => _uow = uow;

        public bool TryComplete(int studentId, int courseId)
        {
            var enrollment = _uow.Enrollments
                .Find(e => e.StudentId == studentId && e.CourseId == courseId)
                .FirstOrDefault();

            if (enrollment == null || enrollment.IsCompleted)
                return false;

            var course = _uow.Courses.GetCourseWithFullTree(courseId);
            if (course == null) return false;

            var allLessons = course.Modules
                .Where(m => m.IsPublished)
                .SelectMany(m => m.Lessons.Where(l => l.IsPublished))
                .ToList();

            if (!allLessons.Any()) return false;

            var completedLessonIds = _uow.LessonProgresses
                .GetByStudentAndCourse(studentId, courseId)
                .Where(p => p.Status == ProgressStatus.Completed)
                .Select(p => p.LessonId)
                .ToHashSet();

            foreach (var lesson in allLessons)
            {
                if (!completedLessonIds.Contains(lesson.LessonId))
                    return false;

                if (lesson.Quiz is { IsPublished: true })
                {
                    var best = _uow.QuizAttempts.GetBestAttempt(studentId, lesson.Quiz.QuizId);
                    if (best == null || !best.Passed)
                        return false;
                }
            }

            enrollment.IsCompleted = true;
            enrollment.CompletedAt = DateTime.UtcNow;
            _uow.SaveChanges();
            return true;
        }
    }
}
