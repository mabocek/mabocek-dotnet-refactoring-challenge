
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RefactoringChallenge.Models;
using RefactoringChallenge.Repositories;
using RefactoringChallenge.Services;

namespace RefactoringChallenge.Output;
/// <summary>
/// New implementation of the CustomerOrderProcessor that utilizes dependency injection
/// and the refactored interfaces from the Common project.
/// </summary>
public class New_CustomerOrderProcessor
{
    private readonly IOrderProcessingService _orderProcessingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="New_CustomerOrderProcessor"/> class.
    /// </summary>
    /// <param name="orderProcessingService">The order processing service.</param>
    public New_CustomerOrderProcessor(IOrderProcessingService orderProcessingService)
    {
        _orderProcessingService = orderProcessingService ?? throw new ArgumentNullException(nameof(orderProcessingService));
    }

    /// <summary>
    /// Process all new orders for specific customer. Update discount and status.
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>List of processed orders</returns>
    public List<Order> ProcessCustomerOrders(int customerId)
    {
        // Validate input
        if (customerId <= 0)
            throw new ArgumentException("ID zákazníka musí být kladné číslo.", nameof(customerId));

        // Use the OrderProcessingService to process orders asynchronously
        // We'll sync it back to maintain the same method signature
        return _orderProcessingService.ProcessCustomerOrdersAsync(customerId).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Asynchronous version of ProcessCustomerOrders to better align with modern .NET practices
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of processed orders.</returns>
    public Task<List<Order>> ProcessCustomerOrdersAsync(int customerId)
    {
        // Validate input
        if (customerId <= 0)
            throw new ArgumentException("ID zákazníka musí být kladné číslo.", nameof(customerId));

        // Use the OrderProcessingService to process orders asynchronously
        return _orderProcessingService.ProcessCustomerOrdersAsync(customerId);
    }
}
