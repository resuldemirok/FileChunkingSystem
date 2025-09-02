namespace FileChunkingSystem.Domain.Interfaces;

/// <summary>
/// Defines a unit of work pattern for managing database transactions and repository access
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets a repository instance for the specified entity type
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <returns>A repository instance for the specified type</returns>
    IGenericRepository<T> Repository<T>() where T : class;
    
    /// <summary>
    /// Saves all pending changes to the database
    /// </summary>
    /// <returns>The number of affected records</returns>
    Task<int> SaveChangesAsync();
    
    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    Task BeginTransactionAsync();
    
    /// <summary>
    /// Commits the current database transaction
    /// </summary>
    Task CommitTransactionAsync();
    
    /// <summary>
    /// Rolls back the current database transaction
    /// </summary>
    Task RollbackTransactionAsync();
}
