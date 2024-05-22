using KanbamApi.Data;
using KanbamApi.Data.Seed;

namespace Kanbam.Test.Reset;

public class TestBase : IAsyncLifetime
{
    public TestBase()
    {
        string projectDir = Directory
            .GetParent(Directory.GetCurrentDirectory())
            ?.Parent?.Parent?.Parent?.FullName!;

        string envFilePath = Path.Combine(projectDir, "KanbamApi", ".env.test");

        if (File.Exists(envFilePath))
        {
            DotNetEnv.Env.Load(envFilePath);
        }
        else
        {
            DotNetEnv.Env.Load();
        }
    }

    public async Task InitializeAsync()
    {
        var kanbamDbContext = new KanbamDbContext();
        var mongoDbSeeder = new MongoDbSeeder(kanbamDbContext);
        await mongoDbSeeder.SeedAsync();
    }

    public Task DisposeAsync()
    {
        // Implement if you need any cleanup after all tests in a derived class run
        return Task.CompletedTask;
    }
}
