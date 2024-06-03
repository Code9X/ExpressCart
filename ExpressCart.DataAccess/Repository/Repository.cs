using System.Linq.Expressions;
using ExpressCart.DataAccess.Data;
using ExpressCart.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace ExpressCart.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
		private readonly ApplicationDbContext _db;
		internal DbSet<T> dbSet;
		public Repository(ApplicationDbContext db)
		{
			_db = db;
			this.dbSet = _db.Set<T>();

		}
		public void Add(T entity)
		{
			var entry = dbSet.Attach(entity);
			if (entry.State == EntityState.Detached)
			{
				dbSet.Add(entity);
			}
		}
        public T Get(Expression<Func<T, bool>> filter, string includeProperties = null, bool tracked = false)
        {
            IQueryable<T> query = tracked ? dbSet : dbSet.AsNoTracking();

            query = query.Where(filter);

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return query.FirstOrDefault();
        }
        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, params Expression<Func<T, object>>[] includeProperties)
		{
			IQueryable<T> query = dbSet;
			if (filter != null)
			{
				query = query.Where(filter);
			}
			if (includeProperties != null)
			{
				foreach (var includeProp in includeProperties)
				{
					query = query.Include(includeProp);
				}
			}
			return query.ToList();
		}
		public void Remove(T entity)
		{
			dbSet.Remove(entity);
		}
		public void RemoveRange(IEnumerable<T> entity)
		{
			dbSet.RemoveRange(entity);
		}
	}
}
