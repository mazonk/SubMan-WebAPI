using SubMan.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;

namespace SubMan.Services;

public class MongoDBService {
    private readonly IMongoCollection<Subscription> _subscriptionCollection;

    public MongoDBService(IOptions<MongoDBSettings> mongoDBSettings) {
        Console.WriteLine($"Connecting to MongoDB)");
        MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
        IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _subscriptionCollection = database.GetCollection<Subscription>(mongoDBSettings.Value.CollectionName);
    }

    public async Task CreateAsync(Subscription subscription) {
        await _subscriptionCollection.InsertOneAsync(subscription);
        return;
    }

    public async Task<List<Subscription>> GetAsync() {
        return await _subscriptionCollection.Find(new BsonDocument()).ToListAsync();
    }

    public async Task UpdateAsync(string id, Subscription subscription) {
        FilterDefinition<Subscription> filter = Builders<Subscription>.Filter.Eq("Id", id);
        UpdateDefinition<Subscription> update = Builders<Subscription>.Update
            .Set("name", subscription.Name)
            .Set("comment", subscription.Comment)
            .Set("price", subscription.Price)
            .Set("currency", subscription.Currency)
            .Set("interval", subscription.Interval);
        
        await _subscriptionCollection.UpdateOneAsync(filter, update);
        return;
    }

    public async Task DeleteAsync(string id) {
        FilterDefinition<Subscription> filter = Builders<Subscription>.Filter.Eq("Id", id);
        await _subscriptionCollection.DeleteOneAsync(filter);
        return;
    }

}
