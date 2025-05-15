using Moq;
using RefactoringChallenge.Factories;
using RefactoringChallenge.Orchestration.Repositories;
using RefactoringChallenge.Tests.Unit.Factories;

namespace RefactoringChallenge.Tests.Unit.Repositories;

[TestFixture]
public class ProductRepositoryTests
{
    private ProductRepository _productRepository = null!;
    private Mock<IDatabaseConnectionFactory> _connectionFactoryMock = null!;

    // Use custom mock classes instead of mocking non-virtual methods
    private MockDbConnection _connection = null!;
    private EnhancedMockDbCommand _command = null!;
    private EnhancedMockDbDataReader _reader = null!;

    [SetUp]
    public void Setup()
    {
        // Initialize our custom mock classes
        _reader = new EnhancedMockDbDataReader();
        _command = new EnhancedMockDbCommand { ReaderToReturn = _reader };
        _connection = new MockDbConnection { CommandToReturn = _command };

        _connectionFactoryMock = new Mock<IDatabaseConnectionFactory>();
        _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
            .ReturnsAsync(_connection);

        _productRepository = new ProductRepository(_connectionFactoryMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _reader?.Dispose();
        _command?.Dispose();
        _connection?.Dispose();
    }

    [Test]
    public async Task GetProductByIdAsync_WhenProductExists_ReturnsProduct()
    {
        // Arrange
        int productId = 1;
        string productName = "Test Product";
        string productCategory = "Electronics";
        decimal productPrice = 99.99m;

        // Setup reader to return one row and then no more rows
        _reader.SetupReadResults(true, false);

        // Setup column values for the product
        _reader.SetupValue(0, productId); // Id
        _reader.SetupValue(1, productName); // Name
        _reader.SetupValue(2, productCategory); // Category
        _reader.SetupValue(3, productPrice); // Price

        // Act
        var product = await _productRepository.GetProductByIdAsync(productId);

        // Assert
        Assert.That(product, Is.Not.Null);
        Assert.That(product!.Id, Is.EqualTo(productId));
        Assert.That(product.Name, Is.EqualTo(productName));
        Assert.That(product.Category, Is.EqualTo(productCategory));
        Assert.That(product.Price, Is.EqualTo(productPrice));

        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Once);
    }

    [Test]
    public async Task GetProductByIdAsync_WhenProductDoesNotExist_ReturnsNull()
    {
        // Arrange
        int productId = 999;

        // Setup reader to return no rows
        _reader.SetupReadResults(false);

        // Act
        var product = await _productRepository.GetProductByIdAsync(productId);

        // Assert
        Assert.That(product, Is.Null);
        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Once);
    }

    [Test]
    public void Constructor_WhenConnectionFactoryIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ProductRepository(null!));
    }

    [Test]
    public async Task GetProductByIdAsync_VerifiesCommandParameters()
    {
        // Arrange
        int productId = 5;

        // Setup reader to return values that will be ignored in this test
        _reader.SetupReadResults(false);

        // Act
        await _productRepository.GetProductByIdAsync(productId);

        // Assert
        Assert.That(_command.CommandText, Does.Contain("SELECT Id, Name, Category, Price FROM Products"));
        Assert.That(_command.CommandText, Does.Contain("WHERE Id = @ProductId"));
        Assert.That(_command.Parameters.Count, Is.EqualTo(1));
        Assert.That(_command.Parameters[0].ParameterName, Is.EqualTo("@ProductId"));
        Assert.That(_command.Parameters[0].Value, Is.EqualTo(productId));
    }

    [Test]
    public async Task GetProductByIdAsync_WhenReaderThrowsException_PropagatesException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Database error");
        _command.ExceptionToThrow = expectedException;

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _productRepository.GetProductByIdAsync(1));
        Assert.That(exception, Is.SameAs(expectedException));
    }
}
