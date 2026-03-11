using System.Text.Json;

using Idempotency.Abstractions;

using Microsoft.Extensions.Options;

using StackExchange.Redis;

namespace Idempotency.Redis;

internal sealed class RedisIdempotencyStore : IdempotencyStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnectionMultiplexer _connection;
    private readonly RedisIdempotencyOptions _options;

    public RedisIdempotencyStore(
        IConnectionMultiplexer connection,
        IOptions<RedisIdempotencyOptions> options)
    {
        _connection = connection;
        _options = options.Value;
    }

    public async Task<IdempotencyRecord?> GetAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var db = _connection.GetDatabase(_options.Database);
        var payload = await db.StringGetAsync(BuildRedisKey(key)).ConfigureAwait(false);

        if (payload.IsNullOrEmpty)
            return null;

        var record = JsonSerializer.Deserialize<IdempotencyRecord>(payload.ToString(), SerializerOptions);

        if (record?.ExpiresAt is not null && record.ExpiresAt <= DateTimeOffset.UtcNow)
            return null;

        return record;
    }

    public async Task SaveAsync(
        IdempotencyRecord record,
        CancellationToken cancellationToken = default)
    {
        var db = _connection.GetDatabase(_options.Database);
        var payload = JsonSerializer.Serialize(record, SerializerOptions);

        TimeSpan? expiry = null;
        if (record.ExpiresAt is not null)
        {
            var remainingTtl = record.ExpiresAt.Value - DateTimeOffset.UtcNow;
            expiry = remainingTtl > TimeSpan.Zero ? remainingTtl : TimeSpan.Zero;
        }

        await db.StringSetAsync(
            BuildRedisKey(record.Key),
            payload,
            expiry).ConfigureAwait(false);
    }

    private string BuildRedisKey(string key)
    {
        if (string.IsNullOrWhiteSpace(_options.InstanceName))
            return string.Concat(_options.KeyPrefix, key);

        return string.Concat(_options.InstanceName, ":", _options.KeyPrefix, key);
    }
}