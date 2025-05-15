using System.Data.Common;
using System.Data.SqlClient;
using RefactoringChallenge.Factories;

namespace RefactoringChallenge.Tests.Unit.Factories;

public class MockDatabaseConnectionFactory : IDatabaseConnectionFactory
{
    private readonly string _connectionString;

    public MockDatabaseConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<DbConnection> CreateConnectionAsync()
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}
