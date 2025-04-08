using MongoDB.Driver;

namespace Subman.Database;

public class MongoDbContext {
    public IMongoDatabase Database { get; }

    // main constructor init connection
    public MongoDbContext(string connectionString) {
        var client = new MongoClient(connectionString);
        Database = client.GetDatabase("SubMan");
    }

    // Test-only constructor overload
    public MongoDbContext(IMongoDatabase db)
    {
        Database = db;
    }
}