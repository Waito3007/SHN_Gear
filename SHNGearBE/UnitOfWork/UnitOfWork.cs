using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SHNGearBE.Data;

namespace SHNGearBE.UnitOfWork;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _contextTransaction;
    private bool _dispose = false;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public ApplicationDbContext Context => _context;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_dispose)
        {
            if (disposing)
            {
                _contextTransaction?.Dispose();
                _context.Dispose();
            }
        }
        _dispose = true;
    }

    public async Task BeginTransactionAsync()
    {
        _contextTransaction = await _context.Database.BeginTransactionAsync();
        await _context.Database.ExecuteSqlRawAsync("SET TRANSACTION ISOLATION LEVEL SERIALIZABLE");
    }

    public async Task CommitAsync()
    {
        await _context.SaveChangesAsync();
        if (_contextTransaction != null)
        {
            await _contextTransaction.CommitAsync();
            await _contextTransaction.DisposeAsync();
            _contextTransaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_contextTransaction != null)
        {
            await _contextTransaction.RollbackAsync();
            await _contextTransaction.DisposeAsync();
            _contextTransaction = null;
        }
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void ClearChangeTracker()
    {
        _context.ChangeTracker.Clear();
    }
}