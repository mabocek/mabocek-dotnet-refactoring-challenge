using RefactoringChallenge.Models;

namespace RefactoringChallenge.Repositories;

/// <summary>
/// Interface for product data access operations. 
/// This repository is responsible for retrieving and managing Product entities.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    /// <param name="productId">The unique identifier of the product to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the Product if found; otherwise, null.
    /// </returns>
    Task<Product?> GetProductByIdAsync(int productId);
}