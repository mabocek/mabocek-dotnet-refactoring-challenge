using RefactoringChallenge.Factories;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace RefactoringChallenge.Tests.TestHelpers;

/// <summary>
/// A factory that provides a SQL Server connection for Docker integration tests.
/// </summary>
public class DockerSqlConnectionFactory : IDatabaseConnectionFactory
{
    private readonly string _connectionString;

    public DockerSqlConnectionFactory()
    {
        // Get connection string from environment variable or use default Docker connection
        _connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ??
            "Server=mssql,1433;Database=refactoringchallenge;User ID=sa;Password=RCPassword1!;TrustServerCertificate=True;";

        Console.WriteLine($"Using connection string: {_connectionString}");
    }

    public async Task<DbConnection> CreateConnectionAsync()
    {
        var connection = new SqlConnection(_connectionString);

        // Try with retry logic
        int maxRetries = 3;
        int retryCount = 0;
        bool connected = false;

        while (!connected && retryCount < maxRetries)
        {
            try
            {
                await connection.OpenAsync();
                Console.WriteLine("Successfully connected to SQL Server database.");
                connected = true;
            }
            catch (Exception ex)
            {
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    Console.WriteLine($"Failed to connect after {maxRetries} attempts: {ex.Message}");
                    throw;
                }
                Console.WriteLine($"Connection attempt {retryCount} failed: {ex.Message}. Retrying in 3 seconds...");
                await Task.Delay(3000);
            }
        }

        return connection;
    }
}
