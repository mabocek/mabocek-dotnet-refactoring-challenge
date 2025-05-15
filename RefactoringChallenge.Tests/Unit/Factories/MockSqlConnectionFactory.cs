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
public class MockSqlConnectionFactory : SqlConnectionFactory
{
    private readonly DbConnection _connectionToReturn;
    private readonly Exception? _exceptionToThrow;

    public MockSqlConnectionFactory(IConfiguration configuration, DbConnection connectionToReturn)
        : base(configuration)
    {
        _connectionToReturn = connectionToReturn;
        _exceptionToThrow = null;
    }

    public MockSqlConnectionFactory(IConfiguration configuration, Exception exceptionToThrow)
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
    /// Creates a new MockSqlConnectionFactory with a mock connection
    /// </summary>
    public static MockSqlConnectionFactory CreateWithMockConnection(IConfiguration configuration)
    {
        var mockConnection = new Mock<DbConnection>();
        mockConnection.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        return new MockSqlConnectionFactory(configuration, mockConnection.Object);
    }

    /// <summary>
    /// Creates a new MockSqlConnectionFactory that throws an exception
    /// </summary>
    public static MockSqlConnectionFactory CreateWithException(IConfiguration configuration, Exception exceptionToThrow)
    {
        return new MockSqlConnectionFactory(configuration, exceptionToThrow);
    }
}
