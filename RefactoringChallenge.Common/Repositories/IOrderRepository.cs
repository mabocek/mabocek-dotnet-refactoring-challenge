using RefactoringChallenge.Models;

namespace RefactoringChallenge.Repositories;

/// <summary>
/// Interface for operations related to order data access in the database.
/// Provides methods to retrieve, update and log order-related information.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Retrieves a list of pending orders for a specific customer.
    /// </summary>
    /// <param name="customerId">The ID of the customer whose pending orders are requested.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of pending orders.</returns>
    Task<List<Order>> GetPendingOrdersByCustomerIdAsync(int customerId);

    /// <summary>
    /// Retrieves an order by its unique identifier.
    /// </summary>
    /// <param name="orderId">The ID of the order to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the order if found, otherwise null.</returns>
    Task<Order?> GetOrderByIdAsync(int orderId);

    /// <summary>
    /// Retrieves all items associated with a specific order.
    /// </summary>
    /// <param name="orderId">The ID of the order whose items are requested.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of order items.</returns>
    Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId);

    /// <summary>
    /// Updates an existing order in the database.
    /// </summary>
    /// <param name="order">The order object with updated information.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateOrderAsync(Order order);

    /// <summary>
    /// Updates the status of an existing order.
    /// </summary>
    /// <param name="orderId">The ID of the order to update.</param>
    /// <param name="status">The new status value to set for the order.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateOrderStatusAsync(int orderId, string status);

    /// <summary>
    /// Adds a log entry for an order to track operations or status changes.
    /// </summary>
    /// <param name="orderId">The ID of the order to log information for.</param>
    /// <param name="message">The message to be logged for the order.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddOrderLogAsync(int orderId, string message);
}
