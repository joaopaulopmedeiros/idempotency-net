using Idempotency.Net.Abstractions;

using Microsoft.Extensions.DependencyInjection;

namespace Idempotency.Net.Extensions;

public sealed class IdempotencyBuilder
{
    public IServiceCollection Services { get; }

    internal IdempotencyBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IdempotencyBuilder Configure(Action<IdempotencyOptions> configure)
    {
        Services.Configure(configure);
        return this;
    }
}