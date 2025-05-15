using System.Data;
using System.Data.Common;
using RefactoringChallenge.Models;
using RefactoringChallenge.Factories;
using RefactoringChallenge.Repositories;

namespace RefactoringChallenge.Orchestration.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IDatabaseConnectionFactory _connectionFactory;

    public ProductRepository(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        const string query = "SELECT Id, Name, Category, Price FROM Products WHERE Id = @ProductId";

        using var command = connection.CreateCommand();
        command.CommandText = query;

        var productIdParam = command.CreateParameter();
        productIdParam.ParameterName = "@ProductId";
        productIdParam.Value = productId;
        command.Parameters.Add(productIdParam);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Category = reader.GetString(2),
                Price = reader.GetDecimal(3)
            };
        }

        return null;
    }
}
