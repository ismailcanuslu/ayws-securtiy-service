using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Domain.Entities.Common;
using Ayws.Security.Service.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ayws.Security.Service.Infrastructure.Persistence.Repositories;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly Dictionary<string, object> _repositories = new();
    private IDbContextTransaction? _transaction;

    public IGenericRepository<T, TId> Repository<T, TId>()
        where T : BaseEntity<TId>
        where TId : struct
    {
        var key = $"{typeof(T).Name}_{typeof(TId).Name}";
        if (!_repositories.TryGetValue(key, out var repo))
        {
            repo = new GenericRepository<T, TId>(context);
            _repositories[key] = repo;
        }
        return (IGenericRepository<T, TId>)repo;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await context.Database.BeginTransactionAsync(ct);

    public async Task CommitAsync(CancellationToken ct = default)
    {
        await context.SaveChangesAsync(ct);
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
            await _transaction.DisposeAsync();

        await context.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
