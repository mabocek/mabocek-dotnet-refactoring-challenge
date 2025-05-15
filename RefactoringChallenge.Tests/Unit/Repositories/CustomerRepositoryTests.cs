using Moq;
using RefactoringChallenge.Factories;
using RefactoringChallenge.Orchestration.Repositories;
using RefactoringChallenge.Tests.Unit.Factories;

namespace RefactoringChallenge.Tests.Unit.Repositories;

[TestFixture]
public class CustomerRepositoryTests
{
    private CustomerRepository _customerRepository = null!;
    private Mock<IDatabaseConnectionFactory> _connectionFactoryMock = null!;

    // Use custom mock classes instead of mocking non-virtual methods
    private MockDbConnection _connection = null!;
    private MockDbCommand _command = null!;
    private MockDbDataReader _reader = null!;

    [SetUp]
    public void Setup()
    {
        // Initialize our custom mock classes
        _reader = new MockDbDataReader();
        _command = new MockDbCommand { ReaderToReturn = _reader };
        _connection = new MockDbConnection { CommandToReturn = _command };

        _connectionFactoryMock = new Mock<IDatabaseConnectionFactory>();
        _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
            .ReturnsAsync(_connection);

        _customerRepository = new CustomerRepository(_connectionFactoryMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _reader?.Dispose();
        _command?.Dispose();
        _connection?.Dispose();
    }

    [Test]
    public async Task GetCustomerByIdAsync_WhenCustomerExists_ReturnsCustomer()
    {
        // Arrange
        int customerId = 1;

        // Setup reader to return one row and then no more rows
        _reader.SetupReadResults(true, false);

        // Setup column values for the customer
        _reader.SetupValue(0, customerId); // Id
        _reader.SetupValue(1, "John Doe"); // Name
        _reader.SetupValue(2, "john@example.com"); // Email
        _reader.SetupValue(3, true); // IsVip
        _reader.SetupValue(4, DateTime.Now); // RegistrationDate

        // Act
        var customer = await _customerRepository.GetCustomerByIdAsync(customerId);

        // Assert
        Assert.That(customer, Is.Not.Null);
        Assert.That(customer.Id, Is.EqualTo(customerId));
        Assert.That(customer.Name, Is.EqualTo("John Doe"));
        Assert.That(customer.Email, Is.EqualTo("john@example.com"));
        Assert.That(customer.IsVip, Is.EqualTo(true));

        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Once);
    }

    [Test]
    public async Task GetCustomerByIdAsync_WhenCustomerDoesNotExist_ReturnsNull()
    {
        // Arrange
        int customerId = 999;

        // Setup reader to return no rows
        _reader.SetupReadResults(false);

        // Act
        var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
        
        // Assert
        Assert.That(customer, Is.Null);
        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Once);
    }

    [Test]
    public void Constructor_WhenConnectionFactoryIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CustomerRepository(null!));
    }
}
