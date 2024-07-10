using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Cnbv.ConectaProcesos.Opiniones.Data.Interfaces
{
  public interface IRepository<T> where T : class
  {
    Task<IEnumerable<T>> GetAllAsync();

    Task<IEnumerable<T>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includeProperties);

    Task<IEnumerable<T>> GetFilteredAsync(Expression<Func<T, bool>> expression);

    Task<IEnumerable<T>> GetFilteredIncludingAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includeProperties);

    Task<T> GetByIdAsync(int id);

    Task<T> GetByIdIncludingAsync(int id, params Expression<Func<T, object>>[] includeProperties);

    Task AddAsync(T entity);

    Task UpdateAsync(T entity);

    Task DeleteAsync(T entity);

    Task<T> GetByConditionAsync(Expression<Func<T, bool>> expression);

    Task<T> GetByConditionIncludingAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includeProperties);

    Task<IDbContextTransaction> BeginTransactionAsync();
  }
}
