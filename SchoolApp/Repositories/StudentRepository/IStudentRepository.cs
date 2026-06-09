using SchoolApp.Models;

namespace SchoolApp.Repositories.StudentRepository
{
    public interface IUserRepository : IRepository<User>
    {
        IQueryable<User> Search(string keyword);
        User GetWithEnrollments(int id);
        User? GetByEmail(string email);
        User? GetByGoogleId(string googleId);
    }
}
