using Cnbv.ConectaProcesos.Opiniones.Data.DatabaseConectaProcesos;
using Cnbv.ConectaProcesos.Opiniones.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Cnbv.ConectaProcesos.Opiniones.Data.Implementations
{
  public class Repository<T> : IRepository<T> where T : class
  {
    private readonly ConectaProcesosContext _context;

    private readonly DbSet<T> _dbSet;

    public Repository(ConectaProcesosContext context)
    {
      _context = context;
      _dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
      return await _dbSet.ToListAsync();
    }

    public async Task<IEnumerable<T>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includeProperties)
    {
      IQueryable<T> query = _dbSet;
      foreach (var includeProperty in includeProperties)
      {
        query = query.Include(includeProperty);
      }

      return await query.ToListAsync();
    }

    public async Task<IEnumerable<T>> GetFilteredAsync(Expression<Func<T, bool>> expression)
    {
      return await _dbSet.Where(expression).ToListAsync();
    }

    public async Task<IEnumerable<T>> GetFilteredIncludingAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includeProperties)
    {
      IQueryable<T> query = _dbSet.Where(expression);
      foreach (var includeProperty in includeProperties)
      {
        query = query.Include(includeProperty);
      }

      return await query.ToListAsync();
    }

    public async Task<T> GetByIdAsync(int id)
    {
      return await _dbSet.FindAsync(id) ?? throw new InvalidOperationException();
    }

    public async Task<T> GetByIdIncludingAsync(int id, params Expression<Func<T, object>>[] includeProperties)
    {
      IQueryable<T> query = _dbSet;
      foreach (var includeProperty in includeProperties)
      {
        query = query.Include(includeProperty);
      }

      return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id) ?? throw new InvalidOperationException();
    }

    public async Task<T> GetByConditionAsync(Expression<Func<T, bool>> expression)
    {
      return await _dbSet.Where(expression).FirstOrDefaultAsync() ?? throw new InvalidOperationException();
    }

    public async Task<T> GetByConditionIncludingAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includeProperties)
    {
      IQueryable<T> query = _dbSet;
      foreach (var includeProperty in includeProperties)
      {
        query = query.Include(includeProperty);
      }

      return await query.FirstOrDefaultAsync(expression) ?? throw new InvalidOperationException();
    }

    public async Task AddAsync(T entity)
    {
      try
      {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
      }
      catch
      {
        throw new DbUpdateException("Ocurrió un error al guardar la información en la base de datos.");
      }
    }

    public async Task UpdateAsync(T entity)
    {
      try
      {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
      }
      catch
      {
        throw new DbUpdateException("Ocurrió un error al actualizar la información en la base de datos.");
      }
    }

    public async Task DeleteAsync(T entity)
    {
      try
      {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
      }
      catch
      {
        throw new DbUpdateException("Ocurrió un error al eliminar la información en la base de datos.");
      }
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
      return await _context.Database.BeginTransactionAsync();
    }
  }
}
