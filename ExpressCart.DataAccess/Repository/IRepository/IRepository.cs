using System.Linq.Expressions;

namespace ExpressCart.DataAccess.Repository.IRepository
{
	public interface IRepository <T> where T : class
	{ 
		IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter, params Expression<Func<T, object>>[] includeProperties);
		T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false);
        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T>entity);
    }
}