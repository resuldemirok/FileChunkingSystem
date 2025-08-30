using FileChunkingSystem.Domain.Interfaces;
using FileChunkingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace FileChunkingSystem.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly MetadataDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _transaction;

    public UnitOfWork(MetadataDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<T> Repository<T>() where T : class
    {
        if (_repositories.ContainsKey(typeof(T)))
            return (IGenericRepository<T>)_repositories[typeof(T)];

        var repository = new GenericRepository<T>(_context);
        _repositories.Add(typeof(T), repository);
        return repository;
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
