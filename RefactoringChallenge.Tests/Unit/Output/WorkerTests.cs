using Microsoft.Extensions.Logging;
using Moq;
using RefactoringChallenge.Models;
using RefactoringChallenge.Output;
using RefactoringChallenge.Services;

namespace RefactoringChallenge.Tests.Unit.Output;

[TestFixture]
public class WorkerTests
{
    private Mock<ILogger<Worker>> _loggerMock = null!;
    private Mock<IOrderProcessingService> _orderProcessingServiceMock = null!;
    private TestableWorker _worker = null!;
    private CancellationToken _stoppingToken;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<Worker>>();
        _orderProcessingServiceMock = new Mock<IOrderProcessingService>();
        _worker = new TestableWorker(_loggerMock.Object, _orderProcessingServiceMock.Object);
        _stoppingToken = new CancellationToken();
    }

    [TearDown]
    public void TearDown()
    {
        _worker.Dispose();
    }

    /// <summary>
    /// A testable version of Worker that exposes the protected ExecuteAsync method
    /// </summary>
    private class TestableWorker : Worker
    {
        public TestableWorker(ILogger<Worker> logger, IOrderProcessingService orderProcessingService)
            : base(logger, orderProcessingService)
        {
        }

        // Expose the protected method
        public new Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return base.ExecuteAsync(stoppingToken);
        }
    }

    [Test]
    public async Task ExecuteAsync_ProcessesCustomerOrder_Successfully()
    {
        // Arrange
        var mockOrders = new List<Order>
        {
            new Order
            {
                Id = 1,
                CustomerId = 1,
                Status = "Ready",
                TotalAmount = 1000M,
                DiscountPercent = 10
            },
            new Order
            {
                Id = 2,
                CustomerId = 1,
                Status = "Ready",
                TotalAmount = 500M,
                DiscountPercent = 5
            }
        };

        _orderProcessingServiceMock.Setup(x => x.ProcessCustomerOrdersAsync(1))
            .ReturnsAsync(mockOrders);

        // Act
        await _worker.ExecuteAsync(_stoppingToken);

        // Assert
        _orderProcessingServiceMock.Verify(x => x.ProcessCustomerOrdersAsync(1), Times.Once);

        // Verify logs were written
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Worker started at:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processed 2 orders for customer ID: 1")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Worker completed at:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_HandlesException_GracefullyAndLogsError()
    {
        // Arrange
        _orderProcessingServiceMock.Setup(x => x.ProcessCustomerOrdersAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        await _worker.ExecuteAsync(_stoppingToken);

        // Assert
        _orderProcessingServiceMock.Verify(x => x.ProcessCustomerOrdersAsync(1), Times.Once);

        // Verify error was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred while processing orders")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new TestableWorker(null!, _orderProcessingServiceMock.Object));

        Assert.That(exception.ParamName, Is.EqualTo("logger"));
    }

    [Test]
    public void Constructor_WithNullOrderProcessingService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new TestableWorker(_loggerMock.Object, null!));

        Assert.That(exception.ParamName, Is.EqualTo("orderProcessingService"));
    }
}
