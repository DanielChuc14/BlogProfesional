using System.Linq.Expressions;
using BlogPlatform.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Persistence;

public class Repository<T>(AppDbContext context) : IRepository<T> where T : class
{
    private readonly DbSet<T> _set = context.Set<T>();

    public IQueryable<T> Query() => _set.AsQueryable();

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _set.FindAsync([id], ct);

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await _set.FirstOrDefaultAsync(predicate, ct);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await _set.Where(predicate).ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await _set.AddAsync(entity, ct);

    public void Update(T entity) =>
        _set.Update(entity);

    public void Remove(T entity) =>
        _set.Remove(entity);
}
