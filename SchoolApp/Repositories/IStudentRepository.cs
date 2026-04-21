namespace SchoolApp.Repositories
{
    public interface IStudentRepository : IRepository<Models.Student>
    {
        IQueryable<Models.Student> Search(string keyword);
        Models.Student GetWithEnrollments(int id);
        Models.Student GetByEmailAndPassword(string email, string password);
    }
}
