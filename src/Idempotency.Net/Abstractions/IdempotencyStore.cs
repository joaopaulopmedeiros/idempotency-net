namespace Idempotency.Net.Abstractions;

public interface IdempotencyStore
{
    Task<IdempotencyRecord?> GetAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        IdempotencyRecord record,
        CancellationToken cancellationToken = default);
}