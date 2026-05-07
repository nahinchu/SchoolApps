using SchoolApp.Models;

namespace SchoolApp.Repositories.CourseRepository
{
    public interface ICourseRepository : IRepository<Models.Course>
    {
        IQueryable<Models.Course> SearchByName(string keyword);
        Course? GetCourseWithFullTree(int courseId);
    }
}
