using SchoolApp.Models;

namespace SchoolApp.Repositories.StudentRepository 
{ 
    public interface IStudentRepository : IRepository<Student>
    {
        IQueryable<Student> Search(string keyword);
        Student GetWithEnrollments(int id);
        Student GetByEmailAndPassword(string email, string password);
        Student? GetByEmail(string email);
    }
}
