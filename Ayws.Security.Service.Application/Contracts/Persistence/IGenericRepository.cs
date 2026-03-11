using Ayws.Security.Service.Domain.Entities.Common;
using System.Linq.Expressions;

namespace Ayws.Security.Service.Application.Contracts.Persistence;

public interface IGenericRepository<T, TId>
    where T : BaseEntity<TId>
    where TId : struct
{
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T?> GetByIdAsync(TId id, CancellationToken ct = default);
    IQueryable<T> GetAll();
    Task<List<T>> GetAllListAsync(CancellationToken ct = default);
    IQueryable<T> Where(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);
    Task DeleteAsync(TId id, CancellationToken ct = default);
}
