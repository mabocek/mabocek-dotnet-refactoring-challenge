using RefactoringChallenge.Models;

namespace RefactoringChallenge.Services;

/// <summary>
/// Defines the contract for a service that processes customer orders.
/// This service is responsible for retrieving and processing orders for a specific customer.
/// </summary>
public interface IOrderProcessingService
{
    /// <summary>
    /// Asynchronously processes all orders for a specific customer.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer whose orders need to be processed.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of processed orders.</returns>
    Task<List<Order>> ProcessCustomerOrdersAsync(int customerId);
}