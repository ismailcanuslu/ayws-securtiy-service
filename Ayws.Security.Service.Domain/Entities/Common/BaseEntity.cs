namespace Ayws.Security.Service.Domain.Entities.Common;

public abstract class BaseEntity<T>
{
    public T Id { get; set; } = default!;
}
