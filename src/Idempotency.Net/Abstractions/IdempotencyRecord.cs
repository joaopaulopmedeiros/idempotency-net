namespace Idempotency.Net.Abstractions;

public sealed class IdempotencyRecord
{
    public string Key { get; init; } = default!;

    public int StatusCode { get; init; }

    public string? ResponseBody { get; init; }

    public string? ContentType { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }
}