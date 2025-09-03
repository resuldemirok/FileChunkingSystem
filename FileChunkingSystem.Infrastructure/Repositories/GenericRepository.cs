using FileChunkingSystem.Domain.Interfaces;
using FileChunkingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FileChunkingSystem.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation providing basic CRUD operations for any entity type.
/// Uses Entity Framework Core with the metadata database context.
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly MetadataDbContext _context;
    private readonly DbSet<T> _dbSet;

    /// <summary>
    /// Initializes a new instance of the GenericRepository
    /// </summary>
    /// <param name="context">The metadata database context</param>
    public GenericRepository(MetadataDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// Gets an entity by its unique identifier
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <returns>The entity if found, null otherwise</returns>
    public async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

    /// <summary>
    /// Gets all entities of type T
    /// </summary>
    /// <returns>Collection of all entities</returns>
    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    /// <summary>
    /// Finds entities matching the specified predicate
    /// </summary>
    /// <param name="predicate">The search predicate</param>
    /// <returns>Collection of matching entities</returns>
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) => 
        await _dbSet.Where(predicate).ToListAsync();

    /// <summary>
    /// Adds a new entity to the repository
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <returns>The added entity</returns>
    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <returns>The updated entity</returns>
    public async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return await Task.FromResult(entity);
    }

    /// <summary>
    /// Deletes an entity by its identifier
    /// </summary>
    /// <param name="id">The entity identifier</param>
    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null) _dbSet.Remove(entity);
    }

    /// <summary>
    /// Checks if an entity exists by its identifier
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <returns>True if entity exists, false otherwise</returns>
    public async Task<bool> ExistsAsync(Guid id) => await _dbSet.FindAsync(id) != null;
}
