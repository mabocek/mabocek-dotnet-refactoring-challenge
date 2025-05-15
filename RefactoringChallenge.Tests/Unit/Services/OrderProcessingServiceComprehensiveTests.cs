using Microsoft.Extensions.Logging;
using Moq;
using RefactoringChallenge.Models;
using RefactoringChallenge.Repositories;
using RefactoringChallenge.Services;

namespace RefactoringChallenge.Tests.Unit.Services;

/// <summary>
/// Comprehensive unit tests for OrderProcessingService focusing on the service logic
/// without trying to mock database operations directly
/// </summary>
[TestFixture]
public class OrderProcessingServiceComprehensiveTests
{
    private OrderProcessingService _orderProcessingService = null!;
    private Mock<ICustomerRepository> _customerRepositoryMock = null!;
    private Mock<IOrderRepository> _orderRepositoryMock = null!;
    private Mock<IInventoryRepository> _inventoryRepositoryMock = null!;
    private Mock<IDiscountService> _discountServiceMock = null!;
    private Mock<ILogger<OrderProcessingService>> _loggerMock = null!;

    [SetUp]
    public void Setup()
    {
        // Setup repository mocks
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _discountServiceMock = new Mock<IDiscountService>();
        _loggerMock = new Mock<ILogger<OrderProcessingService>>();

        // Create the service with mocked dependencies
        _orderProcessingService = new OrderProcessingService(
            _customerRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _discountServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Test]
    public void Constructor_WithNullParameters_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OrderProcessingService(
            null,
            _orderRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _discountServiceMock.Object,
            _loggerMock.Object
        ));

        Assert.Throws<ArgumentNullException>(() => new OrderProcessingService(
            _customerRepositoryMock.Object,
            null,
            _inventoryRepositoryMock.Object,
            _discountServiceMock.Object,
            _loggerMock.Object
        ));

        Assert.Throws<ArgumentNullException>(() => new OrderProcessingService(
            _customerRepositoryMock.Object,
            _orderRepositoryMock.Object,
            null,
            _discountServiceMock.Object,
            _loggerMock.Object
        ));

        Assert.Throws<ArgumentNullException>(() => new OrderProcessingService(
            _customerRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            null,
            _loggerMock.Object
        ));

