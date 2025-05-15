using System.Data;
using System.Data.Common;
using RefactoringChallenge.Models;
using RefactoringChallenge.Factories;
using RefactoringChallenge.Repositories;

namespace RefactoringChallenge.Orchestration.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly IDatabaseConnectionFactory _connectionFactory;

    public CustomerRepository(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<Customer?> GetCustomerByIdAsync(int customerId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        const string query = "SELECT Id, Name, Email, IsVip, RegistrationDate FROM Customers WHERE Id = @CustomerId";

        using var command = connection.CreateCommand();
        command.CommandText = query;

        var customerIdParam = command.CreateParameter();
        customerIdParam.ParameterName = "@CustomerId";
        customerIdParam.Value = customerId;
        command.Parameters.Add(customerIdParam);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Customer
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                IsVip = reader.GetBoolean(3),
                RegistrationDate = reader.GetDateTime(4)
            };
        }

        return null;
    }
}
