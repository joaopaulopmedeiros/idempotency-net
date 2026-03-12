using Npgsql;

using Testcontainers.PostgreSql;

namespace Idempotency.Net.PostgreSql.IntegrationTests.Fixtures;

public sealed class PostgreSqlContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public string ConnectionString { get; private set; } = string.Empty;

    public PostgreSqlContainerFixture()
    {
        _container = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("idempotencytests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public async Task DropSchemaIfExistsAsync(
        string schema,
        CancellationToken cancellationToken = default)
    {
        await using NpgsqlConnection connection = await OpenConnectionAsync(cancellationToken);
        string quotedSchema = QuoteIdentifier(schema);

        string sql = $"DROP SCHEMA IF EXISTS {quotedSchema} CASCADE;";

        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RecreateSchemaAsync(
        string schema,
        CancellationToken cancellationToken = default)
    {
        await using NpgsqlConnection connection = await OpenConnectionAsync(cancellationToken);
        string quotedSchema = QuoteIdentifier(schema);

        string sql = $"""
            DROP SCHEMA IF EXISTS {quotedSchema} CASCADE;
            CREATE SCHEMA {quotedSchema};
            """;

        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> TableExistsAsync(
        string schema,
        string tableName,
        CancellationToken cancellationToken = default)
    {
        await using NpgsqlConnection connection = await OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = @schema
                  AND table_name = @table_name
            );
            """;

        await using NpgsqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("schema", schema);
        command.Parameters.AddWithValue("table_name", tableName);

        object? scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar is true;
    }

    public async Task InsertRawRecordAsync(
        string schema,
        string tableName,
        string key,
        int statusCode,
        string? responseBody,
        string? contentType,
        DateTimeOffset createdAt,
        DateTimeOffset? expiresAt,
        CancellationToken cancellationToken = default)
    {
        await using NpgsqlConnection connection = await OpenConnectionAsync(cancellationToken);

        string sql = $"""
            INSERT INTO {GetQualifiedTableName(schema, tableName)} (
                idempotency_key,
                status_code,
                response_body,
                content_type,
                created_at,
                expires_at
            )
            VALUES (
                @key,
                @status_code,
                @response_body,
                @content_type,
                @created_at,
                @expires_at
            );
            """;

        await using NpgsqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("key", key);
        command.Parameters.AddWithValue("status_code", statusCode);
        command.Parameters.AddWithValue("response_body", (object?)responseBody ?? DBNull.Value);
        command.Parameters.AddWithValue("content_type", (object?)contentType ?? DBNull.Value);
        command.Parameters.AddWithValue("created_at", createdAt);
        command.Parameters.AddWithValue("expires_at", (object?)expiresAt ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> RecordExistsInStorageAsync(
        string schema,
        string tableName,
        string key,
        CancellationToken cancellationToken = default)
    {
        await using NpgsqlConnection connection = await OpenConnectionAsync(cancellationToken);

        string sql = $"""
            SELECT EXISTS (
                SELECT 1
                FROM {GetQualifiedTableName(schema, tableName)}
                WHERE idempotency_key = @key
            );
            """;

        await using NpgsqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("key", key);

        object? scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar is true;
    }

    private async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new InvalidOperationException("PostgreSQL connection string is not initialized.");

        NpgsqlConnection connection = new(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static string GetQualifiedTableName(string schema, string tableName)
    {
        return $"{QuoteIdentifier(schema)}.{QuoteIdentifier(tableName)}";
    }

    private static string QuoteIdentifier(string identifier)
    {
        return $"\"{identifier.Replace("\"", "\"\"")}\"";
    }
}