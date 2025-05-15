namespace RefactoringChallenge.Repositories;

/// <summary>
/// Represents a repository interface for managing product inventory operations.
/// </summary>
/// <remarks>
/// This interface provides methods to query and update product stock quantities in the inventory system.
/// </remarks>
public interface IInventoryRepository
{
    /// <summary>
    /// Retrieves the current stock quantity for a specified product.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <returns>
    /// A Task containing the nullable stock quantity. Returns null if the product is not found.
    /// </returns>
    Task<int?> GetStockQuantityByProductIdAsync(int productId);

    /// <summary>
    /// Updates the stock quantity of a specified product asynchronously.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="quantity">The new quantity to set for the product.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateStockQuantityAsync(int productId, int quantity);
}
