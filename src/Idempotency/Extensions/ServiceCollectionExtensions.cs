using Idempotency.Abstractions;
using Idempotency.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Idempotency.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdempotency(
        this IServiceCollection services,
        Action<IdempotencyOptions>? configure = null)
    {
        services.AddOptions<IdempotencyOptions>();

        if (configure != null)
            services.Configure(configure);

        services.AddScoped<IdempotencyService>();

        return services;
    }
}