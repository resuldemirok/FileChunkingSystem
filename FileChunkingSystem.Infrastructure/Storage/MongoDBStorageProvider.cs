using FileChunkingSystem.Domain.Enums;
using FileChunkingSystem.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FileChunkingSystem.Infrastructure.Storage;

/// <summary>
/// MongoDB storage provider implementation for storing chunks in MongoDB collections.
/// Creates separate collections for each group and stores chunks as BSON documents.
/// </summary>
public class MongoDBStorageProvider : IStorageProvider
{
    private string _collectionName = string.Empty;
    private IMongoCollection<BsonDocument>? _collection;
    private readonly ILogger<MongoDBStorageProvider> _logger;
    private readonly IMongoDatabase _database;
    private readonly string _collectionSuffix;

    /// <summary>
    /// Gets the storage provider type
    /// </summary>
    public StorageProviderType ProviderType => StorageProviderType.MongoDB;

    /// <summary>
    /// Initializes a new instance of the MongoDBStorageProvider
    /// </summary>
    /// <param name="configuration">Application configuration</param>
    /// <param name="logger">Logger instance</param>
    public MongoDBStorageProvider(IConfiguration configuration, ILogger<MongoDBStorageProvider> logger)
    {
        var connectionString = configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
        var databaseName = configuration["Storage:MongoDB:DatabaseName"] ?? "filechunking_storage";
        _collectionSuffix = configuration["Storage:MongoDB:CollectionName"] ?? "chunks";
        
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
        _logger = logger;
    }

    /// <summary>
    /// Stores a chunk in MongoDB collection
    /// </summary>
    /// <param name="chunkData">The chunk data to store</param>
    /// <param name="group">The group name (used for collection naming)</param>
    /// <param name="key">The unique key for the chunk</param>
    /// <returns>The storage key</returns>
    public async Task<string> StoreChunkAsync(byte[] chunkData, string group, string key)
    {
        try
        {
            _collection = GetCollection(group);
            
            // Create BSON document with chunk data
            var document = new BsonDocument
            {
                ["_id"] = key,
                ["data"] = chunkData,
                ["createdAt"] = DateTime.UtcNow
            };

            // Upsert the document (insert or update if exists)
            await _collection.ReplaceOneAsync(
                Builders<BsonDocument>.Filter.Eq("_id", key),
                document,
                new ReplaceOptions { IsUpsert = true });

            _logger.LogInformation("Chunk stored successfully in MongoDB with key {Key}", key);
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing chunk in MongoDB with key {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a chunk from MongoDB collection
    /// </summary>
    /// <param name="group">The group name</param>
    /// <param name="key">The unique key for the chunk</param>
    /// <returns>The chunk data</returns>
    public async Task<byte[]> RetrieveChunkAsync(string group, string key)
    {
        try
        {
            _collection = GetCollection(group);
            
            var filter = Builders<BsonDocument>.Filter.Eq("_id", key);
            var document = await _collection.Find(filter).FirstOrDefaultAsync();
            
            if (document != null && document.Contains("data"))
            {
                return document["data"].AsByteArray;
            }
            throw new KeyNotFoundException($"Chunk not found with key: {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chunk from MongoDB with key {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Deletes a chunk from MongoDB collection
    /// </summary>
    /// <param name="group">The group name</param>
    /// <param name="key">The unique key for the chunk</param>
    /// <returns>True if deleted successfully, false if not found</returns>
    public async Task<bool> DeleteChunkAsync(string group, string key)
    {
        try
        {
            _collection = GetCollection(group);
            
            var filter = Builders<BsonDocument>.Filter.Eq("_id", key);
            var result = await _collection.DeleteOneAsync(filter);
            
            _logger.LogInformation("Chunk deleted from MongoDB: {Key}", key);
            return result.DeletedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chunk from MongoDB with key {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Gets or creates a MongoDB collection for the specified group
    /// </summary>
    /// <param name="group">The group name</param>
    /// <returns>The MongoDB collection</returns>
    private IMongoCollection<BsonDocument> GetCollection(string group)
    {
        // Cache collection instance if it's the same group
        if (_collection == null || _collectionName.Equals(string.Empty) || 
            !_collectionName.Equals($"{group}_{_collectionSuffix}"))
        {
            _collectionName = $"{group}_{_collectionSuffix}";
            _collection = _database.GetCollection<BsonDocument>(_collectionName);
            
            _logger.LogDebug("Using MongoDB collection: {CollectionName}", _collectionName);
        }
        
        return _collection;
    }
}
