# Examples

This folder contains runnable ASP.NET Core samples showing how to integrate idempotency.

## Projects

- `DemoMinimalApi`: Minimal API sample.
- `DemoControllerApi`: ASP.NET Core Controllers sample using the `[Idempotent]` attribute.

Both projects support **Redis** and **PostgreSQL**. The provider is selected in `appsettings.json` using:

```json
"Idempotency": {
  "Provider": "Redis"
}
```

Valid values are `Redis` and `PostgreSql`.

## Run

From the repository root:

```bash
dotnet run --project examples/DemoMinimalApi/DemoMinimalApi.csproj
```

```bash
dotnet run --project examples/DemoControllerApi/DemoControllerApi.csproj
```

## Try idempotency

Use the same idempotency key in repeated requests.

```bash
curl -X POST http://localhost:5000/orders \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: demo-123" \
  -d '{"productId":"123","quantity":1}'
```

Run the same request again with the same `X-Idempotency-Key` value and the cached response will be returned.
