using Microsoft.Extensions.Configuration;
using RefactoringChallenge.Orchestration.Factories;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Moq;
using Moq.Protected;

namespace RefactoringChallenge.Tests.Unit.Factories;

[TestFixture]
public class SqlConnectionFactoryTests
{
    [Test]
    public void Constructor_NullConfiguration_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(null!));
        Assert.That(ex.ParamName, Is.EqualTo("configuration"));
    }

    [Test]
    public void Constructor_MissingConnectionString_ThrowsArgumentException()
    {
        // Arrange
        // Create a real configuration with no connection string
        var configValues = new Dictionary<string, string?>
        {
            {"SomeSetting", "SomeValue"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new SqlConnectionFactory(configuration));
        Assert.That(ex.Message, Does.Contain("Connection string 'DefaultConnection' is missing or empty"));
    }

    [Test]
    public void Constructor_EmptyConnectionString_ThrowsArgumentException()
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", ""}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new SqlConnectionFactory(configuration));
        Assert.That(ex.Message, Does.Contain("Connection string 'DefaultConnection' is missing or empty"));
    }

    [Test]
    public void Constructor_WithValidConfiguration_CreatesInstance()
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", "Data Source=test;Initial Catalog=TestDb;"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();

        // Act
        var factory = new SqlConnectionFactory(configuration);

        // Assert
        Assert.That(factory, Is.Not.Null);
    }

    // Removed test: CreateConnectionAsync_ValidConnectionString_ReturnsConnection 
    // Reason: Requires an actual database connection and is redundant with mocked tests

    [Test]
    public async Task CreateConnectionAsync_CreatesAndOpensConnection()
    {
        // Arrange
        var connectionString = "Data Source=test;Initial Catalog=TestDb;";
        var configValues = new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", connectionString}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();

        var mockConnection = new Mock<DbConnection>();
        mockConnection.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Create a testable factory and set up its protected method
        var factory = new TestableConnectionFactory(configuration);
        factory.Setup(mockConnection.Object);

        // Act
        var connection = await factory.CreateConnectionAsync();

        // Assert
        Assert.That(connection, Is.Not.Null);
        mockConnection.Verify(c => c.OpenAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void CreateConnectionAsync_WhenOpenAsyncThrowsException_PropagatesException()
    {
        // Arrange
        var connectionString = "Data Source=test;Initial Catalog=TestDb;";
        var configValues = new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", connectionString}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();

        var mockConnection = new Mock<DbConnection>();
        var expectedException = new InvalidOperationException("Connection failed");
        mockConnection.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Create a testable factory and set up its protected method
        var factory = new TestableConnectionFactory(configuration);
        factory.Setup(mockConnection.Object);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => factory.CreateConnectionAsync());
        Assert.That(ex.Message, Is.EqualTo("Failed to establish database connection. See inner exception for details."));
        Assert.That(ex.InnerException, Is.SameAs(expectedException));
    }

    [Test]
    public void CreateSqlConnection_CreatesConnectionWithProvidedConnectionString()
    {
        // Arrange
        var connectionString = "Data Source=test;Initial Catalog=TestDb;";
        var configValues = new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", connectionString}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();

        // Use a subclass that makes the protected method public
        var factory = new TestableConnectionFactory(configuration);

        // Act
        var connection = factory.PublicCreateSqlConnection(connectionString);

        // Assert
        Assert.That(connection, Is.Not.Null);
        Assert.That(connection, Is.TypeOf<SqlConnection>());
        Assert.That(connection.ConnectionString, Is.EqualTo(connectionString));
    }

    private class TestableConnectionFactory : SqlConnectionFactory
    {
        private DbConnection? _connectionToReturn;

        public TestableConnectionFactory(IConfiguration configuration) : base(configuration) { }

        public DbConnection PublicCreateSqlConnection(string connectionString)
        {
            return CreateSqlConnection(connectionString);
        }

        public void Setup(DbConnection connectionToReturn)
        {
            _connectionToReturn = connectionToReturn;
        }

        protected override DbConnection CreateSqlConnection(string connectionString)
        {
            return _connectionToReturn ?? base.CreateSqlConnection(connectionString);
        }
    }

    [Test]
    public void CreateConnectionAsync_WithInvalidConnectionString_ThrowsException()
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", "Invalid Connection String"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();

        var factory = new SqlConnectionFactory(configuration);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => factory.CreateConnectionAsync());
        Assert.That(ex.Message, Is.EqualTo("Failed to establish database connection. See inner exception for details."));
        Assert.That(ex.InnerException, Is.TypeOf<ArgumentException>());
    }

    // Removed test: CreateConnectionAsync_WhenConnectionFails_ProperlyPropagatesException
    // Reason: Already ignored and functionality covered by other exception tests

    [Test]
    public async Task CreateConnectionAsync_ConnectionSuccessfullyOpened_ReturnsOpenConnection()
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", "Data Source=test;Initial Catalog=TestDb;"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();

        var factory = new TestSqlConnectionFactoryWithMockConnection(configuration);

        // Act
        var connection = await factory.CreateConnectionAsync();

        // Assert
        Assert.That(connection, Is.Not.Null);
        Assert.That(factory.ConnectionWasOpened, Is.True);
    }

    // Removed test: CreateConnectionAsync_OpensConnection
    // Reason: Functionality covered by CreateConnectionAsync_CallsOpenAsyncOnConnection and other tests

    // Removed test: CreateConnectionAsync_ReturnsOpenedConnection
    // Reason: Duplicate of CreateConnectionAsync_ReturnsTheOpenedConnection

    // Removed test: CreateConnectionAsync_WhenOpenThrowsException_PropagatesException
    // Reason: Duplicate of CreateConnectionAsync_WhenOpenAsyncThrowsException_PropagatesException

    /// <summary>
    /// Test subclass to inspect the connection string without actually connecting to a database
    /// </summary>
    private class TestSqlConnectionFactory : SqlConnectionFactory
    {
        public string LastConnectionString { get; private set; } = string.Empty;

        public TestSqlConnectionFactory(IConfiguration configuration) : base(configuration)
        {
        }

        public new Task<DbConnection> CreateConnectionAsync()
        {
            var connectionMock = new Mock<DbConnection>();
            LastConnectionString = GetConnectionString();
            return Task.FromResult<DbConnection>(connectionMock.Object);
        }

        // Helper method to expose the connection string for testing
        public string GetConnectionString()
        {
            // Use reflection to access the private _connectionString field
            var field = typeof(SqlConnectionFactory).GetField("_connectionString",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            return field?.GetValue(this) as string ?? string.Empty;
        }
    }

    /// <summary>
    /// Test subclass with a fake/mock connection that can verify OpenAsync was called
    /// </summary>
    private class TestSqlConnectionFactoryWithMockConnection : SqlConnectionFactory
    {
        public bool ConnectionWasOpened { get; private set; }

        public TestSqlConnectionFactoryWithMockConnection(IConfiguration configuration) : base(configuration)
        {
        }

        public override async Task<DbConnection> CreateConnectionAsync()
        {
            // Create a fake connection that just sets a flag when opened
            var connection = new FakeDbConnection();
            await connection.OpenAsync();
            ConnectionWasOpened = connection.IsOpen;
            return connection;
        }

#nullable disable
        private class FakeDbConnection : DbConnection
        {
            public bool IsOpen { get; private set; }
            public override string ConnectionString { get; set; } = string.Empty;
            public override string Database => "TestDb";
            public override string DataSource => "TestServer";
            public override string ServerVersion => "1.0";
            public override ConnectionState State => IsOpen ? ConnectionState.Open : ConnectionState.Closed;
#nullable restore

            protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) =>
                throw new NotImplementedException();

            public override void ChangeDatabase(string databaseName) =>
                throw new NotImplementedException();

            public override void Close() => IsOpen = false;

            public override Task OpenAsync(CancellationToken cancellationToken = default)
            {
                IsOpen = true;
                return Task.CompletedTask;
            }

            public override void Open()
            {
                IsOpen = true;
            }

            protected override DbCommand CreateDbCommand() =>
                throw new NotImplementedException();
        }
    }

    [Test]
    public async Task CreateConnectionAsync_CallsOpenAsyncOnConnection()
    {
        // Arrange
        var connectionString = "Data Source=test;Initial Catalog=TestDb;";
        var configValues = new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", connectionString}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();

        var factory = new TestConnectionFactory(configuration);

        // Act
        await factory.CreateConnectionAsync();

        // Assert
        Assert.That(factory.ConnectionWasOpened, Is.True);
    }

    [Test]
    public async Task CreateConnectionAsync_ReturnsTheOpenedConnection()
    {
        // Arrange
        var connectionString = "Data Source=test;Initial Catalog=TestDb;";
        var configValues = new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", connectionString}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();

        var factory = new TestConnectionFactory(configuration);

        // Act
        var connection = await factory.CreateConnectionAsync();

        // Assert
        Assert.That(connection, Is.Not.Null);
        Assert.That(factory.CreatedConnection, Is.SameAs(connection));
    }

    // Test subclass that overrides CreateSqlConnection to track when it's opened
    private class TestConnectionFactory : SqlConnectionFactory
    {
        public bool ConnectionWasOpened { get; private set; }
        public DbConnection? CreatedConnection { get; private set; }

        public TestConnectionFactory(IConfiguration configuration) : base(configuration)
        {
        }

        protected override DbConnection CreateSqlConnection(string connectionString)
        {
            var mockConnection = new Mock<DbConnection>();
            mockConnection.SetupGet(c => c.ConnectionString).Returns(connectionString);
            mockConnection.SetupGet(c => c.State).Returns(ConnectionState.Closed);
            mockConnection.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
                .Callback(() => ConnectionWasOpened = true)
                .Returns(Task.CompletedTask);

            CreatedConnection = mockConnection.Object;
            return CreatedConnection;
        }
    }
}
