using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RefactoringChallenge.Factories;
using RefactoringChallenge.Orchestration.Factories;
using System.Data.Common;

namespace RefactoringChallenge.Tests.Unit.Factories;

/// <summary>
/// A mock implementation of SqlConnectionFactory that can be used in tests.
/// </summary>
public class TestSqlConnectionFactory : SqlConnectionFactory
{
    private readonly DbConnection _connectionToReturn;
    private readonly Exception? _exceptionToThrow;

    public TestSqlConnectionFactory(IConfiguration configuration, DbConnection connectionToReturn)
        : base(configuration)
    {
        _connectionToReturn = connectionToReturn;
        _exceptionToThrow = null;
    }

    public TestSqlConnectionFactory(IConfiguration configuration, Exception exceptionToThrow)
        : base(configuration)
    {
        _exceptionToThrow = exceptionToThrow;
        _connectionToReturn = new Mock<DbConnection>().Object;
    }

    public override async Task<DbConnection> CreateConnectionAsync()
    {
        if (_exceptionToThrow != null)
        {
            throw new InvalidOperationException("Failed to establish database connection. See inner exception for details.", _exceptionToThrow);
        }

        await Task.CompletedTask; // Add await to satisfy async method signature
        return _connectionToReturn;
    }

    /// <summary>
    /// Creates a new TestSqlConnectionFactory with a mock connection
    /// </summary>
    public static TestSqlConnectionFactory CreateWithMockConnection(IConfiguration configuration)
    {
        var mockConnection = new Mock<DbConnection>();
        mockConnection.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        return new TestSqlConnectionFactory(configuration, mockConnection.Object);
    }

    /// <summary>
    /// Creates a new TestSqlConnectionFactory that throws an exception
    /// </summary>
    public static TestSqlConnectionFactory CreateWithException(IConfiguration configuration, Exception exceptionToThrow)
    {
        return new TestSqlConnectionFactory(configuration, exceptionToThrow);
    }
}
