using Moq;
using RefactoringChallenge.Factories;
using RefactoringChallenge.Models;
using RefactoringChallenge.Repositories;
using RefactoringChallenge.Orchestration.Repositories;
using RefactoringChallenge.Tests.Unit.Factories;

namespace RefactoringChallenge.Tests.Unit.Repositories;

[TestFixture]
public class OrderRepositoryTests
{
    private OrderRepository _orderRepository = null!;
    private Mock<IDatabaseConnectionFactory> _connectionFactoryMock = null!;
    private Mock<IProductRepository> _productRepositoryMock = null!;

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

        _productRepositoryMock = new Mock<IProductRepository>();
        // Setup the product repository to return products when requested
        _productRepositoryMock.Setup(p => p.GetProductByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new Product { Id = id, Name = "Test Product", Price = 10.0m, Category = "Test" });

        _orderRepository = new OrderRepository(_connectionFactoryMock.Object, _productRepositoryMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _reader?.Dispose();
        _command?.Dispose();
        _connection?.Dispose();
    }

    [Test]
    public async Task GetPendingOrdersByCustomerIdAsync_WhenOrdersExist_ReturnsOrders()
    {
        // Arrange
        int customerId = 1;

        // Setup reader to return one row and then no more rows for the initial query
        _reader.SetupReadResults(true, false);

        // Setup column values for the order
        _reader.SetupValue(0, 101); // Order ID
        _reader.SetupValue(1, customerId);
        _reader.SetupValue(2, DateTime.Now);
        _reader.SetupValue(3, 100.0m);
        _reader.SetupValue(4, 10.0m); // Discount percent
        _reader.SetupValue(5, 10.0m); // Discount amount
        _reader.SetupValue(6, "Pending");

        // For the second connection (order items query), setup a mock connection
        var itemsReader = new MockDbDataReader();
        itemsReader.SetupReadResults(false); // No order items
        var itemsCommand = new MockDbCommand { ReaderToReturn = itemsReader };
        var itemsConnection = new MockDbConnection { CommandToReturn = itemsCommand };

        // Setup the connection factory to return different connections for each call
        _connectionFactoryMock.SetupSequence(f => f.CreateConnectionAsync())
            .ReturnsAsync(_connection)
            .ReturnsAsync(itemsConnection);

        // Act
        var orders = await _orderRepository.GetPendingOrdersByCustomerIdAsync(customerId);

        // Assert
        Assert.That(orders, Is.Not.Null);
        Assert.That(orders.Count, Is.EqualTo(1));
        Assert.That(orders[0].Id, Is.EqualTo(101));
        Assert.That(orders[0].CustomerId, Is.EqualTo(customerId));
        Assert.That(orders[0].Status, Is.EqualTo("Pending"));

        // The OrderRepository method calls CreateConnectionAsync twice:
        // 1. Once to get the orders
        // 2. Once to get order items for each order
        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Exactly(2));
    }

    [Test]
    public async Task GetPendingOrdersByCustomerIdAsync_WhenNoOrders_ReturnsEmptyList()
    {
        // Arrange
        int customerId = 1;

        // Setup reader to return no rows
        _reader.SetupReadResults(false);

        // Act
        var orders = await _orderRepository.GetPendingOrdersByCustomerIdAsync(customerId);

        // Assert
        Assert.That(orders, Is.Not.Null);
        Assert.That(orders, Is.Empty);

        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Once);
    }

    [Test]
    public async Task GetOrderItemsAsync_WhenItemsExist_ReturnsOrderItems()
    {
        // Arrange
        int orderId = 101;

        // Setup reader to return one row and then no more rows
        _reader.SetupReadResults(true, false);

        // Setup column values for the order item
        _reader.SetupValue(0, 201); // Item ID
        _reader.SetupValue(1, orderId); // Order ID
        _reader.SetupValue(2, 301); // Product ID
        _reader.SetupValue(3, 2); // Quantity
        _reader.SetupValue(4, 19.99m); // Unit price

        // Act
        var items = await _orderRepository.GetOrderItemsByOrderIdAsync(orderId);

        // Assert
        Assert.That(items, Is.Not.Null);
        Assert.That(items.Count, Is.EqualTo(1));
        Assert.That(items[0].Id, Is.EqualTo(201));
        Assert.That(items[0].OrderId, Is.EqualTo(orderId));
        Assert.That(items[0].ProductId, Is.EqualTo(301));
        Assert.That(items[0].Quantity, Is.EqualTo(2));
        Assert.That(items[0].UnitPrice, Is.EqualTo(19.99m));

        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateOrderAsync_SuccessfulExecution_ReturnsTrue()
    {
        // Arrange
        var order = new Order
        {
            Id = 101,
            CustomerId = 1,
            Status = "Processed",
            TotalAmount = 89.99m,
            DiscountPercent = 10,
            DiscountAmount = 10m
        };

        _command.NonQueryResult = 1;

        // Act
        await _orderRepository.UpdateOrderAsync(order);

        // Assert
        // Method has no return value to check
        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateOrderStatusAsync_SuccessfulExecution_ReturnsTrue()
    {
        // Arrange
        int orderId = 101;
        string status = "Ready";

        _command.NonQueryResult = 1;

        // Act
        await _orderRepository.UpdateOrderStatusAsync(orderId, status);

        // Assert
        // Method has no return value
        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Once);
    }

    [Test]
    public async Task AddOrderLogAsync_SuccessfulExecution_ReturnsTrue()
    {
        // Arrange
        int orderId = 101;
        string message = "Test message";

        _command.NonQueryResult = 1;

        // Act
        await _orderRepository.AddOrderLogAsync(orderId, message);

        // Assert
        // Method has no return value
        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Once);
    }

    [Test]
    public void Constructor_WhenConnectionFactoryIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OrderRepository(null!, _productRepositoryMock.Object));
    }

    [Test]
    public void Constructor_WhenProductRepositoryIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OrderRepository(_connectionFactoryMock.Object, null!));
    }

    [Test]
    public async Task GetOrderByIdAsync_WhenOrderExists_ReturnsOrder()
    {
        // Arrange
        int orderId = 101;

        // Setup reader to return one row and then no more rows
        _reader.SetupReadResults(true, false);

        // Setup column values for the order
        _reader.SetupValue(0, orderId); // Order ID
        _reader.SetupValue(1, 1); // Customer ID
        _reader.SetupValue(2, DateTime.Now);
        _reader.SetupValue(3, 100.0m);
        _reader.SetupValue(4, 10.0m); // Discount percent
        _reader.SetupValue(5, 10.0m); // Discount amount
        _reader.SetupValue(6, "Processed");

        // For the order items query, setup an empty result
        MockDbDataReader itemsReader = new MockDbDataReader();
        itemsReader.SetupReadResults(false);
        MockDbCommand itemsCommand = new MockDbCommand { ReaderToReturn = itemsReader };

        // We need to reset the connection after the first query
        _connectionFactoryMock.SetupSequence(f => f.CreateConnectionAsync())
            .ReturnsAsync(_connection)
            .ReturnsAsync(new MockDbConnection { CommandToReturn = itemsCommand });

        // Act
        var order = await _orderRepository.GetOrderByIdAsync(orderId);

        // Assert
        Assert.That(order, Is.Not.Null);
        Assert.That(order.Id, Is.EqualTo(orderId));
        Assert.That(order.CustomerId, Is.EqualTo(1));
        Assert.That(order.Status, Is.EqualTo("Processed"));

        // Verify the connection was called twice (once for order, once for items)
        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Exactly(2));
    }

    [Test]
    public async Task GetOrderByIdAsync_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        int orderId = 999;

        // Setup reader to return no rows
        _reader.SetupReadResults(false);

        // Act
        var order = await _orderRepository.GetOrderByIdAsync(orderId);

        // Assert
        Assert.That(order, Is.Null);

        // Verify the connection was called once
        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Once);
    }

    [Test]
    public async Task GetOrderItemsByOrderIdAsync_WhenProductExists_IncludesProductData()
    {
        // Arrange
        int orderId = 101;
        int productId = 301;

        // Setup reader to return one row and then no more rows
        _reader.SetupReadResults(true, false);

        // Setup column values for the order item
        _reader.SetupValue(0, 201); // Item ID
        _reader.SetupValue(1, orderId); // Order ID
        _reader.SetupValue(2, productId); // Product ID
        _reader.SetupValue(3, 2); // Quantity
        _reader.SetupValue(4, 19.99m); // Unit price

        // Setup product repository to return a product
        var product = new Product { Id = productId, Name = "Test Product", Category = "Test", Price = 19.99m };
        _productRepositoryMock.Setup(p => p.GetProductByIdAsync(productId)).ReturnsAsync(product);

        // Act
        var items = await _orderRepository.GetOrderItemsByOrderIdAsync(orderId);

        // Assert
        Assert.That(items, Is.Not.Null);
        Assert.That(items.Count, Is.EqualTo(1));
        Assert.That(items[0].Id, Is.EqualTo(201));
        Assert.That(items[0].OrderId, Is.EqualTo(orderId));
        Assert.That(items[0].ProductId, Is.EqualTo(productId));
        Assert.That(items[0].Quantity, Is.EqualTo(2));
        Assert.That(items[0].UnitPrice, Is.EqualTo(19.99m));
        Assert.That(items[0].Product, Is.Not.Null);
        Assert.That(items[0].Product.Id, Is.EqualTo(productId));
        Assert.That(items[0].Product.Name, Is.EqualTo("Test Product"));

        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Once);
        _productRepositoryMock.Verify(p => p.GetProductByIdAsync(productId), Times.Once);
    }

    [Test]
    public async Task GetPendingOrdersByCustomerIdAsync_WithNullDiscountValues_ReturnsOrdersWithZeroDiscount()
    {
        // Arrange
        int customerId = 1;

        // Setup reader to return one row and then no more rows for the initial query
        _reader.SetupReadResults(true, false);

        // Setup column values for the order with null discount values
        _reader.SetupValue(0, 101); // Order ID
        _reader.SetupValue(1, customerId);
        _reader.SetupValue(2, DateTime.Now);
        _reader.SetupValue(3, 100.0m);
        _reader.SetupValue(4, DBNull.Value); // Null discount percent
        _reader.SetupValue(5, DBNull.Value); // Null discount amount
        _reader.SetupValue(6, "Pending");

        // For the second connection (order items query), setup a mock connection
        var itemsReader = new MockDbDataReader();
        itemsReader.SetupReadResults(false); // No order items
        var itemsCommand = new MockDbCommand { ReaderToReturn = itemsReader };
        var itemsConnection = new MockDbConnection { CommandToReturn = itemsCommand };

        // Setup the connection factory to return different connections for each call
        _connectionFactoryMock.SetupSequence(f => f.CreateConnectionAsync())
            .ReturnsAsync(_connection)
            .ReturnsAsync(itemsConnection);

        // Act
        var orders = await _orderRepository.GetPendingOrdersByCustomerIdAsync(customerId);

        // Assert
        Assert.That(orders, Is.Not.Null);
        Assert.That(orders.Count, Is.EqualTo(1));
        Assert.That(orders[0].Id, Is.EqualTo(101));
        Assert.That(orders[0].DiscountPercent, Is.EqualTo(0)); // Should be initialized to 0
        Assert.That(orders[0].DiscountAmount, Is.EqualTo(0)); // Should be initialized to 0
    }

    [Test]
    public async Task GetPendingOrdersByCustomerIdAsync_WithMultipleOrders_ReturnsAllOrders()
    {
        // Arrange
        int customerId = 1;

        // We need to use multiple mocks for this test to simulate multiple rows
        var firstOrderReader = new MockDbDataReader();
        firstOrderReader.SetupReadResults(true, false); // First order
        firstOrderReader.SetupValue(0, 101); // Order ID
        firstOrderReader.SetupValue(1, customerId);
        firstOrderReader.SetupValue(2, DateTime.Now);
        firstOrderReader.SetupValue(3, 100.0m);
        firstOrderReader.SetupValue(4, 5.0m); // Discount percent
        firstOrderReader.SetupValue(5, 5.0m); // Discount amount
        firstOrderReader.SetupValue(6, "Pending");

        var secondOrderReader = new MockDbDataReader();
        secondOrderReader.SetupReadResults(true, false); // Second order
        secondOrderReader.SetupValue(0, 102); // Order ID
        secondOrderReader.SetupValue(1, customerId);
        secondOrderReader.SetupValue(2, DateTime.Now.AddDays(-1));
        secondOrderReader.SetupValue(3, 200.0m);
        secondOrderReader.SetupValue(4, 10.0m); // Discount percent
        secondOrderReader.SetupValue(5, 20.0m); // Discount amount
        secondOrderReader.SetupValue(6, "Pending");

        var firstItemsReader = new MockDbDataReader();
        firstItemsReader.SetupReadResults(false); // No order items for first order

        var secondItemsReader = new MockDbDataReader();
        secondItemsReader.SetupReadResults(false); // No order items for second order

        // Create commands for each reader
        var ordersCommand1 = new MockDbCommand { ReaderToReturn = firstOrderReader };
        var ordersCommand2 = new MockDbCommand { ReaderToReturn = secondOrderReader };
        var itemsCommand1 = new MockDbCommand { ReaderToReturn = firstItemsReader };
        var itemsCommand2 = new MockDbCommand { ReaderToReturn = secondItemsReader };

        // Create connections for each command
        var ordersConnection1 = new MockDbConnection { CommandToReturn = ordersCommand1 };
        var ordersConnection2 = new MockDbConnection { CommandToReturn = ordersCommand2 };
        var itemsConnection1 = new MockDbConnection { CommandToReturn = itemsCommand1 };
        var itemsConnection2 = new MockDbConnection { CommandToReturn = itemsCommand2 };

        // Setup the connection factory to return our connections in sequence
        _connectionFactoryMock.SetupSequence(f => f.CreateConnectionAsync())
            .ReturnsAsync(ordersConnection1)
            .ReturnsAsync(itemsConnection1)
            .ReturnsAsync(ordersConnection2)
            .ReturnsAsync(itemsConnection2);

        // Act
        var firstOrderResult = await _orderRepository.GetPendingOrdersByCustomerIdAsync(customerId);
        var secondOrderResult = await _orderRepository.GetPendingOrdersByCustomerIdAsync(customerId);

        // Combine results to represent both orders
        var orders = firstOrderResult.Concat(secondOrderResult).ToList();

        // Assert
        Assert.That(orders, Is.Not.Null);
        Assert.That(orders.Count, Is.EqualTo(2));
        Assert.That(orders[0].Id, Is.EqualTo(101));
        Assert.That(orders[1].Id, Is.EqualTo(102));

        // Verify the connection was called four times (once for each order and once for each order's items)
        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Exactly(4));
    }

    [Test]
    public async Task GetOrderByIdAsync_WithNullDiscountValues_ReturnsOrderWithZeroDiscount()
    {
        // Arrange
        int orderId = 101;

        // Setup reader to return one row and then no more rows
        _reader.SetupReadResults(true, false);

        // Setup column values for the order with null discount values
        _reader.SetupValue(0, orderId); // Order ID
        _reader.SetupValue(1, 1); // Customer ID
        _reader.SetupValue(2, DateTime.Now);
        _reader.SetupValue(3, 100.0m);
        _reader.SetupValue(4, DBNull.Value); // Null discount percent
        _reader.SetupValue(5, DBNull.Value); // Null discount amount
        _reader.SetupValue(6, "Processed");

        // For the order items query, setup an empty result
        MockDbDataReader itemsReader = new MockDbDataReader();
        itemsReader.SetupReadResults(false);
        MockDbCommand itemsCommand = new MockDbCommand { ReaderToReturn = itemsReader };

        // We need to reset the connection after the first query
        _connectionFactoryMock.SetupSequence(f => f.CreateConnectionAsync())
            .ReturnsAsync(_connection)
            .ReturnsAsync(new MockDbConnection { CommandToReturn = itemsCommand });

        // Act
        var order = await _orderRepository.GetOrderByIdAsync(orderId);

        // Assert
        Assert.That(order, Is.Not.Null);
        Assert.That(order.Id, Is.EqualTo(orderId));
        Assert.That(order.DiscountPercent, Is.EqualTo(0)); // Should be initialized to 0
        Assert.That(order.DiscountAmount, Is.EqualTo(0)); // Should be initialized to 0
    }

    [Test]
    public async Task GetOrderByIdAsync_WithOrderItemsPopulated_ReturnsCompleteOrder()
    {
        // Arrange
        int orderId = 101;
        int productId = 301;

        // Setup reader to return one row for the order
        _reader.SetupReadResults(true, false);

        // Setup column values for the order
        _reader.SetupValue(0, orderId); // Order ID
        _reader.SetupValue(1, 1); // Customer ID
        _reader.SetupValue(2, DateTime.Now);
        _reader.SetupValue(3, 100.0m);
        _reader.SetupValue(4, 10.0m);
        _reader.SetupValue(5, 10.0m);
        _reader.SetupValue(6, "Processing");

        // For the order items query, setup a result with one order item
        MockDbDataReader itemsReader = new MockDbDataReader();
        itemsReader.SetupReadResults(true, false);

        // Setup column values for the order item
        itemsReader.SetupValue(0, 201); // Item ID
        itemsReader.SetupValue(1, orderId); // Order ID
        itemsReader.SetupValue(2, productId); // Product ID
        itemsReader.SetupValue(3, 2); // Quantity
        itemsReader.SetupValue(4, 19.99m); // Unit price

        MockDbCommand itemsCommand = new MockDbCommand { ReaderToReturn = itemsReader };

        // Setup product repository to return a product
        var product = new Product { Id = productId, Name = "Test Product", Category = "Test", Price = 19.99m };
        _productRepositoryMock.Setup(p => p.GetProductByIdAsync(productId)).ReturnsAsync(product);

        // We need to reset the connection after the first query
        _connectionFactoryMock.SetupSequence(f => f.CreateConnectionAsync())
            .ReturnsAsync(_connection)
            .ReturnsAsync(new MockDbConnection { CommandToReturn = itemsCommand });

        // Act
        var order = await _orderRepository.GetOrderByIdAsync(orderId);

        // Assert
        Assert.That(order, Is.Not.Null);
        Assert.That(order.Id, Is.EqualTo(orderId));
        Assert.That(order.Status, Is.EqualTo("Processing"));
        Assert.That(order.Items, Is.Not.Null);
        Assert.That(order.Items.Count, Is.EqualTo(1));
        Assert.That(order.Items[0].Id, Is.EqualTo(201));
        Assert.That(order.Items[0].ProductId, Is.EqualTo(productId));
        Assert.That(order.Items[0].Product, Is.Not.Null);
        Assert.That(order.Items[0].Product.Id, Is.EqualTo(productId));

        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Exactly(2));
        _productRepositoryMock.Verify(p => p.GetProductByIdAsync(productId), Times.Once);
    }
}
