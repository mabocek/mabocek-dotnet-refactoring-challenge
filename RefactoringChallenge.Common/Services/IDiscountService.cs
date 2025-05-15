using RefactoringChallenge.Models;

namespace RefactoringChallenge.Services;

/// <summary>
/// Provides discount calculation functionality for customer orders
/// </summary>
/// <remarks>
/// This service is responsible for determining the appropriate discount percentage
/// based on customer information and order total amount.
/// </remarks>
public interface IDiscountService
{
    /// <summary>
    /// Calculates the discount percentage applicable for a given customer and order amount
    /// </summary>
    /// <param name="customer">The customer information used to determine eligibility for discounts</param>
    /// <param name="totalAmount">The total monetary amount of the order before applying discounts</param>
    /// <returns>A decimal value representing the discount percentage (e.g., 0.1 for 10% discount)</returns>
    decimal CalculateDiscountPercentage(Customer customer, decimal totalAmount);
}