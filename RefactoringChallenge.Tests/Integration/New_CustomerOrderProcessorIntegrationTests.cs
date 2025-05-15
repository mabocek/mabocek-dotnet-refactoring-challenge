using Moq;
using NUnit.Framework;
using RefactoringChallenge.Models;
using RefactoringChallenge.Output;
using RefactoringChallenge.Repositories;
using RefactoringChallenge.Services;

namespace RefactoringChallenge.Tests.Integration;

[TestFixture]
public class New_CustomerOrderProcessorIntegrationTests
{
    private Mock<IOrderProcessingService> _orderProcessingServiceMock;
    private Mock<ICustomerRepository> _customerRepositoryMock;
    private Mock<IOrderRepository> _orderRepositoryMock;
    private Mock<IInventoryRepository> _inventoryRepositoryMock;
    private Mock<IProductRepository> _productRepositoryMock;
    private Mock<IDiscountService> _discountServiceMock;

    private New_CustomerOrderProcessor _processor;

    [SetUp]
    public void SetUp()
    {
        // Setup mocks for all dependencies
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _discountServiceMock = new Mock<IDiscountService>();
        _orderProcessingServiceMock = new Mock<IOrderProcessingService>();

        // Create the processor with mocked order processing service
        _processor = new New_CustomerOrderProcessor(_orderProcessingServiceMock.Object);
    }

    [Test]
    public async Task ProcessCustomerOrdersAsync_WithValidData_ShouldSucceed()
    {
        // Arrange
        int customerId = 1;
        var customer = new Customer
        {
            Id = customerId,
            Name = "Test Customer",
            Email = "test@example.com",
            IsVip = true,
            RegistrationDate = new DateTime(2020, 1, 1)
        };

        var pendingOrder = new Order
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
                    ProductId = 101,
                    Quantity = 2,
                    UnitPrice = 50m,
                    Product = new Product
                    {
                        Id = 101,
                        Name = "Test Product",
                        Category = "Test Category",
                        Price = 50m
                    }
                }
            }
        };

        var processedOrder = new Order
        {
            Id = 1,
            CustomerId = customerId,
            OrderDate = DateTime.Now,
            Status = "Ready",
            TotalAmount = 90m,
            DiscountPercent = 10m,
            DiscountAmount = 10m,
            Items = pendingOrder.Items
        };

        var expectedOrders = new List<Order> { processedOrder };

        // Setup mocks
        _orderProcessingServiceMock.Setup(s => s.ProcessCustomerOrdersAsync(customerId))
            .ReturnsAsync(expectedOrders);

        // Act
        var result = await _processor.ProcessCustomerOrdersAsync(customerId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Status, Is.EqualTo("Ready"));
        Assert.That(result[0].TotalAmount, Is.EqualTo(90m));
        Assert.That(result[0].DiscountPercent, Is.EqualTo(10m));

        _orderProcessingServiceMock.Verify(s => s.ProcessCustomerOrdersAsync(customerId), Times.Once);
    }

    [Test]
    public void ProcessCustomerOrders_WithEmptyResults_ShouldReturnEmptyList()
    {
        // Arrange
        int customerId = 999; // Non-existent customer
        var emptyList = new List<Order>();

        _orderProcessingServiceMock.Setup(s => s.ProcessCustomerOrdersAsync(customerId))
            .ReturnsAsync(emptyList);

        // Act
        var result = _processor.ProcessCustomerOrders(customerId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }
}
