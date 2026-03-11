using Idempotency.Abstractions;

namespace Idempotency.Services;

public class IdempotencyService
{
    private readonly IdempotencyStore _store;

    public IdempotencyService(IdempotencyStore store)
    {
        _store = store;
    }

    public async Task<IdempotencyRecord?> GetAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        return await _store.GetAsync(key, cancellationToken);
    }

    public async Task SaveAsync(
        IdempotencyRecord record,
        CancellationToken cancellationToken = default)
    {
        await _store.SaveAsync(record, cancellationToken);
    }
}