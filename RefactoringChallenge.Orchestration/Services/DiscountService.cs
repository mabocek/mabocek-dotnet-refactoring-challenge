using Microsoft.Extensions.Logging;
using RefactoringChallenge.Models;
using RefactoringChallenge.Services;

namespace RefactoringChallenge.Orchestration.Services;

public class DiscountService : IDiscountService
{
    private readonly ILogger<DiscountService> _logger;

    public DiscountService(ILogger<DiscountService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public decimal CalculateDiscountPercentage(Customer customer, decimal totalAmount)
    {
        if (customer == null)
        {
            throw new ArgumentNullException(nameof(customer));
        }

        if (totalAmount < 0)
        {
            throw new ArgumentException("Order amount cannot be negative", nameof(totalAmount));
        }

        const decimal MaxDiscountPercent = 25m;

        decimal discountPercent =
            GetVipDiscount(customer) +
            GetLoyaltyDiscount(customer) +
            GetOrderValueDiscount(totalAmount);

        if (discountPercent > MaxDiscountPercent)
        {
            _logger.LogDebug(
                "Discount capped at {MaxDiscountPercent}% (original {OriginalDiscount}%)",
                MaxDiscountPercent,
                discountPercent);

            discountPercent = MaxDiscountPercent;
        }

        _logger.LogInformation(
            "Final discount for customer {CustomerId}: {DiscountPercent}%",
            customer.Id,
            discountPercent);

        return discountPercent;
    }

    /* ----------  private helpers  ---------- */

    private static decimal GetVipDiscount(Customer customer) =>
        customer.IsVip ? 10m : 0m;

    private static decimal GetLoyaltyDiscount(Customer customer)
    {
        // Use UTC to avoid daylight-savings edge-cases and treat "full years" correctly.
        var yearsWithCompany =
            (DateTime.UtcNow.Date - customer.RegistrationDate.Date).TotalDays / 365;

        return yearsWithCompany switch
        {
            >= 5 => 5m,
            >= 2 => 2m,
            _ => 0m
        };
    }

    private static decimal GetOrderValueDiscount(decimal totalAmount) => totalAmount switch
    {
        > 10_000m => 15m,
        > 5_000m => 10m,
        > 1_000m => 5m,
        _ => 0m
    };
}
