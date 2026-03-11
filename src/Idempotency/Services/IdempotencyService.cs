using Idempotency.Abstractions;

namespace Idempotency.Services;

public class IdempotencyService
{
    private readonly IdempotencyStore _store;

    public IdempotencyService(IdempotencyStore store)
    {
        _store = store;
    }

    public async Task<IdempotencyRecord?> GetAsync(string key)
    {
        return await _store.GetAsync(key);
    }

    public async Task SaveAsync(IdempotencyRecord record)
    {
        await _store.SaveAsync(record);
    }
}