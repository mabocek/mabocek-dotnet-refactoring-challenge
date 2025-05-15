using RefactoringChallenge.Factories;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RefactoringChallenge.Orchestration.Helpers;

namespace RefactoringChallenge.Orchestration.Factories;

public class SqlConnectionFactory : IDatabaseConnectionFactory
{
    private readonly string _connectionString;
    private readonly ILogger<SqlConnectionFactory>? _logger;

    public SqlConnectionFactory(IConfiguration configuration, ILogger<SqlConnectionFactory>? logger = null)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        _logger = logger;

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger?.LogError("Connection string 'DefaultConnection' is missing or empty");
            throw new ArgumentException("Connection string 'DefaultConnection' is missing or empty.", nameof(configuration));
        }

        _connectionString = connectionString;
        _logger?.LogDebug("SQL Connection Factory initialized with {ConnectionString}",
            ConnectionStringHelper.MaskConnectionString(connectionString));
    }

    public virtual async Task<DbConnection> CreateConnectionAsync()
    {
        try
        {
            var connection = CreateSqlConnection(_connectionString);
            _logger?.LogDebug("Opening SQL connection");
            await connection.OpenAsync();
            _logger?.LogDebug("SQL connection established successfully");
            return connection;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to establish SQL connection: {ErrorMessage}. Connection string: {ConnectionString}",
                ex.Message, ConnectionStringHelper.MaskConnectionString(_connectionString));
            throw new InvalidOperationException("Failed to establish database connection. See inner exception for details.", ex);
        }
    }

    // Protected virtual method to allow for testing
    protected virtual DbConnection CreateSqlConnection(string connectionString)
    {
        return new SqlConnection(connectionString);
    }
}
