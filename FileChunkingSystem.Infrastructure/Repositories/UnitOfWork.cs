using FileChunkingSystem.Domain.Interfaces;
using FileChunkingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace FileChunkingSystem.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation that manages repositories and database transactions.
/// Provides a single point of access to all repositories and ensures data consistency.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly MetadataDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _transaction;

    /// <summary>
    /// Initializes a new instance of the UnitOfWork
    /// </summary>
    /// <param name="context">The metadata database context</param>
    public UnitOfWork(MetadataDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets or creates a repository for the specified entity type
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <returns>The repository instance</returns>
    public IGenericRepository<T> Repository<T>() where T : class
    {
        // Return existing repository if already created
        if (_repositories.ContainsKey(typeof(T)))
            return (IGenericRepository<T>)_repositories[typeof(T)];

        // Create new repository and cache it
        var repository = new GenericRepository<T>(_context);
        _repositories.Add(typeof(T), repository);
        return repository;
    }

    /// <summary>
    /// Saves all changes made in this unit of work to the database
    /// </summary>
    /// <returns>The number of affected records</returns>
    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// Disposes the unit of work and releases resources
    /// </summary>
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
