namespace SchoolApp.Repositories
{
    public interface ICourseRepository : IRepository<Models.Course>
    {
        IQueryable<Models.Course> SearchByName(string keyword);
    }
}
