using Moq;
using RefactoringChallenge.Factories;
using RefactoringChallenge.Orchestration.Repositories;
using RefactoringChallenge.Tests.Unit.Factories;

namespace RefactoringChallenge.Tests.Unit.Repositories;

[TestFixture]
public class InventoryRepositoryTests
{
    private InventoryRepository _inventoryRepository = null!;
    private Mock<IDatabaseConnectionFactory> _connectionFactoryMock = null!;

    // Use custom mock classes instead of mocking non-virtual methods
    private MockDbConnection _connection = null!;
    private MockDbCommand _command = null!;

    [SetUp]
    public void Setup()
    {
        // Initialize our custom mock classes
        _command = new MockDbCommand();
        _connection = new MockDbConnection { CommandToReturn = _command };

        _connectionFactoryMock = new Mock<IDatabaseConnectionFactory>();
        _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
            .ReturnsAsync(_connection);

        _command.ScalarResult = 10; // Default scalar result for stock quantity

        _inventoryRepository = new InventoryRepository(_connectionFactoryMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _command?.Dispose();
        _connection?.Dispose();
    }

    [Test]
    public async Task GetStockQuantityByProductIdAsync_WhenProductExists_ReturnsQuantity()
    {
        // Arrange
        int productId = 1;
        int expectedQuantity = 10;

        _command.ScalarResult = expectedQuantity;

        // Act
        var quantity = await _inventoryRepository.GetStockQuantityByProductIdAsync(productId);

        // Assert
        Assert.That(quantity, Is.EqualTo(expectedQuantity));
        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Once);
    }

    [Test]
    public async Task GetStockQuantityByProductIdAsync_WhenProductDoesNotExist_ReturnsNull()
    {
        // Arrange
        int productId = 999;

        _command.ScalarResult = DBNull.Value;

        // Act
        var result = await _inventoryRepository.GetStockQuantityByProductIdAsync(productId);

        // Assert
        Assert.That(result, Is.Null);
        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateStockQuantityAsync_ValidParameters_UpdatesStock()
    {
        // Arrange
        int productId = 1;
        int quantity = 5;

        _command.NonQueryResult = 1;

        // Act
        await _inventoryRepository.UpdateStockQuantityAsync(productId, quantity);

        // Assert
        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Once);
        Assert.That(_command.NonQueryResult, Is.EqualTo(1));
    }

    [Test]
    public void Constructor_NullConnectionFactory_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new InventoryRepository(null!));
        Assert.That(ex.ParamName, Is.EqualTo("connectionFactory"));
    }

    [Test]
    public async Task UpdateStockQuantityAsync_DatabaseError_PropagatesException()
    {
        // Arrange
        int productId = 1;
        int quantity = 5;
        var expectedException = new InvalidOperationException("Database error");

        _command.ExceptionToThrow = expectedException;

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _inventoryRepository.UpdateStockQuantityAsync(productId, quantity));

        Assert.That(ex, Is.SameAs(expectedException));
    }
}
