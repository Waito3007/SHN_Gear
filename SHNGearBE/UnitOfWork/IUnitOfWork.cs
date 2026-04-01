using SHNGearBE.Data;

namespace SHNGearBE.UnitOfWork;

public interface IUnitOfWork
{
    ApplicationDbContext Context { get; }

    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    Task SaveAsync();
    void ClearChangeTracker();
}
