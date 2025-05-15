
using RefactoringChallenge.Models;

namespace RefactoringChallenge.Repositories;
/// <summary>
/// Represents a repository for managing customer data.
/// </summary>
public interface ICustomerRepository
{
    /// <summary>
    /// Retrieves a customer by their unique identifier asynchronously.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the customer if found; otherwise, null.</returns>
    Task<Customer?> GetCustomerByIdAsync(int customerId);
}