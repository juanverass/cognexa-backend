using Cognexa.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace Cognexa.Tests.Compartilhado;

public sealed class PostgreSqlFactAttribute : FactAttribute
{
    public PostgreSqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("COGNEXA_TEST_CONNECTION")))
            Skip = "Defina COGNEXA_TEST_CONNECTION para executar contra PostgreSQL descartável.";
    }
}
public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly string _schema = "teste_" + Guid.NewGuid().ToString("N");
    private readonly string? _baseConnection = Environment.GetEnvironmentVariable("COGNEXA_TEST_CONNECTION");
    public string ConnectionString { get; private set; } = "";
    public async Task InitializeAsync()
    {
        if (string.IsNullOrWhiteSpace(_baseConnection))
            return;
        await using var connection = new NpgsqlConnection(_baseConnection);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE SCHEMA {_schema}", connection);
        await command.ExecuteNonQueryAsync();
        var builder = new NpgsqlConnectionStringBuilder(_baseConnection) { SearchPath = _schema };
        ConnectionString = builder.ConnectionString;
        await using var db = Contexto();
        await db.Database.MigrateAsync();
    }
    public CognexaDbContext Contexto() => new(new DbContextOptionsBuilder<CognexaDbContext>().UseNpgsql(ConnectionString).Options);
    public async Task DisposeAsync()
    {
        if (string.IsNullOrWhiteSpace(_baseConnection))
            return;
        await using var connection = new NpgsqlConnection(_baseConnection);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP SCHEMA IF EXISTS {_schema} CASCADE", connection);
        await command.ExecuteNonQueryAsync();
    }
}
