using Microsoft.Extensions.Logging;
using Moq;
using RefactoringChallenge.Factories;
using RefactoringChallenge.Orchestration.Repositories;
using RefactoringChallenge.Orchestration.Services;
using RefactoringChallenge.Services;
using RefactoringChallenge.Tests.TestHelpers;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace RefactoringChallenge.Tests.Integration;

[TestFixture]
[Category("Integration")]
public class OrderProcessingServiceIntegrationTests
{
    private IDatabaseConnectionFactory _connectionFactory = null!;
    private CustomerRepository _customerRepository = null!;
    private OrderRepository _orderRepository = null!;
    private ProductRepository _productRepository = null!;
    private InventoryRepository _inventoryRepository = null!;
    private OrderProcessingService _orderProcessingService = null!;
    private DiscountService _discountService = null!;
    private DbConnection _connection = null!;
    private bool _isDocker = false;

    [SetUp]
    public void Setup()
    {
        // Check if we're running in Docker
        _isDocker = Environment.GetEnvironmentVariable("DOCKER_CONTAINER") == "true";

        if (_isDocker)
        {
            // Use SQL Server connection for Docker
            _connectionFactory = new DockerSqlConnectionFactory();

            // Get a reference to the actual connection
            _connection = _connectionFactory.CreateConnectionAsync().Result;
        }
        else
        {
            // Use the shared in-memory connection factory for local development
            _connectionFactory = SharedInMemoryConnectionFactory.Instance;

            // Reset the database to a clean state with test data
            ((SharedInMemoryConnectionFactory)_connectionFactory).Reset();

            // Get a reference to the actual connection
            _connection = _connectionFactory.CreateConnectionAsync().Result;

            // Ensure database is properly initialized with tables
            ((SharedInMemoryConnectionFactory)_connectionFactory).InitializeDatabase();

            // Re-seed the test data to make sure it exists
            ((SharedInMemoryConnectionFactory)_connectionFactory).SeedTestData();
        }

        // Create repositories
        _productRepository = new ProductRepository(_connectionFactory);
        _customerRepository = new CustomerRepository(_connectionFactory);
        _orderRepository = new OrderRepository(_connectionFactory, _productRepository);
        _inventoryRepository = new InventoryRepository(_connectionFactory);

        // Create services with mocked loggers
        var discountServiceLoggerMock = new Mock<ILogger<DiscountService>>();
        _discountService = new DiscountService(discountServiceLoggerMock.Object);

        var orderProcessingServiceLoggerMock = new Mock<ILogger<OrderProcessingService>>();

        // Create the service to test
        _orderProcessingService = new OrderProcessingService(
            _customerRepository,
            _orderRepository,
            _inventoryRepository,
            _discountService,
            orderProcessingServiceLoggerMock.Object
        );

        // For Docker, we need to initialize test data
        if (_isDocker)
        {
            SeedDockerTestData().Wait();
        }
    }

