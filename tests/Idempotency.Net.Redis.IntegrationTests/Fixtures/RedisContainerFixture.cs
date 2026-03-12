using System.Net;

using StackExchange.Redis;

using Testcontainers.Redis;

namespace Idempotency.Net.Redis.IntegrationTests.Fixtures;

public sealed class RedisContainerFixture : IAsyncLifetime
{
    private readonly RedisContainer _container;
    private IConnectionMultiplexer? _connection;

    public string ConnectionString { get; private set; } = string.Empty;

    public IConnectionMultiplexer Connection =>
        _connection ?? throw new InvalidOperationException("Redis connection is not initialized.");

    public RedisContainerFixture()
    {
        _container = new RedisBuilder("redis:7.2-alpine")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        ConnectionString = _container.GetConnectionString();

        ConfigurationOptions options = ConfigurationOptions.Parse(ConnectionString, true);
        options.AllowAdmin = true;
        options.AbortOnConnectFail = false;

        _connection = await ConnectionMultiplexer.ConnectAsync(options);
    }

    public async Task DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();

        await _container.DisposeAsync();
    }

    public async Task FlushDatabaseAsync(int database)
    {
        EndPoint endpoint = Connection.GetEndPoints().Single();
        IServer server = Connection.GetServer(endpoint);

        await server.FlushDatabaseAsync(database);
    }
}