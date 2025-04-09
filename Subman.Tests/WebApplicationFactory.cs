dusing Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Subman.Database;
using DotNetEnv;

namespace Subman.Tests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class {
    protected override IHost CreateHost(IHostBuilder builder) {
        // Load environment variables (including connection string)
        Env.Load("../.env");

        // Override DI setup
        builder.ConfigureServices(services => {
            // Remove the existing MongoDbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(MongoDbContext));
            if (descriptor is not null)
                services.Remove(descriptor);

            // Inject a test database instance using the same connection string
            var mongoConnStr = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");
            var testDbName = "SubManTest";

            services.AddSingleton<MongoDbContext>(sp =>
            {
                var mongoClient = new MongoDB.Driver.MongoClient(mongoConnStr);
                var testDb = mongoClient.GetDatabase(testDbName);
                return new MongoDbContext(testDb); // overload with IMongoDatabase
            });
        });

        return base.CreateHost(builder);
    }
}
