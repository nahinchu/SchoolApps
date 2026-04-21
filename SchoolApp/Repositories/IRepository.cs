using System.Linq.Expressions;

namespace SchoolApp.Repositories
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        IQueryable<T> Find(Expression<Func<T, bool>> predicate);
        T GetById(int id);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        bool Any(Expression<Func<T, bool>> predicate);
    }
}
