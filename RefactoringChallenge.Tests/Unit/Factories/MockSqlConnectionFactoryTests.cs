using Microsoft.Extensions.Configuration;
using Moq;
using RefactoringChallenge.Factories;
using RefactoringChallenge.Orchestration.Factories;
using System.Data.Common;
using System.Data;

namespace RefactoringChallenge.Tests.Unit.Factories;

[TestFixture]
public class TestSqlConnectionFactoryTests
{
    [Test]
    public async Task CreateConnectionAsync_ReturnsProvidedConnection()
    {
        // Arrange
        var mockConnection = new Mock<DbConnection>();
        var configuration = CreateTestConfiguration();
        var factory = new TestSqlConnectionFactory(configuration, mockConnection.Object);

        // Act
        var result = await factory.CreateConnectionAsync();

        // Assert
        Assert.That(result, Is.SameAs(mockConnection.Object));
    }

    [Test]
    public void CreateConnectionAsync_WithException_ThrowsException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        var configuration = CreateTestConfiguration();
        var factory = new TestSqlConnectionFactory(configuration, expectedException);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => factory.CreateConnectionAsync());
        Assert.That(ex.Message, Is.EqualTo("Failed to establish database connection. See inner exception for details."));
        Assert.That(ex.InnerException, Is.SameAs(expectedException));
    }

    [Test]
    public async Task CreateWithMockConnection_CreatesFactoryWithWorkingConnection()
    {
        // Arrange & Act
        var configuration = CreateTestConfiguration();
        var factory = TestSqlConnectionFactory.CreateWithMockConnection(configuration);
        var connection = await factory.CreateConnectionAsync();

        // Assert
        Assert.That(connection, Is.Not.Null);
    }

    [Test]
    public void CreateWithException_CreatesFactoryThatThrows()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        var configuration = CreateTestConfiguration();
        var factory = TestSqlConnectionFactory.CreateWithException(configuration, expectedException);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => factory.CreateConnectionAsync());
        Assert.That(ex.Message, Is.EqualTo("Failed to establish database connection. See inner exception for details."));
        Assert.That(ex.InnerException, Is.SameAs(expectedException));
    }

    private static IConfiguration CreateTestConfiguration()
    {
        var configValues = new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", "Data Source=test;Initial Catalog=TestDb;"}
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();
    }
}
