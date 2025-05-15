using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RefactoringChallenge.Factories;
using RefactoringChallenge.Models;
using RefactoringChallenge.Orchestration.Factories;
using RefactoringChallenge.Orchestration.Repositories;
using RefactoringChallenge.Orchestration.Services;
using RefactoringChallenge.Repositories;
using RefactoringChallenge.Services;
using RefactoringChallenge.Tests.TestHelpers;

namespace RefactoringChallenge.Tests.Integration;

[TestFixture]
[Category("Integration")]
public class IntegrationTests
{
    private DbConnection _connection = null!;
    private IDatabaseConnectionFactory _connectionFactory = null!;
    private ICustomerRepository _customerRepository = null!;
    private IProductRepository _productRepository = null!;
    private IInventoryRepository _inventoryRepository = null!;
    private IOrderRepository _orderRepository = null!;
    private IDiscountService _discountService = null!;
    private IOrderProcessingService _orderProcessingService = null!;
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

            // Get a reference to the connection (will be SQL Server connection in Docker)
            _connection = _connectionFactory.CreateConnectionAsync().Result;
        }
        else
        {
            // Use the shared in-memory connection for local development
            _connectionFactory = SharedInMemoryConnectionFactory.Instance;
            // Reset the database to a clean state
            ((SharedInMemoryConnectionFactory)_connectionFactory).Reset();

            // Get a reference to the shared connection
            _connection = _connectionFactory.CreateConnectionAsync().Result;
        }

        // Create repositories and services
        _productRepository = new ProductRepository(_connectionFactory);
        _customerRepository = new CustomerRepository(_connectionFactory);
        _inventoryRepository = new InventoryRepository(_connectionFactory);
        _orderRepository = new OrderRepository(_connectionFactory, _productRepository);
        // Create mock loggers for the services
        var orderProcessingLoggerMock = new Mock<ILogger<OrderProcessingService>>();
        var discountServiceLoggerMock = new Mock<ILogger<DiscountService>>();

        _discountService = new DiscountService(discountServiceLoggerMock.Object);
        _orderProcessingService = new RefactoringChallenge.Services.OrderProcessingService(
            _customerRepository,
            _orderRepository,
            _inventoryRepository,
            _discountService,
            orderProcessingLoggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        // We're using a shared connection that's managed by SharedInMemoryConnectionFactory
        // No need to close or dispose it after each test
    }

    [Test]
    public async Task End_To_End_ProcessOrder_Flow()
    {
        if (!_isDocker)
        {
            // Clear all data first to ensure we don't have conflicts
            ((SharedInMemoryConnectionFactory)_connectionFactory).Reset();
        }

        // Use unique IDs for this specific test to avoid conflicts - use higher numbers to prevent collisions
        const int customerId = 9001;
        const int productId = 9001;
        const int orderId = 9001;
        const int orderItemId = 9001;

        // Add a customer
        await ExecuteCommandAsync($"INSERT INTO Customers (Id, Name, Email, IsVip, RegistrationDate) " +
                                  $"VALUES ({customerId}, 'Test Customer', 'test@example.com', 0, '2023-01-01')");

        // Add a product
        await ExecuteCommandAsync($"INSERT INTO Products (Id, Name, Category, Price) " +
                                  $"VALUES ({productId}, 'Test Product', 'Test Category', 100.00)");

        // Add inventory for the product
        await ExecuteCommandAsync($"INSERT INTO Inventory (ProductId, StockQuantity) " +
                                  $"VALUES ({productId}, 10)");

        // Add an order - ensure Status is exactly 'Pending'
        await ExecuteCommandAsync($"INSERT INTO Orders (Id, CustomerId, OrderDate, TotalAmount, Status) " +
                                  $"VALUES ({orderId}, {customerId}, '2023-01-15', 100.00, 'Pending')");

        // Add order item
        await ExecuteCommandAsync($"SET IDENTITY_INSERT OrderItems ON; " +
                                  $"INSERT INTO OrderItems (Id, OrderId, ProductId, Quantity, UnitPrice) " +
                                  $"VALUES ({orderItemId}, {orderId}, {productId}, 1, 100.00); " +
                                  $"SET IDENTITY_INSERT OrderItems OFF;");

        // Debug - verify the order was inserted correctly with Status='Pending'
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = $"SELECT Status FROM Orders WHERE Id = {orderId}";
            var status = command.ExecuteScalar()?.ToString();
            Assert.That(status, Is.EqualTo("Pending"), "Order status should be 'Pending'");
        }

        // Process the order
        var results = await _orderProcessingService.ProcessCustomerOrdersAsync(customerId);

        // Verify results
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Id, Is.EqualTo(orderId));
        Assert.That(results[0].Status, Is.EqualTo("Ready"));

        // Verify inventory was updated - should decrease by 1
        var stockQuantity = await _inventoryRepository.GetStockQuantityByProductIdAsync(productId);
        Assert.That(stockQuantity, Is.EqualTo(9));
    }

    private async Task ExecuteCommandAsync(string commandText)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = commandText;
        await command.ExecuteNonQueryAsync();
    }
}
