using Idempotency.Net.Abstractions;
using Idempotency.Net.Extensions;
using Idempotency.Net.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Idempotency.Net.Extensions;

public static class ServiceCollectionExtensions
{
    public static IdempotencyBuilder AddIdempotency(
        this IServiceCollection services,
        Action<IdempotencyOptions>? configure = null)
    {
        services.AddOptions<IdempotencyOptions>();

        if (configure != null)
            services.Configure(configure);

        services.TryAddScoped<IdempotencyService>();

        return new IdempotencyBuilder(services);
    }
}