using MongoDB.Driver;
using Subman.Models;
using Subman.Database;
using MongoDB.Bson;

namespace Subman.Repositories;

public class SubscriptionRepository : BaseRepository<Subscription> {
    public SubscriptionRepository(MongoDbContext dbContext) : base(dbContext, "subscriptions") {}

    public async Task<IEnumerable<Subscription>> GetAllByUserIdAsync(string userId) {
        var filter = Builders<Subscription>.Filter.Eq("userId", userId);
        return await getCollection().Find(filter).ToListAsync();
    }
}