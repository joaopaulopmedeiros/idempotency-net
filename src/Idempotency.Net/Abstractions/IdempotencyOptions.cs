namespace Idempotency.Net.Abstractions;

public sealed class IdempotencyOptions
{
    public string HeaderName { get; set; } = "X-Idempotency-Key";

    public TimeSpan Expiration { get; set; } = TimeSpan.FromHours(24);
}