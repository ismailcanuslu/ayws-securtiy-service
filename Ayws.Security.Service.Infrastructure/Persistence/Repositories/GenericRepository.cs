using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Domain.Entities.Common;
using Ayws.Security.Service.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Ayws.Security.Service.Infrastructure.Persistence.Repositories;

public class GenericRepository<T, TId>(AppDbContext context) : IGenericRepository<T, TId>
    where T : BaseEntity<TId>
    where TId : struct
{
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await DbSet.AnyAsync(predicate, ct);

    public async Task<T?> GetByIdAsync(TId id, CancellationToken ct = default)
        => await DbSet.FindAsync([id], ct);

    public IQueryable<T> GetAll()
        => DbSet.AsNoTracking();

    public async Task<List<T>> GetAllListAsync(CancellationToken ct = default)
        => await DbSet.AsNoTracking().ToListAsync(ct);

    public IQueryable<T> Where(Expression<Func<T, bool>> predicate)
        => DbSet.Where(predicate);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await DbSet.AddAsync(entity, ct);

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        => await DbSet.AddRangeAsync(entities, ct);

    public void Update(T entity)
        => DbSet.Update(entity);

    public void Delete(T entity)
        => DbSet.Remove(entity);

    public void DeleteRange(IEnumerable<T> entities)
        => DbSet.RemoveRange(entities);

    public async Task DeleteAsync(TId id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity is not null)
            DbSet.Remove(entity);
    }
}
