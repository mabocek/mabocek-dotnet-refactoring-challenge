using Microsoft.Extensions.Logging;
using Moq;
using RefactoringChallenge.Models;
using RefactoringChallenge.Orchestration.Services;

namespace RefactoringChallenge.Tests.Unit.Services;

[TestFixture]
public class DiscountServiceTests
{
    private Mock<ILogger<DiscountService>> _loggerMock = null!;
    private DiscountService _discountService = null!;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<DiscountService>>();
        _discountService = new DiscountService(_loggerMock.Object);
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DiscountService(null!));
    }

    [Test]
    public void CalculateDiscountPercentage_WithNullCustomer_ThrowsArgumentNullException()
    {
        // Arrange
        Customer? customer = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _discountService.CalculateDiscountPercentage(customer!, 100m));
    }

    [Test]
    public void CalculateDiscountPercentage_WithNegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "Test Customer",
            Email = "test@example.com",
            IsVip = false,
            RegistrationDate = DateTime.UtcNow.AddDays(-30)
        };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            _discountService.CalculateDiscountPercentage(customer, -100m));

        Assert.That(ex!.Message, Contains.Substring("Order amount cannot be negative"));
    }

    [Test]
    public void CalculateDiscountPercentage_ForNewNonVipCustomer_WithSmallOrder_ReturnsZeroDiscount()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "New Customer",
            Email = "new@example.com",
            IsVip = false,
            RegistrationDate = DateTime.UtcNow.AddDays(-30) // 30 days old
        };

        // Act
        var result = _discountService.CalculateDiscountPercentage(customer, 500m);

        // Assert
        Assert.That(result, Is.EqualTo(0m));
    }

    [Test]
    public void CalculateDiscountPercentage_ForVipCustomer_AppliesTenPercentDiscount()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "VIP Customer",
            Email = "vip@example.com",
            IsVip = true,
            RegistrationDate = DateTime.UtcNow.AddDays(-30) // 30 days old
        };

        // Act
        var result = _discountService.CalculateDiscountPercentage(customer, 500m);

        // Assert
        Assert.That(result, Is.EqualTo(10m));
    }

    [Test]
    public void CalculateDiscountPercentage_ForCustomerWithTwoYearsLoyalty_AppliesTwoPercentDiscount()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "Loyal Customer",
            Email = "loyal@example.com",
            IsVip = false,
            RegistrationDate = DateTime.UtcNow.AddYears(-2).AddDays(-1) // Just over 2 years
        };

        // Act
        var result = _discountService.CalculateDiscountPercentage(customer, 500m);

        // Assert
        Assert.That(result, Is.EqualTo(2m));
    }

    [Test]
    public void CalculateDiscountPercentage_ForCustomerWithFiveYearsLoyalty_AppliesFivePercentDiscount()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "Very Loyal Customer",
            Email = "veryloyal@example.com",
            IsVip = false,
            RegistrationDate = DateTime.UtcNow.AddYears(-5).AddDays(-1) // Just over 5 years
        };

        // Act
        var result = _discountService.CalculateDiscountPercentage(customer, 500m);

        // Assert
        Assert.That(result, Is.EqualTo(5m));
    }

    [Test]
    public void CalculateDiscountPercentage_ForOrderOver1000_AppliesFivePercentDiscount()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "Regular Customer",
            Email = "regular@example.com",
            IsVip = false,
            RegistrationDate = DateTime.UtcNow.AddDays(-30) // 30 days old
        };

        // Act
        var result = _discountService.CalculateDiscountPercentage(customer, 1001m);

        // Assert
        Assert.That(result, Is.EqualTo(5m));
    }

    [Test]
    public void CalculateDiscountPercentage_ForOrderOver5000_AppliesTenPercentDiscount()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "Regular Customer",
            Email = "regular@example.com",
            IsVip = false,
            RegistrationDate = DateTime.UtcNow.AddDays(-30) // 30 days old
        };

        // Act
        var result = _discountService.CalculateDiscountPercentage(customer, 5001m);

        // Assert
        Assert.That(result, Is.EqualTo(10m));
    }

    [Test]
    public void CalculateDiscountPercentage_ForOrderOver10000_AppliesFifteenPercentDiscount()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "Regular Customer",
            Email = "regular@example.com",
            IsVip = false,
            RegistrationDate = DateTime.UtcNow.AddDays(-30) // 30 days old
        };

        // Act
        var result = _discountService.CalculateDiscountPercentage(customer, 10001m);

        // Assert
        Assert.That(result, Is.EqualTo(15m));
    }

    [Test]
    public void CalculateDiscountPercentage_ForCombinedDiscounts_AppliesSumOfDiscounts()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "VIP Loyal Customer",
            Email = "viployal@example.com",
            IsVip = true, // 10% discount
            RegistrationDate = DateTime.UtcNow.AddYears(-3) // 3 years = 2% discount
        };

        // Act
        var result = _discountService.CalculateDiscountPercentage(customer, 6000m); // 10% discount for order value

        // Assert
        var expectedDiscount = 10m + 2m + 10m; // VIP + loyalty + order value
        Assert.That(result, Is.EqualTo(expectedDiscount));
    }

    [Test]
    public void CalculateDiscountPercentage_WhenDiscountExceedsMaximum_CapsDiscountAtMaximum()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "VIP Long-term Customer",
            Email = "viplong@example.com",
            IsVip = true, // 10% discount
            RegistrationDate = DateTime.UtcNow.AddYears(-6) // 6 years = 5% discount
        };

        // Act
        var result = _discountService.CalculateDiscountPercentage(customer, 11000m); // 15% discount for order value

        // Assert
        // Total would be 30% (10% + 5% + 15%), but should be capped at 25%
        Assert.That(result, Is.EqualTo(25m));
    }

    [Test]
    public void CalculateDiscountPercentage_WithAllPossibleDiscounts_LogsCorrectInformation()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "VIP Long-term Customer",
            Email = "viplong@example.com",
            IsVip = true, // 10% discount
            RegistrationDate = DateTime.UtcNow.AddYears(-6) // 6 years = 5% discount
        };

        // Act
        _discountService.CalculateDiscountPercentage(customer, 11000m); // 15% discount for order value

        // Assert
        // Verify that logging was called with appropriate arguments
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Discount capped at 25%")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Final discount for customer 1: 25%")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
