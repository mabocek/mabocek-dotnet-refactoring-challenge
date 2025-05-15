using System.Data;
using System.Data.Common;
using RefactoringChallenge.Factories;
using RefactoringChallenge.Tests.TestHelpers;

namespace RefactoringChallenge.Tests.Unit.Factories;

/// <summary>
/// A simple in-memory database connection factory for unit tests
/// </summary>
public class InMemoryDatabaseConnectionFactory : IDatabaseConnectionFactory
{
    // Singleton instance
    private static readonly Lazy<InMemoryDatabaseConnectionFactory> _instance =
        new Lazy<InMemoryDatabaseConnectionFactory>(() => new InMemoryDatabaseConnectionFactory());

    public static InMemoryDatabaseConnectionFactory Instance => _instance.Value;

    private readonly InMemoryDbConnection _connection;

    public InMemoryDatabaseConnectionFactory()
    {
        _connection = new InMemoryDbConnection();
    }

    public async Task<DbConnection> CreateConnectionAsync()
    {
        await Task.CompletedTask;
        return _connection;
    }

    /// <summary>
    /// Reset the in-memory database
    /// </summary>
    public void Reset()
    {
        _connection.ClearCommands();
    }

    /// <summary>
    /// Set up the mock to return specific data for a SQL query
    /// </summary>
    public void SetupCommand(string sql, DataTable result)
    {
        _connection.SetupCommand(sql, result);
    }
}
