# Idempotency

[![build](https://github.com/joaopaulopmedeiros/idempotency-dotnet/actions/workflows/build.yml/badge.svg)](https://github.com/joaopaulopmedeiros/idempotency-dotnet/actions/workflows/build.yml)

Idempotency is a lean library for implementing idempotent operations in .NET applications. It ensures safe retries, request deduplication, and consistent execution of APIs, background jobs, and message handlers.

It integrates with ASP.NET Core and supports multiple storage providers through a pluggable persistence model, making it suitable for high-reliability and distributed systems.


## Installation

Install the core package:

```bash
dotnet add package Idempotency
```

Install ASP.NET Core integration:

```bash
dotnet add package Idempotency.AspNetCore
```

Install a persistence provider:

```bash
dotnet add package Idempotency.Redis
```

```bash
dotnet add package Idempotency.PostgreSql
```

Other providers are also available (see below).

## Quick Example
Clients can provide an idempotency key using the `X-Idempotency-Key` header.  
Requests with the same key will only be executed once.

```bash
curl -X POST http://localhost:5000/orders \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: 8f3b2c1a-6f41-4a63-b5fa-3e5e0b3c7c21" \
  -d '{
        "productId": "123",
        "quantity": 1
      }'
```

If the same request is retried with the same `X-Idempotency-Key`, the previously stored response will be returned instead of executing the operation again.

Register Idempotency in your application:

When using provider-specific packages, import the corresponding namespace:

```csharp
using Idempotency.PostgreSql;
using Idempotency.Redis;
```

```csharp
builder.Services
    .AddIdempotency(options =>
    {
        options.HeaderName = "X-Idempotency-Key";
        options.Expiration = TimeSpan.FromHours(12);
    })
    .UsePostgreSql(options =>
    {
        options.ConnectionString = builder.Configuration.GetConnectionString("PostgreSql");
        options.Schema = "idempotency";
        options.TableName = "requests";
        options.EnableAutoCreateTable = true;
        options.UseAdvisoryLocks = true;
        options.CommandTimeout = TimeSpan.FromSeconds(15);
        options.CleanupBatchSize = 2_000;
    });
```

Or configure Redis with full control:

```csharp
builder.Services
    .AddIdempotency(options =>
    {
        options.HeaderName = "X-Idempotency-Key";
        options.Expiration = TimeSpan.FromMinutes(30);
    })
    .UseRedis(options =>
    {
        options.ConnectionString = builder.Configuration.GetConnectionString("Redis");
        options.Configuration = "localhost:6379,allowAdmin=true,ssl=false";
        options.InstanceName = "idempotency";
        options.Database = 2;
        options.KeyPrefix = "orders:";
        options.ConnectTimeout = TimeSpan.FromSeconds(3);
        options.SyncTimeout = TimeSpan.FromSeconds(2);
        options.AbortOnConnectFail = false;
    });
```

---

### ASP.NET Core Controllers

Use the `[Idempotent]` attribute to protect operations:

```csharp
[Idempotent]
[HttpPost("/orders")]
public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
{
    var order = await _service.CreateAsync(request);
    return Ok(order);
}
```

If the same request is retried with the same idempotency key, the previously stored result is returned instead of executing the operation again.

---

### Minimal APIs

Idempotency can also be applied to Minimal APIs:

```csharp
app.MapPost("/orders", async (CreateOrderRequest request, IOrderService service) =>
{
    var order = await service.CreateAsync(request);
    return Results.Ok(order);
})
.WithIdempotency();
```