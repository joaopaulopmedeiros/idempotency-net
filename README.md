# Idempotency

[![build](https://github.com/joaopaulopmedeiros/idempotency-dotnet/actions/workflows/build.yml/badge.svg)](https://github.com/joaopaulopmedeiros/idempotency-dotnet/actions/workflows/build.yml)

Idempotency is a lean library for implementing idempotent operations in .NET applications. It ensures safe retries, request deduplication, and consistent execution of APIs, background jobs and message handlers.

It integrates with ASP.NET Core and supports multiple storage providers through a pluggable persistence model, making it suitable for high-reliability and distributed systems.

## Installation

Install the core package:

```bash
dotnet add package Idempotency
```

For ASP.NET Core support:
```bash
dotnet add package Idempotency.AspNetCore
```

## Quick Example
Register Idempotency in your application:
```csharp
builder.Services.AddIdempotency(options =>
{
    options.UseInMemoryStorage();
});
```

Use the [Idempotent] attribute to protect operations:
```csharp
[Idempotent]
[HttpPost("/orders")]
public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
{
    var order = await _service.CreateAsync(request);
    return Ok(order);
}
```

## Storage Providers

Idempotency supports multiple persistence providers.
| Provider   | Package               |
| ---------- | --------------------- |
| InMemory   | Idempotency           |
| Redis      | Idempotency.Redis     |
| PostgreSQL | Idempotency.Postgres  |
