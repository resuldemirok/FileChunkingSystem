using System.Linq.Expressions;

namespace FileChunkingSystem.Domain.Interfaces;

/// <summary>
/// Generic repository interface providing basic CRUD operations for entities
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IGenericRepository<T> where T : class
{
    /// <summary>
    /// Retrieves an entity by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the entity</param>
    /// <returns>The entity if found, otherwise null</returns>
    Task<T?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Retrieves all entities of type T
    /// </summary>
    /// <returns>A collection of all entities</returns>
    Task<IEnumerable<T>> GetAllAsync();
    
    /// <summary>
    /// Finds entities that match the specified predicate
    /// </summary>
    /// <param name="predicate">The search predicate</param>
    /// <returns>A collection of matching entities</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// Adds a new entity to the repository
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <returns>The added entity</returns>
    Task<T> AddAsync(T entity);
    
    /// <summary>
    /// Updates an existing entity in the repository
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <returns>The updated entity</returns>
    Task<T> UpdateAsync(T entity);
    
    /// <summary>
    /// Deletes an entity by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete</param>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Checks if an entity exists with the specified identifier
    /// </summary>
    /// <param name="id">The unique identifier to check</param>
    /// <returns>True if the entity exists, otherwise false</returns>
    Task<bool> ExistsAsync(Guid id);
}
