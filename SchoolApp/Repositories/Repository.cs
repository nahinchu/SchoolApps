using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using System.Linq.Expressions;

namespace SchoolApp.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual IQueryable<T> GetAll()
        {
            return _dbSet.AsQueryable();
        }

        public virtual IQueryable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }

        public virtual T GetById(int id)
        {
            return _dbSet.Find(id);
        }

        public virtual void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public virtual void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public virtual bool Any(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Any(predicate);
        }
    }
}
