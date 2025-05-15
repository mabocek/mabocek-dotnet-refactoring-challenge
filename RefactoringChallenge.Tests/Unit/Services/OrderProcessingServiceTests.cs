using Microsoft.Extensions.Logging;
using Moq;
using RefactoringChallenge.Models;
using RefactoringChallenge.Repositories;
using RefactoringChallenge.Services;

namespace RefactoringChallenge.Tests.Unit.Services;

[TestFixture]
public class OrderProcessingServiceTests
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
    public async Task ProcessCustomerOrdersAsync_ForValidCustomer_ProcessesAllOrders()
    {
        // Arrange
        int customerId = 1;

        var customer = new Customer
        {
            Id = customerId,
            Name = "Test Customer",
            Email = "test@example.com",
            IsVip = true,
            RegistrationDate = DateTime.Now.AddYears(-5)
        };

        var pendingOrders = new List<Order>
        {
            new Order
            {
                Id = 1,
                CustomerId = customerId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = 1,
                        OrderId = 1,
                        ProductId = 1,
                        Quantity = 5,
                        UnitPrice = 100,
                        Product = new Product { Id = 1, Name = "Product 1", Category = "Test", Price = 100 }
                    }
                }
            }
        };

        _customerRepositoryMock.Setup(repo => repo.GetCustomerByIdAsync(customerId))
            .ReturnsAsync(customer);

        _orderRepositoryMock.Setup(repo => repo.GetPendingOrdersByCustomerIdAsync(customerId))
            .ReturnsAsync(pendingOrders);

        _discountServiceMock.Setup(service => service.CalculateDiscountPercentage(customer, 500))
            .Returns(20);

        _inventoryRepositoryMock.Setup(repo => repo.GetStockQuantityByProductIdAsync(1))
            .ReturnsAsync(10);

        // Act
        var result = await _orderProcessingService.ProcessCustomerOrdersAsync(customerId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Status, Is.EqualTo("Ready"));
        Assert.That(result[0].DiscountPercent, Is.EqualTo(20));
        Assert.That(result[0].TotalAmount, Is.EqualTo(400)); // 500 - 20% discount

        // Verify repository calls
        _customerRepositoryMock.Verify(repo => repo.GetCustomerByIdAsync(customerId), Times.Once);
        _orderRepositoryMock.Verify(repo => repo.GetPendingOrdersByCustomerIdAsync(customerId), Times.Once);
        _orderRepositoryMock.Verify(repo => repo.UpdateOrderAsync(It.IsAny<Order>()), Times.Once);
        _orderRepositoryMock.Verify(repo => repo.UpdateOrderStatusAsync(1, "Ready"), Times.Once);
        _orderRepositoryMock.Verify(repo => repo.AddOrderLogAsync(1, It.IsAny<string>()), Times.Once);
        _inventoryRepositoryMock.Verify(repo => repo.UpdateStockQuantityAsync(1, 5), Times.Once);
    }

    [Test]
    public async Task ProcessCustomerOrdersAsync_ForOrderWithInsufficientInventory_SetsOrderOnHold()
    {
        // Arrange
        int customerId = 2;

        var customer = new Customer
        {
            Id = customerId,
            Name = "Test Customer 2",
            Email = "test2@example.com",
            IsVip = false,
            RegistrationDate = DateTime.Now.AddYears(-1)
        };

        var pendingOrders = new List<Order>
        {
            new Order
            {
                Id = 2,
                CustomerId = customerId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = 2,
                        OrderId = 2,
                        ProductId = 2,
                        Quantity = 10,
                        UnitPrice = 50,
                        Product = new Product { Id = 2, Name = "Product 2", Category = "Test", Price = 50 }
                    }
                }
            }
        };

        _customerRepositoryMock.Setup(repo => repo.GetCustomerByIdAsync(customerId))
            .ReturnsAsync(customer);

        _orderRepositoryMock.Setup(repo => repo.GetPendingOrdersByCustomerIdAsync(customerId))
            .ReturnsAsync(pendingOrders);

        _discountServiceMock.Setup(service => service.CalculateDiscountPercentage(customer, 500))
            .Returns(5);

        // Insufficient inventory
        _inventoryRepositoryMock.Setup(repo => repo.GetStockQuantityByProductIdAsync(2))
            .ReturnsAsync(5); // Only 5 available, need 10

        // Act
        var result = await _orderProcessingService.ProcessCustomerOrdersAsync(customerId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Status, Is.EqualTo("OnHold"));

        // Verify repository calls
        _orderRepositoryMock.Verify(repo => repo.UpdateOrderStatusAsync(2, "OnHold"), Times.Once);
        _orderRepositoryMock.Verify(repo => repo.AddOrderLogAsync(2, "Order on hold. Some items are not on stock."), Times.Once);
        _inventoryRepositoryMock.Verify(repo => repo.UpdateStockQuantityAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void ProcessCustomerOrdersAsync_ForInvalidCustomerId_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _orderProcessingService.ProcessCustomerOrdersAsync(0));

        Assert.That(exception!.Message, Does.Contain("Customer ID must be a positive number"));

        _customerRepositoryMock.Verify(repo => repo.GetCustomerByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void ProcessCustomerOrdersAsync_ForNonExistentCustomer_ThrowsInvalidOperationException()
    {
        // Arrange
        int customerId = 999;
        _customerRepositoryMock.Setup(repo => repo.GetCustomerByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _orderProcessingService.ProcessCustomerOrdersAsync(customerId));

        Assert.That(exception!.Message, Does.Contain($"Customer with ID {customerId} not found"));

        _customerRepositoryMock.Verify(repo => repo.GetCustomerByIdAsync(customerId), Times.Once);
        _orderRepositoryMock.Verify(repo => repo.GetPendingOrdersByCustomerIdAsync(It.IsAny<int>()), Times.Never);
    }
}
