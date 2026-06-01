using SchoolApp.Models;

namespace SchoolApp.Repositories.ReviewRepository
{
    public interface IReviewRepository : IRepository<CourseReview>
    {
        IQueryable<CourseReview> GetByCourse(int courseId);
        CourseReview? GetByStudentAndCourse(int studentId, int courseId);
        double GetAverageRating(int courseId);
    }
}
