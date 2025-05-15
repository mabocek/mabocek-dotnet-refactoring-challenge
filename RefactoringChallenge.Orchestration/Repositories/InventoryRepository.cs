using System.Data;
using System.Data.Common;
using RefactoringChallenge.Factories;
using RefactoringChallenge.Repositories;

namespace RefactoringChallenge.Orchestration.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly IDatabaseConnectionFactory _connectionFactory;

    public InventoryRepository(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<int?> GetStockQuantityByProductIdAsync(int productId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        const string query = "SELECT StockQuantity FROM Inventory WHERE ProductId = @ProductId";

        using var command = connection.CreateCommand();
        command.CommandText = query;

        var productIdParam = command.CreateParameter();
        productIdParam.ParameterName = "@ProductId";
        productIdParam.Value = productId;
        command.Parameters.Add(productIdParam);

        var result = await command.ExecuteScalarAsync();
        return result != DBNull.Value ? Convert.ToInt32(result) : null;
    }

    public async Task UpdateStockQuantityAsync(int productId, int stockQuantityUpdate)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        const string query = "UPDATE Inventory SET StockQuantity = StockQuantity - @StockQuantityUpdate WHERE ProductId = @ProductId";

        using var command = connection.CreateCommand();
        command.CommandText = query;

        var productIdParam = command.CreateParameter();
        productIdParam.ParameterName = "@ProductId";
        productIdParam.Value = productId;
        command.Parameters.Add(productIdParam);

        var quantityParam = command.CreateParameter();
        quantityParam.ParameterName = "@StockQuantityUpdate";
        quantityParam.Value = stockQuantityUpdate;
        command.Parameters.Add(quantityParam);

        await command.ExecuteNonQueryAsync();
    }
}
