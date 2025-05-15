using Microsoft.Extensions.Logging;
using Moq;
using RefactoringChallenge.Factories;
using RefactoringChallenge.Repositories;
using RefactoringChallenge.Services;
using RefactoringChallenge.Orchestration.Services;
using RefactoringChallenge.Orchestration.Repositories;

namespace RefactoringChallenge.Tests.Unit.Services;

[TestFixture]
public class IntegratedServicesTests
{
    private OrderProcessingService _orderProcessingService = null!;
    private DiscountService _discountService = null!;
    private CustomerRepository _customerRepository = null!;
    private OrderRepository _orderRepository = null!;
    private ProductRepository _productRepository = null!;
    private InventoryRepository _inventoryRepository = null!;

    private Mock<IDatabaseConnectionFactory> _connectionFactoryMock = null!;
    private Mock<ILogger<OrderProcessingService>> _orderProcessingLoggerMock = null!;
    private Mock<ILogger<DiscountService>> _discountServiceLoggerMock = null!;

    [SetUp]
    public void Setup()
    {
        _connectionFactoryMock = new Mock<IDatabaseConnectionFactory>();
        _orderProcessingLoggerMock = new Mock<ILogger<OrderProcessingService>>();
        _discountServiceLoggerMock = new Mock<ILogger<DiscountService>>();

        // Create real instances instead of mocks
        _discountService = new DiscountService(_discountServiceLoggerMock.Object);
        _productRepository = new ProductRepository(_connectionFactoryMock.Object);
        _customerRepository = new CustomerRepository(_connectionFactoryMock.Object);
        _inventoryRepository = new InventoryRepository(_connectionFactoryMock.Object);
        _orderRepository = new OrderRepository(_connectionFactoryMock.Object, _productRepository);

        _orderProcessingService = new OrderProcessingService(
            _customerRepository,
            _orderRepository,
            _inventoryRepository,
            _discountService,
            _orderProcessingLoggerMock.Object);
    }

    [Test]
    public void ValidateServiceInstantiation_Success()
    {
        // Just verify that all services can be instantiated without errors
        Assert.That(_orderProcessingService, Is.Not.Null);
        Assert.That(_discountService, Is.Not.Null);
        Assert.That(_customerRepository, Is.Not.Null);
        Assert.That(_orderRepository, Is.Not.Null);
        Assert.That(_productRepository, Is.Not.Null);
        Assert.That(_inventoryRepository, Is.Not.Null);
    }

    [Test]
    public void ServiceDependencies_AreCorrectlyInjected()
    {
        // Use reflection to verify dependency injection if needed
        var orderProcessingServiceType = _orderProcessingService.GetType();
        var fields = orderProcessingServiceType.GetFields(
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        var dependencyTypes = new List<System.Type> {
            typeof(ICustomerRepository),
            typeof(IOrderRepository),
            typeof(IInventoryRepository),
            typeof(IDiscountService),
            typeof(ILogger<OrderProcessingService>)
        };

        foreach (var field in fields)
        {
            var fieldType = field.FieldType;
            if (dependencyTypes.Contains(fieldType))
            {
                dependencyTypes.Remove(fieldType);
            }
        }

        // All dependencies should have been found and removed from the list
        Assert.That(dependencyTypes, Is.Empty, "Some dependencies were not properly injected");
    }
}
