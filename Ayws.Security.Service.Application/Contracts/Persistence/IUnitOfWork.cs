using Ayws.Security.Service.Domain.Entities.Common;

namespace Ayws.Security.Service.Application.Contracts.Persistence;

public interface IUnitOfWork : IAsyncDisposable
{
    IGenericRepository<T, TId> Repository<T, TId>()
        where T : BaseEntity<TId>
        where TId : struct;

    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
