using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Moq;
using RefactoringChallenge.Orchestration.Factories;

namespace RefactoringChallenge.Tests.Unit.Factories;

/// <summary>
/// Test implementation of SqlConnectionFactory that allows us to verify the connection is opened
/// without actually connecting to a database
/// </summary>
public class TestableConnectionFactory : SqlConnectionFactory
{
    public bool ConnectionOpened { get; private set; }
    public string LastConnectionString { get; private set; } = string.Empty;
    private readonly DbConnection _mockConnection;

    public TestableConnectionFactory(IConfiguration configuration, DbConnection? mockConnection = null)
        : base(configuration)
    {
        _mockConnection = mockConnection ?? new Mock<DbConnection>().Object;
    }

    protected override DbConnection CreateSqlConnection(string connectionString)
    {
        LastConnectionString = connectionString;

        // Track when OpenAsync is called on the mock connection
        var mockConnection = Mock.Get(_mockConnection);
        mockConnection.Setup(c => c.OpenAsync(It.IsAny<System.Threading.CancellationToken>()))
            .Callback(() => ConnectionOpened = true)
            .Returns(Task.CompletedTask);

        return _mockConnection;
    }
}
