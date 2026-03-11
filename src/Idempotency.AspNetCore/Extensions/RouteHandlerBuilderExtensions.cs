using System.Text.Json;

using Idempotency.Abstractions;
using Idempotency.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Idempotency.AspNetCore.Extensions;

public static class RouteHandlerBuilderExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static RouteHandlerBuilder WithIdempotency(this RouteHandlerBuilder builder)
    {
        builder.AddEndpointFilter(async (context, next) =>
        {
            var requestServices = context.HttpContext.RequestServices;
            var options = requestServices.GetRequiredService<IOptions<IdempotencyOptions>>().Value;

            if (!TryGetIdempotencyKey(context.HttpContext, options, out var key))
                return await next(context).ConfigureAwait(false);

            var service = requestServices.GetRequiredService<IdempotencyService>();
            var cancellationToken = context.HttpContext.RequestAborted;

            var cached = await service.GetAsync(key, cancellationToken).ConfigureAwait(false);
            if (cached is not null)
                return new CachedIdempotencyResult(cached);

            var result = await next(context).ConfigureAwait(false);
            var resultToPersist = ToRecord(key, result, options);

            if (resultToPersist is not null)
                await service.SaveAsync(resultToPersist, cancellationToken).ConfigureAwait(false);

            return result;
        });

        return builder;
    }

    private static bool TryGetIdempotencyKey(HttpContext httpContext, IdempotencyOptions options, out string key)
    {
        key = string.Empty;

        if (!httpContext.Request.Headers.TryGetValue(options.HeaderName, out var values))
            return false;

        var value = values.ToString().Trim();
        if (string.IsNullOrWhiteSpace(value))
            return false;

        key = value;
        return true;
    }

    private static IdempotencyRecord? ToRecord(string key, object? result, IdempotencyOptions options)
    {
        var createdAt = DateTimeOffset.UtcNow;
        var expiresAt = createdAt.Add(options.Expiration);

        if (result is null)
        {
            return new IdempotencyRecord
            {
                Key = key,
                StatusCode = StatusCodes.Status200OK,
                CreatedAt = createdAt,
                ExpiresAt = expiresAt,
            };
        }

        if (result is string text)
        {
            return new IdempotencyRecord
            {
                Key = key,
                StatusCode = StatusCodes.Status200OK,
                ResponseBody = text,
                ContentType = "text/plain; charset=utf-8",
                CreatedAt = createdAt,
                ExpiresAt = expiresAt,
            };
        }

        if (result is IValueHttpResult valueResult)
        {
            var statusCode = result is IStatusCodeHttpResult statusResult && statusResult.StatusCode is not null
                ? statusResult.StatusCode.Value
                : StatusCodes.Status200OK;

            var contentType = result is IContentTypeHttpResult contentTypeResult
                ? contentTypeResult.ContentType
                : "application/json; charset=utf-8";

            return new IdempotencyRecord
            {
                Key = key,
                StatusCode = statusCode,
                ResponseBody = JsonSerializer.Serialize(valueResult.Value, SerializerOptions),
                ContentType = contentType,
                CreatedAt = createdAt,
                ExpiresAt = expiresAt,
            };
        }

        if (result is IResult)
        {
            // Some result kinds (stream/file/push) are not safely serializable for replay.
            return null;
        }

        return new IdempotencyRecord
        {
            Key = key,
            StatusCode = StatusCodes.Status200OK,
            ResponseBody = JsonSerializer.Serialize(result, SerializerOptions),
            ContentType = "application/json; charset=utf-8",
            CreatedAt = createdAt,
            ExpiresAt = expiresAt,
        };
    }

    private sealed class CachedIdempotencyResult : IResult
    {
        private readonly IdempotencyRecord _cached;

        public CachedIdempotencyResult(IdempotencyRecord cached)
        {
            _cached = cached;
        }

        public async Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = _cached.StatusCode;

            if (!string.IsNullOrWhiteSpace(_cached.ContentType))
                httpContext.Response.ContentType = _cached.ContentType;

            if (!string.IsNullOrEmpty(_cached.ResponseBody))
                await httpContext.Response.WriteAsync(_cached.ResponseBody, httpContext.RequestAborted).ConfigureAwait(false);
        }
    }
}