using SchoolApp.Models;
using System.Linq.Expressions;

namespace SchoolApp.Repositories.ModuleRepository
{
    public interface IModuleRepository : IRepository<Module>
    {
        IQueryable<Module> GetModulesByCourse(int courseId);
        IQueryable<Module> SearchByTitle(string keyword, int? courseId = null);
        bool ExistsInCourse(int moduleId, int courseId);
    }
}