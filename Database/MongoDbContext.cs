using MongoDB.Driver;

namespace Subman.Database;

public class MongoDbContext {
    public IMongoDatabase Database { get; }

    // init connection
    public MongoDbContext(string connectionString) {
        var client = new MongoClient(connectionString);
        Database = client.GetDatabase("SubMan");
    }
}