        Assert.Throws<ArgumentNullException>(() => new OrderProcessingService(
            _customerRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _discountServiceMock.Object,
            null
        ));
    }

    [Test]
    public void ProcessCustomerOrdersAsync_InvalidCustomerId_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(
            () => _orderProcessingService.ProcessCustomerOrdersAsync(0));

        Assert.That(exception.Message, Does.Contain("Customer ID must be a positive number"));

        exception = Assert.ThrowsAsync<ArgumentException>(
            () => _orderProcessingService.ProcessCustomerOrdersAsync(-1));

        Assert.That(exception.Message, Does.Contain("Customer ID must be a positive number"));
    }

    [Test]
    public void ProcessCustomerOrdersAsync_NullOrMissingCustomer_ThrowsException()
    {
        // Arrange
        int customerId = 999;
        _customerRepositoryMock.Setup(repo => repo.GetCustomerByIdAsync(customerId))
            .ReturnsAsync((Customer)null!);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            () => _orderProcessingService.ProcessCustomerOrdersAsync(customerId));

        Assert.That(exception.Message, Does.Contain($"Customer with ID {customerId} not found"));
    }

    [Test]
    public async Task ProcessCustomerOrdersAsync_NoOrders_CompletesWithoutError()
    {
        // Arrange
        int customerId = 1;
        var customer = new Customer { Id = customerId, Name = "Test Customer" };

        _customerRepositoryMock.Setup(repo => repo.GetCustomerByIdAsync(customerId))
            .ReturnsAsync(customer);

        _orderRepositoryMock.Setup(repo => repo.GetPendingOrdersByCustomerIdAsync(customerId))
            .ReturnsAsync(new List<Order>());

        // Act
        var result = await _orderProcessingService.ProcessCustomerOrdersAsync(customerId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));
        _orderRepositoryMock.Verify(repo => repo.GetPendingOrdersByCustomerIdAsync(customerId), Times.Once);
        _orderRepositoryMock.Verify(repo => repo.UpdateOrderStatusAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ProcessCustomerOrdersAsync_WithSufficientInventory_ProcessesOrdersSuccessfully()
    {
        // Arrange - Setup a customer with pending orders
        int customerId = 1;
        var customer = new Customer { Id = customerId, Name = "Test Customer", IsVip = true };

        var orderItems1 = new List<OrderItem>
        {
            new OrderItem { Id = 1, OrderId = 101, ProductId = 201, Quantity = 2, UnitPrice = 100m },
            new OrderItem { Id = 2, OrderId = 101, ProductId = 202, Quantity = 1, UnitPrice = 300m }
        };

        var orderItems2 = new List<OrderItem>
        {
            new OrderItem { Id = 3, OrderId = 102, ProductId = 203, Quantity = 3, UnitPrice = 333.33m }
        };

        var order1 = new Order
        {
            Id = 101,
            CustomerId = customerId,
            Status = "Pending",
            Items = orderItems1
        };

        var order2 = new Order
        {
            Id = 102,
            CustomerId = customerId,
            Status = "Pending",
            Items = orderItems2
        };

        // Setup repository returns
        _customerRepositoryMock.Setup(repo => repo.GetCustomerByIdAsync(customerId))
            .ReturnsAsync(customer);

        _orderRepositoryMock.Setup(repo => repo.GetPendingOrdersByCustomerIdAsync(customerId))
            .ReturnsAsync(new List<Order> { order1, order2 });

        // Setup sufficient inventory
        _inventoryRepositoryMock.Setup(repo => repo.GetStockQuantityByProductIdAsync(201))
            .ReturnsAsync(10); // Plenty of stock
        _inventoryRepositoryMock.Setup(repo => repo.GetStockQuantityByProductIdAsync(202))
            .ReturnsAsync(5);  // Sufficient 
        _inventoryRepositoryMock.Setup(repo => repo.GetStockQuantityByProductIdAsync(203))
            .ReturnsAsync(5);  // More than enough

        // Setup discount calculation
        _discountServiceMock.Setup(svc => svc.CalculateDiscountPercentage(customer, It.IsAny<decimal>()))
            .Returns(15m); // 15% discount

        // Act
        var processedOrders = await _orderProcessingService.ProcessCustomerOrdersAsync(customerId);

        // Assert
        Assert.That(processedOrders, Is.Not.Null);
        Assert.That(processedOrders.Count, Is.EqualTo(2));

        // Verify customer was retrieved once
        _customerRepositoryMock.Verify(repo => repo.GetCustomerByIdAsync(customerId), Times.Once);

        // Verify pending orders were retrieved once
        _orderRepositoryMock.Verify(repo => repo.GetPendingOrdersByCustomerIdAsync(customerId), Times.Once);

        // Verify inventory was checked for each product
        _inventoryRepositoryMock.Verify(repo => repo.GetStockQuantityByProductIdAsync(201), Times.Once);
        _inventoryRepositoryMock.Verify(repo => repo.GetStockQuantityByProductIdAsync(202), Times.Once);
        _inventoryRepositoryMock.Verify(repo => repo.GetStockQuantityByProductIdAsync(203), Times.Once);

        // Verify discounts were calculated - each order once
        _discountServiceMock.Verify(svc => svc.CalculateDiscountPercentage(customer, It.IsAny<decimal>()), Times.Exactly(2));

        // Verify order updates
        _orderRepositoryMock.Verify(repo => repo.UpdateOrderAsync(It.IsAny<Order>()), Times.Exactly(2));

        // Verify order statuses were updated to 'Ready' since inventory is sufficient
        _orderRepositoryMock.Verify(repo => repo.UpdateOrderStatusAsync(101, "Ready"), Times.Once);
        _orderRepositoryMock.Verify(repo => repo.UpdateOrderStatusAsync(102, "Ready"), Times.Once);

        // Verify order logs were added
        _orderRepositoryMock.Verify(repo => repo.AddOrderLogAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Exactly(2));

        // Verify inventory was updated for all products
        _inventoryRepositoryMock.Verify(repo => repo.UpdateStockQuantityAsync(201, It.IsAny<int>()), Times.Once);
        _inventoryRepositoryMock.Verify(repo => repo.UpdateStockQuantityAsync(202, It.IsAny<int>()), Times.Once);
        _inventoryRepositoryMock.Verify(repo => repo.UpdateStockQuantityAsync(203, It.IsAny<int>()), Times.Once);

        // Verify the final status of orders
        Assert.That(processedOrders[0].Status, Is.EqualTo("Ready"));
        Assert.That(processedOrders[1].Status, Is.EqualTo("Ready"));
    }

    [Test]
    public async Task ProcessCustomerOrdersAsync_WithInsufficientInventory_MarksOrderOnHold()
    {
        // Arrange - Setup a customer with a pending order
        int customerId = 1;
        var customer = new Customer { Id = customerId, Name = "Test Customer", IsVip = false };

        var orderItems = new List<OrderItem>
        {
            new OrderItem { Id = 1, OrderId = 101, ProductId = 201, Quantity = 10, UnitPrice = 100m },
            new OrderItem { Id = 2, OrderId = 101, ProductId = 202, Quantity = 10, UnitPrice = 100m }
        };

        var order = new Order
        {
            Id = 101,
            CustomerId = customerId,
            Status = "Pending",
            Items = orderItems
        };

        // Setup repository returns
        _customerRepositoryMock.Setup(repo => repo.GetCustomerByIdAsync(customerId))
            .ReturnsAsync(customer);

        _orderRepositoryMock.Setup(repo => repo.GetPendingOrdersByCustomerIdAsync(customerId))
            .ReturnsAsync(new List<Order> { order });

        // Setup insufficient inventory for the second product
        _inventoryRepositoryMock.Setup(repo => repo.GetStockQuantityByProductIdAsync(201))
            .ReturnsAsync(20); // Sufficient stock
        _inventoryRepositoryMock.Setup(repo => repo.GetStockQuantityByProductIdAsync(202))
            .ReturnsAsync(5);  // Insufficient (need 10)

        // Setup discount calculation - for non-VIP with 2000 order
        _discountServiceMock.Setup(svc => svc.CalculateDiscountPercentage(customer, It.IsAny<decimal>()))
            .Returns(10m); // 10% discount

        // Act
        var processedOrders = await _orderProcessingService.ProcessCustomerOrdersAsync(customerId);

        // Assert
        Assert.That(processedOrders, Is.Not.Null);
        Assert.That(processedOrders.Count, Is.EqualTo(1));

        // Verify inventory was checked for products
        _inventoryRepositoryMock.Verify(repo => repo.GetStockQuantityByProductIdAsync(201), Times.Once);
        _inventoryRepositoryMock.Verify(repo => repo.GetStockQuantityByProductIdAsync(202), Times.Once);

        // Verify discount was calculated
        _discountServiceMock.Verify(svc => svc.CalculateDiscountPercentage(customer, It.IsAny<decimal>()), Times.Once);

        // Verify order was updated
        _orderRepositoryMock.Verify(repo => repo.UpdateOrderAsync(It.IsAny<Order>()), Times.Once);

        // Verify order status was updated to 'OnHold' due to insufficient inventory
        _orderRepositoryMock.Verify(repo => repo.UpdateOrderStatusAsync(101, "OnHold"), Times.Once);

        // Verify order log was added explaining the hold status
        _orderRepositoryMock.Verify(repo => repo.AddOrderLogAsync(101, It.Is<string>(s =>
            s.Contains("hold") && s.Contains("not on stock"))), Times.Once);

        // Verify inventory was NOT updated (since the order is on hold)
        _inventoryRepositoryMock.Verify(repo => repo.UpdateStockQuantityAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);

        // Verify the order is on hold
        Assert.That(processedOrders[0].Status, Is.EqualTo("OnHold"));
    }

    [Test]
    public async Task ProcessCustomerOrdersAsync_WithNoInventoryInfo_MarksOrderOnHold()
    {
        // Arrange - Setup a customer with a pending order
        int customerId = 1;
        var customer = new Customer { Id = customerId, Name = "Test Customer" };

        var orderItems = new List<OrderItem>
        {
            new OrderItem { Id = 1, OrderId = 101, ProductId = 201, Quantity = 1, UnitPrice = 100m }
        };

        var order = new Order
        {
            Id = 101,
            CustomerId = customerId,
            Status = "Pending",
            Items = orderItems
        };

        // Setup repository returns
        _customerRepositoryMock.Setup(repo => repo.GetCustomerByIdAsync(customerId))
            .ReturnsAsync(customer);

        _orderRepositoryMock.Setup(repo => repo.GetPendingOrdersByCustomerIdAsync(customerId))
            .ReturnsAsync(new List<Order> { order });

        // Return null for stock quantity (simulating no inventory record)
        _inventoryRepositoryMock.Setup(repo => repo.GetStockQuantityByProductIdAsync(201))
            .ReturnsAsync((int?)null);

        _discountServiceMock.Setup(svc => svc.CalculateDiscountPercentage(customer, It.IsAny<decimal>()))
            .Returns(0m);

        // Act
        var processedOrders = await _orderProcessingService.ProcessCustomerOrdersAsync(customerId);

        // Assert
        Assert.That(processedOrders.Count, Is.EqualTo(1));

        // Verify inventory was checked
        _inventoryRepositoryMock.Verify(repo => repo.GetStockQuantityByProductIdAsync(201), Times.Once);

        // Verify order status was updated to 'OnHold'
        _orderRepositoryMock.Verify(repo => repo.UpdateOrderStatusAsync(101, "OnHold"), Times.Once);

        // Verify the order is on hold
        Assert.That(processedOrders[0].Status, Is.EqualTo("OnHold"));
    }
}
