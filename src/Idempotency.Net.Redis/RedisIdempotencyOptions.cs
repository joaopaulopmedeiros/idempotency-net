namespace Idempotency.Net.Redis;

public sealed class RedisIdempotencyOptions
{
    public string? ConnectionString { get; set; }

    public string? Configuration { get; set; }

    public string? InstanceName { get; set; }

    public int Database { get; set; }

    public string KeyPrefix { get; set; } = "idempotency:";

    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

    public TimeSpan SyncTimeout { get; set; } = TimeSpan.FromSeconds(5);

    public bool AbortOnConnectFail { get; set; }
}