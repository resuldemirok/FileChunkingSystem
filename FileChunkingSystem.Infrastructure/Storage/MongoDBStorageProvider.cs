using FileChunkingSystem.Domain.Enums;
using FileChunkingSystem.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FileChunkingSystem.Infrastructure.Storage;

public class MongoDBStorageProvider : IStorageProvider
{
    private string _collectionName = string.Empty;
    private IMongoCollection<BsonDocument>? _collection;
    private readonly ILogger<MongoDBStorageProvider> _logger;
    private readonly IMongoDatabase _database;
    private readonly string _collectionSuffix;

    public StorageProviderType ProviderType => StorageProviderType.MongoDB;

    public MongoDBStorageProvider(IConfiguration configuration, ILogger<MongoDBStorageProvider> logger)
    {
        var connectionString = configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
        var databaseName = configuration["Storage:MongoDB:DatabaseName"] ?? "filechunking_storage";
        _collectionSuffix = configuration["Storage:MongoDB:CollectionName"] ?? "chunks";
        
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
        _logger = logger;
    }

    public async Task<string> StoreChunkAsync(byte[] chunkData, string group, string key)
    {
        try
        {
            _collection = GetCollection(group);
            
            var document = new BsonDocument
            {
                ["_id"] = key,
                ["data"] = chunkData,
                ["createdAt"] = DateTime.UtcNow
            };

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

    private IMongoCollection<BsonDocument> GetCollection(string group)
    {
        if (_collection == null || _collectionName.Equals(string.Empty) || !_collectionName.Equals($"{group}_{_collectionSuffix}")) {
            _collectionName = $"{group}_{_collectionSuffix}";
            _collection = _database.GetCollection<BsonDocument>(_collectionName);
        }

        return _collection!;
    }
}
