using MongoDB.Driver;
using Subman.Models;
using Subman.Database;
using MongoDB.Bson;

namespace Subman.Repositories;

public class SubscriptionRepository : BaseRepository<Subscription> {
    public SubscriptionRepository(MongoDbContext dbContext) : base(dbContext, "subscriptions") {}
}