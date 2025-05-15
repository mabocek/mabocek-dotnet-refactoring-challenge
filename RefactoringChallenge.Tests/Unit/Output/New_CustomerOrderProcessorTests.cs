using Moq;
using RefactoringChallenge.Models;
using RefactoringChallenge.Output;
using RefactoringChallenge.Services;

namespace RefactoringChallenge.Tests.Unit.Output;

[TestFixture]
public class New_CustomerOrderProcessorTests
{
    private Mock<IOrderProcessingService> _orderProcessingServiceMock;
    private New_CustomerOrderProcessor _processor;

    [SetUp]
    public void SetUp()
    {
        _orderProcessingServiceMock = new Mock<IOrderProcessingService>();
        _processor = new New_CustomerOrderProcessor(_orderProcessingServiceMock.Object);
    }

    [Test]
    public void ProcessCustomerOrders_ShouldCallOrderProcessingService()
    {
        // Arrange
        int customerId = 1;
        var expectedOrders = new List<Order> { new Order { Id = 1, CustomerId = 1 } };
        _orderProcessingServiceMock.Setup(s => s.ProcessCustomerOrdersAsync(customerId))
            .ReturnsAsync(expectedOrders);

        // Act
        var result = _processor.ProcessCustomerOrders(customerId);

        // Assert
        Assert.That(result, Is.SameAs(expectedOrders));
        _orderProcessingServiceMock.Verify(s => s.ProcessCustomerOrdersAsync(customerId), Times.Once);
    }

    [Test]
    public void ProcessCustomerOrders_WithInvalidId_ShouldThrowArgumentException()
    {
        // Arrange
        int invalidCustomerId = 0;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _processor.ProcessCustomerOrders(invalidCustomerId));
        Assert.That(ex.ParamName, Is.EqualTo("customerId"));
    }

    [Test]
    public async Task ProcessCustomerOrdersAsync_ShouldCallOrderProcessingService()
    {
        // Arrange
        int customerId = 1;
        var expectedOrders = new List<Order> { new Order { Id = 1, CustomerId = 1 } };
        _orderProcessingServiceMock.Setup(s => s.ProcessCustomerOrdersAsync(customerId))
            .ReturnsAsync(expectedOrders);

        // Act
        var result = await _processor.ProcessCustomerOrdersAsync(customerId);

        // Assert
        Assert.That(result, Is.SameAs(expectedOrders));
        _orderProcessingServiceMock.Verify(s => s.ProcessCustomerOrdersAsync(customerId), Times.Once);
    }

    [Test]
    public void ProcessCustomerOrdersAsync_WithInvalidId_ShouldThrowArgumentException()
    {
        // Arrange
        int invalidCustomerId = -1;

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _processor.ProcessCustomerOrdersAsync(invalidCustomerId));
        Assert.That(ex.ParamName, Is.EqualTo("customerId"));
    }
}
