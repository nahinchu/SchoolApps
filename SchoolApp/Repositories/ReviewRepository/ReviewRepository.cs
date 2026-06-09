using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Repositories.ReviewRepository
{
    public class ReviewRepository : Repository<CourseReview>, IReviewRepository
    {
        public ReviewRepository(AppDbContext context) : base(context) { }

        public IQueryable<CourseReview> GetByCourse(int courseId)
        {
            return _context.CourseReviews
                .Include(r => r.User)
                .Where(r => r.CourseId == courseId)
                .OrderByDescending(r => r.CreatedAt);
        }

        public CourseReview? GetByStudentAndCourse(int userId, int courseId)
        {
            return _context.CourseReviews
                .FirstOrDefault(r => r.UserId == userId && r.CourseId == courseId);
        }

        public double GetAverageRating(int courseId)
        {
            var reviews = _context.CourseReviews.Where(r => r.CourseId == courseId);
            if (!reviews.Any()) return 0;
            return reviews.Average(r => r.Rating);
        }
    }
}
