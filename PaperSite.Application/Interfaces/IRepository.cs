using PaperSite.Domain.Common;
using System.Linq.Expressions;

namespace PaperSite.Application.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    IQueryable<T> Query(bool asNoTracking = false);
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    void update(T entity);
    void Delete(T entity);
}