    private async Task SeedDockerTestData()
    {
        try
        {
            // First clear any existing test data - order matters due to foreign key constraints
            await ExecuteCommandAsync("DELETE FROM OrderLogs"); // Delete logs first (they reference orders)
            await ExecuteCommandAsync("DELETE FROM OrderItems"); // Then order items (they reference orders)
            await ExecuteCommandAsync("DELETE FROM Orders"); // Then orders (they reference customers)
            await ExecuteCommandAsync("DELETE FROM Inventory"); // Then inventory (it references products)
            await ExecuteCommandAsync("DELETE FROM Products"); // Then products
            await ExecuteCommandAsync("DELETE FROM Customers"); // Finally customers

            // Insert test customers - Make sure IDs match with what the test expects (1-3) and avoid duplicate IDs
            await ExecuteCommandAsync(@"
                INSERT INTO Customers (Id, Name, Email, IsVip, RegistrationDate)
                VALUES
                (1, 'John Doe', 'john.doe@example.com', 0, '2023-01-01'), -- 2 years from 2025, gets 2% discount
                (2, 'Jane Smith', 'jane.smith@example.com', 1, '2018-05-15'), -- VIP plus >5 years, gets 15% discount
                (3, 'VIP Customer', 'vip@example.com', 1, '2015-10-15') -- VIP plus >5 years, gets 15% discount
            ");

            // Insert test products
            await ExecuteCommandAsync(@"
                INSERT INTO Products (Id, Name, Category, Price)
                VALUES
                (1, 'Product 1', 'Category A', 100.00),
                (2, 'Product 2', 'Category B', 200.00),
                (3, 'Product 3', 'Category A', 300.00)
            ");

            // Insert test inventory
            await ExecuteCommandAsync(@"
                INSERT INTO Inventory (ProductId, StockQuantity)
                VALUES
                (1, 10),
                (2, 20),
                (3, 30)
            ");

            // Insert test orders - ensure DiscountAmount column is included too
            await ExecuteCommandAsync(@"
                INSERT INTO Orders (Id, CustomerId, OrderDate, TotalAmount, Status, DiscountPercent, DiscountAmount)
                VALUES
                (1, 1, '2023-05-10', 300.00, 'Pending', 0, 0),
                (2, 1, '2023-05-11', 200.00, 'Ready', 0, 0),
                (3, 3, '2023-05-12', 600.00, 'Pending', 0, 0)
            ");

            // Insert test order items
            await ExecuteCommandAsync(@"
                SET IDENTITY_INSERT OrderItems ON;
                INSERT INTO OrderItems (Id, OrderId, ProductId, Quantity, UnitPrice)
                VALUES
                (1, 1, 1, 1, 100.00),
                (2, 1, 2, 1, 200.00),
                (3, 2, 1, 2, 100.00),
                (4, 3, 3, 2, 300.00);
                SET IDENTITY_INSERT OrderItems OFF;
            ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding Docker test data: {ex.Message}");
            throw;
        }
    }

    private async Task ExecuteCommandAsync(string commandText)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = commandText;
        await command.ExecuteNonQueryAsync();
    }

    [TearDown]
    public void TearDown()
    {
        // We're using a shared connection that's managed by the SharedInMemoryConnectionFactory
        // No need to dispose it after each test
    }

    [Test]
    public async Task ProcessCustomerOrdersAsync_WithValidCustomerId_ProcessesOrders()
    {
        // Arrange
        int customerId = 1; // Use the test customer from our seed data (VIP)

        // Verify customer exists before the test
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT COUNT(*) FROM Customers WHERE Id = 1";
            var count = Convert.ToInt32(command.ExecuteScalar());
            Assert.That(count, Is.EqualTo(1), "Customer record not found in database");
        }

        // Verify order exists before the test
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT Status FROM Orders WHERE Id = 1";
            var status = command.ExecuteScalar()?.ToString();
            Assert.That(status, Is.EqualTo("Pending"), "Order status should be 'Pending'");
        }

        // Act
        await _orderProcessingService.ProcessCustomerOrdersAsync(customerId);

        // Assert
        var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
        Assert.That(customer, Is.Not.Null);

        var orders = await _orderRepository.GetPendingOrdersByCustomerIdAsync(customerId);
        Assert.That(orders.Count, Is.EqualTo(0)); // All orders should be processed

        // Check that the order has been updated with the correct status and total amount
        // This assumes the order ID is 1 from the seed data
        var order = await _orderRepository.GetOrderByIdAsync(1);
        Assert.That(order, Is.Not.Null);
        Assert.That(order.Status, Is.EqualTo("Ready"));

        // There should be a 2% discount due to the customer having registered in 2023 (2 years from 2025)
        decimal expectedTotalWithDiscount = 294m; // 2% discount for a customer with 2 years
        Assert.That(order.TotalAmount, Is.EqualTo(expectedTotalWithDiscount));
    }

    [Test]
    public void ProcessCustomerOrdersAsync_WithInvalidCustomerId_ThrowsException()
    {
        // Arrange
        int invalidCustomerId = 999;

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _orderProcessingService.ProcessCustomerOrdersAsync(invalidCustomerId));
    }

    [Test]
    public async Task ProcessCustomerOrdersAsync_ForVipCustomerWithLargeOrder_AppliesCorrectDiscounts()
    {
        // Arrange - Use the VIP customer with ID 3 from seed data
        int vipCustomerId = 3;

        // Verify customer exists before the test
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT COUNT(*) FROM Customers WHERE Id = 3";
            var count = Convert.ToInt32(command.ExecuteScalar());
            Assert.That(count, Is.EqualTo(1), "VIP Customer record not found in database");
        }

        // Verify order exists before the test
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT Status FROM Orders WHERE Id = 3";
            var status = command.ExecuteScalar()?.ToString();
            Assert.That(status, Is.EqualTo("Pending"), "Order status should be 'Pending'");
        }

        // Act
        await _orderProcessingService.ProcessCustomerOrdersAsync(vipCustomerId);

        // Assert
        var order = await _orderRepository.GetOrderByIdAsync(3);
        Assert.That(order, Is.Not.Null);
        Assert.That(order.Status, Is.EqualTo("Ready"));

        // VIP discount (10%) + 5+ years loyalty discount (5%) + order size discount (0% for 600)
        // Total discount should be 15%
        decimal expectedTotalWithDiscount = 600m * 0.85m;
        Assert.That(order.TotalAmount, Is.EqualTo(expectedTotalWithDiscount));
    }
}
