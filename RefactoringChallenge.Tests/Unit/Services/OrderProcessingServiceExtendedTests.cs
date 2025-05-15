using Microsoft.Extensions.Logging;
using Moq;
using RefactoringChallenge.Models;
using RefactoringChallenge.Repositories;
using RefactoringChallenge.Services;

namespace RefactoringChallenge.Tests.Unit.Services;

[TestFixture]
public class OrderProcessingServiceExtendedTests
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
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _discountServiceMock = new Mock<IDiscountService>();
        _loggerMock = new Mock<ILogger<OrderProcessingService>>();

        _orderProcessingService = new OrderProcessingService(
            _customerRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _discountServiceMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task ProcessCustomerOrdersAsync_WhenNoOrders_ReturnsEmptyList()
    {
        // Arrange
        int customerId = 1;
        var customer = new Customer { Id = customerId, Name = "Test Customer", IsVip = false };

        _customerRepositoryMock.Setup(r => r.GetCustomerByIdAsync(customerId))
            .ReturnsAsync(customer);

        _orderRepositoryMock.Setup(r => r.GetPendingOrdersByCustomerIdAsync(customerId))
            .ReturnsAsync(new List<Order>());

        // Act
        var result = await _orderProcessingService.ProcessCustomerOrdersAsync(customerId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);

        _customerRepositoryMock.Verify(r => r.GetCustomerByIdAsync(customerId), Times.Once);
        _orderRepositoryMock.Verify(r => r.GetPendingOrdersByCustomerIdAsync(customerId), Times.Once);
    }

    [Test]
    public async Task ProcessCustomerOrdersAsync_WithInsufficientInventory_MarksOrderOnHold()
    {
        // Arrange
        int customerId = 1;
        var customer = new Customer { Id = customerId, Name = "Test Customer", IsVip = false };

        var orderItem = new OrderItem
        {
            Id = 1,
            OrderId = 101,
            ProductId = 201,
            Quantity = 5,
            UnitPrice = 10.0m
        };

        var order = new Order
        {
            Id = 101,
            CustomerId = customerId,
            Status = "Pending",
            Items = new List<OrderItem> { orderItem },
            TotalAmount = 50.0m
        };

        _customerRepositoryMock.Setup(r => r.GetCustomerByIdAsync(customerId))
            .ReturnsAsync(customer);

        _orderRepositoryMock.Setup(r => r.GetPendingOrdersByCustomerIdAsync(customerId))
            .ReturnsAsync(new List<Order> { order });

        _orderRepositoryMock.Setup(r => r.GetOrderItemsByOrderIdAsync(order.Id))
            .ReturnsAsync(order.Items);

        _discountServiceMock.Setup(s => s.CalculateDiscountPercentage(customer, 50.0m))
            .Returns(5.0m);

        // Insufficient stock
        _inventoryRepositoryMock.Setup(r => r.GetStockQuantityByProductIdAsync(orderItem.ProductId))
            .ReturnsAsync(3); // Less than the 5 requested

        _orderRepositoryMock.Setup(r => r.UpdateOrderAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        // Note: These methods likely don't return values in the actual implementation
        // Adjust as needed based on the actual interface

        // Act
        var result = await _orderProcessingService.ProcessCustomerOrdersAsync(customerId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Status, Is.EqualTo("OnHold"));

        _inventoryRepositoryMock.Verify(r => r.GetStockQuantityByProductIdAsync(orderItem.ProductId), Times.Once);
        // Verify inventory was not updated since it was insufficient
        _inventoryRepositoryMock.Verify(r => r.UpdateStockQuantityAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task ProcessCustomerOrdersAsync_WithInvalidCustomerId_ThrowsArgumentException()
    {
        // Arrange
        int invalidCustomerId = -1;

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _orderProcessingService.ProcessCustomerOrdersAsync(invalidCustomerId));

        Assert.That(ex.Message, Does.Contain("must be a positive number"));
    }

    [Test]
    public async Task ProcessCustomerOrdersAsync_WhenCustomerNotFound_ThrowsException()
    {
        // Arrange
        int customerId = 999;

        _customerRepositoryMock.Setup(r => r.GetCustomerByIdAsync(customerId))
            .ReturnsAsync((Customer)null!);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _orderProcessingService.ProcessCustomerOrdersAsync(customerId));

        Assert.That(ex.Message, Does.Contain("not found"));
    }

    [Test]
    public void Constructor_WithNullDependencies_ThrowsArgumentNullException()
    {
        // Assert for each dependency
        Assert.Throws<ArgumentNullException>(() => new OrderProcessingService(
            null!,
            _orderRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _discountServiceMock.Object,
            _loggerMock.Object));

        Assert.Throws<ArgumentNullException>(() => new OrderProcessingService(
            _customerRepositoryMock.Object,
            null!,
            _inventoryRepositoryMock.Object,
            _discountServiceMock.Object,
            _loggerMock.Object));

        Assert.Throws<ArgumentNullException>(() => new OrderProcessingService(
            _customerRepositoryMock.Object,
            _orderRepositoryMock.Object,
            null!,
            _discountServiceMock.Object,
            _loggerMock.Object));

        Assert.Throws<ArgumentNullException>(() => new OrderProcessingService(
            _customerRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            null!,
            _loggerMock.Object));

        Assert.Throws<ArgumentNullException>(() => new OrderProcessingService(
            _customerRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _discountServiceMock.Object,
            null!));
    }
}
