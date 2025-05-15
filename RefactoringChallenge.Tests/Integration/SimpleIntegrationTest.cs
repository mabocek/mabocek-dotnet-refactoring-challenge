using Microsoft.Extensions.Logging;
using Moq;
using RefactoringChallenge.Models;
using RefactoringChallenge.Orchestration.Services;

namespace RefactoringChallenge.Tests.Integration;

/// <summary>
/// Simple integration test focusing on core functionality without database dependencies
/// using mock objects where appropriate
/// </summary>
[TestFixture]
[Category("Integration")]
public class SimpleIntegrationTest
{
    [Test]
    public void DiscountService_CalculateDiscountPercentage_Test()
    {
        // Create a logger mock
        var loggerMock = new Mock<ILogger<DiscountService>>();

        // Create the discount service
        var discountService = new DiscountService(loggerMock.Object);

        // Create test customer
        var customer = new Customer
        {
            Id = 1,
            Name = "Test Customer",
            Email = "test@example.com",
            IsVip = true,
            RegistrationDate = DateTime.Now.AddYears(-3) // 3 years old customer
        };

        // Test with VIP customer and large order
        var discount = discountService.CalculateDiscountPercentage(customer, 1000m);

        // VIP should get 10% discount plus 2% for being a customer for 3 years
        Assert.That(discount, Is.EqualTo(12m));
    }

    [Test]
    public void DiscountService_MaximumDiscount_Test()
    {
        // Create a logger mock
        var loggerMock = new Mock<ILogger<DiscountService>>();

        // Create the discount service
        var discountService = new DiscountService(loggerMock.Object);

        // Create VIP long-time customer
        var vipLongTimeCustomer = new Customer
        {
            Id = 2,
            Name = "VIP Long-time Customer",
            Email = "vip@example.com",
            IsVip = true,
            RegistrationDate = DateTime.Now.AddYears(-10) // 10 years old customer
        };

        // Test with VIP customer and very large order
        var discount = discountService.CalculateDiscountPercentage(vipLongTimeCustomer, 15000m);

        // VIP (10%) + 5+ years loyalty (5%) + large order (15%) = 30%, but should be capped at 25%
        Assert.That(discount, Is.EqualTo(25m));
    }

    [Test]
    public void DiscountService_NewNonVipCustomer_Test()
    {
        // Create a logger mock
        var loggerMock = new Mock<ILogger<DiscountService>>();

        // Create the discount service
        var discountService = new DiscountService(loggerMock.Object);

        // Create new non-VIP customer
        var newCustomer = new Customer
        {
            Id = 3,
            Name = "New Customer",
            Email = "new@example.com",
            IsVip = false,
            RegistrationDate = DateTime.Now.AddDays(-10) // Very new customer
        };

        // Test with new non-VIP customer with small order - should get no discount
        var smallOrderDiscount = discountService.CalculateDiscountPercentage(newCustomer, 500m);
        Assert.That(smallOrderDiscount, Is.EqualTo(0m));

        // Test with new non-VIP customer with medium order - should get order size discount
        var mediumOrderDiscount = discountService.CalculateDiscountPercentage(newCustomer, 2000m);
        Assert.That(mediumOrderDiscount, Is.EqualTo(5m));
    }

    [Test]
    public void DiscountService_LoyaltyDiscount_Test()
    {
        // Create a logger mock
        var loggerMock = new Mock<ILogger<DiscountService>>();

        // Create the discount service
        var discountService = new DiscountService(loggerMock.Object);

        // Create test customers with different ages
        var twoYearsCustomer = new Customer { Id = 1, IsVip = false, RegistrationDate = DateTime.Now.AddYears(-2) };
        var fiveYearsCustomer = new Customer { Id = 2, IsVip = false, RegistrationDate = DateTime.Now.AddYears(-5) };

        // Test loyalty discounts (small order amount to ensure no order-based discount)
        var twoYearsDiscount = discountService.CalculateDiscountPercentage(twoYearsCustomer, 500m);
        var fiveYearsDiscount = discountService.CalculateDiscountPercentage(fiveYearsCustomer, 500m);

        // Verify correct loyalty discounts
        Assert.That(twoYearsDiscount, Is.EqualTo(2m)); // 2 years = 2%
        Assert.That(fiveYearsDiscount, Is.EqualTo(5m)); // 5 years = 5%
    }

    [Test]
    public void DiscountService_OrderSizeDiscount_Test()
    {
        // Create a logger mock
        var loggerMock = new Mock<ILogger<DiscountService>>();

        // Create the discount service
        var discountService = new DiscountService(loggerMock.Object);

        // Create a new non-VIP customer (no VIP or loyalty discounts)
        var newCustomer = new Customer { Id = 1, IsVip = false, RegistrationDate = DateTime.Now.AddDays(-10) };

        // Test different order amounts
        var smallOrderDiscount = discountService.CalculateDiscountPercentage(newCustomer, 500m); // < 1000
        var mediumOrderDiscount = discountService.CalculateDiscountPercentage(newCustomer, 3000m); // > 1000, < 5000
        var largeOrderDiscount = discountService.CalculateDiscountPercentage(newCustomer, 6000m); // > 5000, < 10000
        var veryLargeOrderDiscount = discountService.CalculateDiscountPercentage(newCustomer, 15000m); // > 10000

        // Verify correct order size discounts
        Assert.That(smallOrderDiscount, Is.EqualTo(0m)); // No discount for small orders
        Assert.That(mediumOrderDiscount, Is.EqualTo(5m)); // 5% for orders > 1000
        Assert.That(largeOrderDiscount, Is.EqualTo(10m)); // 10% for orders > 5000
        Assert.That(veryLargeOrderDiscount, Is.EqualTo(15m)); // 15% for orders > 10000
    }
}
