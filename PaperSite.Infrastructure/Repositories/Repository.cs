using Microsoft.EntityFrameworkCore;
using PaperSite.Application.Interfaces;
using PaperSite.Domain.Common;
using PaperSite.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace PaperSite.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(ApplicationDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public IQueryable<T> Query(bool asNoTracking = false)
    {
        return asNoTracking ? DbSet.AsNoTracking() : DbSet;
    }

    public async Task AddAsync(T entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await DbSet.AddAsync(entity);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await DbSet.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.AsNoTracking().Where(predicate).ToListAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.FirstOrDefaultAsync(predicate);
    }

    public void Update(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        DbSet.Update(entity);
    }

    public void update(T entity)
    {
        Update(entity);
    }

    public void Delete(T entity)
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        DbSet.Update(entity);
    }
}
