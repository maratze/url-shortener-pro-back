using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UrlShortenerPro.Infrastructure.Data;
using UrlShortenerPro.Infrastructure.Interfaces;

namespace UrlShortenerPro.Infrastructure.Repositories;

public class Repository<T>(AppDbContext dbContext) : IRepository<T>
    where T : class
{
    protected readonly DbSet<T> DbSet = dbContext.Set<T>();

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await DbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
    }

    public virtual Task UpdateAsync(T entity)
    {
        DbSet.Attach(entity);
        dbContext.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}