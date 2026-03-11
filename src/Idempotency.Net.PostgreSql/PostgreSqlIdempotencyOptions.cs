namespace Idempotency.Net.PostgreSql;

public sealed class PostgreSqlIdempotencyOptions
{
    public string? ConnectionString { get; set; }

    public string Schema { get; set; } = "public";

    public string TableName { get; set; } = "idempotency_records";

    public bool EnableAutoCreateTable { get; set; } = true;

    public bool UseAdvisoryLocks { get; set; } = true;

    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(30);

    public int CleanupBatchSize { get; set; } = 1000;
}