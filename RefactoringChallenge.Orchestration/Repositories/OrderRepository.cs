using System.Data;
using System.Data.Common;
using RefactoringChallenge.Models;
using RefactoringChallenge.Factories;
using RefactoringChallenge.Repositories;

namespace RefactoringChallenge.Orchestration.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IDatabaseConnectionFactory _connectionFactory;
    private readonly IProductRepository _productRepository;

    public OrderRepository(IDatabaseConnectionFactory connectionFactory, IProductRepository productRepository)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<List<Order>> GetPendingOrdersByCustomerIdAsync(int customerId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        const string query = "SELECT Id, CustomerId, OrderDate, TotalAmount, DiscountPercent, DiscountAmount, Status " +
                             "FROM Orders WHERE CustomerId = @CustomerId AND Status = 'Pending'";

        using var command = connection.CreateCommand();
        command.CommandText = query;

        var customerIdParam = command.CreateParameter();
        customerIdParam.ParameterName = "@CustomerId";
        customerIdParam.Value = customerId;
        command.Parameters.Add(customerIdParam);

        using var reader = await command.ExecuteReaderAsync();
        var orders = new List<Order>();

        while (await reader.ReadAsync())
        {
            var order = new Order
            {
                Id = reader.GetInt32(0),
                CustomerId = reader.GetInt32(1),
                OrderDate = reader.GetDateTime(2),
                TotalAmount = reader.GetDecimal(3),
                DiscountPercent = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
                DiscountAmount = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                Status = reader.GetString(6),
                Items = new List<OrderItem>()
            };

            orders.Add(order);
        }

        foreach (var order in orders)
        {
            order.Items = await GetOrderItemsByOrderIdAsync(order.Id);
        }

        return orders;
    }

    public async Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        const string query = "SELECT Id, OrderId, ProductId, Quantity, UnitPrice FROM OrderItems WHERE OrderId = @OrderId";

        using var command = connection.CreateCommand();
        command.CommandText = query;

        var orderIdParam = command.CreateParameter();
        orderIdParam.ParameterName = "@OrderId";
        orderIdParam.Value = orderId;
        command.Parameters.Add(orderIdParam);

        using var reader = await command.ExecuteReaderAsync();
        var orderItems = new List<OrderItem>();

        while (await reader.ReadAsync())
        {
            var orderItem = new OrderItem
            {
                Id = reader.GetInt32(0),
                OrderId = reader.GetInt32(1),
                ProductId = reader.GetInt32(2),
                Quantity = reader.GetInt32(3),
                UnitPrice = reader.GetDecimal(4)
            };

            orderItems.Add(orderItem);
        }

        // Load product details for each order item
        foreach (var item in orderItems)
        {
            var product = await _productRepository.GetProductByIdAsync(item.ProductId);
            if (product != null)
            {
                item.Product = product;
            }
        }

        return orderItems;
    }

    public async Task UpdateOrderAsync(Order order)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        const string query = "UPDATE Orders SET TotalAmount = @TotalAmount, " +
                             "DiscountPercent = @DiscountPercent, DiscountAmount = @DiscountAmount, " +
                             "Status = @Status WHERE Id = @OrderId";

        using var command = connection.CreateCommand();
        command.CommandText = query;

        var totalAmountParam = command.CreateParameter();
        totalAmountParam.ParameterName = "@TotalAmount";
        totalAmountParam.Value = order.TotalAmount;
        command.Parameters.Add(totalAmountParam);

        var discountPercentParam = command.CreateParameter();
        discountPercentParam.ParameterName = "@DiscountPercent";
        discountPercentParam.Value = order.DiscountPercent;
        command.Parameters.Add(discountPercentParam);

        var discountAmountParam = command.CreateParameter();
        discountAmountParam.ParameterName = "@DiscountAmount";
        discountAmountParam.Value = order.DiscountAmount;
        command.Parameters.Add(discountAmountParam);

        var statusParam = command.CreateParameter();
        statusParam.ParameterName = "@Status";
        statusParam.Value = order.Status;
        command.Parameters.Add(statusParam);

        var orderIdParam = command.CreateParameter();
        orderIdParam.ParameterName = "@OrderId";
        orderIdParam.Value = order.Id;
        command.Parameters.Add(orderIdParam);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateOrderStatusAsync(int orderId, string status)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        const string query = "UPDATE Orders SET Status = @Status WHERE Id = @OrderId";

        using var command = connection.CreateCommand();
        command.CommandText = query;

        var statusParam = command.CreateParameter();
        statusParam.ParameterName = "@Status";
        statusParam.Value = status;
        command.Parameters.Add(statusParam);

        var orderIdParam = command.CreateParameter();
        orderIdParam.ParameterName = "@OrderId";
        orderIdParam.Value = orderId;
        command.Parameters.Add(orderIdParam);

        await command.ExecuteNonQueryAsync();
    }

    public async Task AddOrderLogAsync(int orderId, string message)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        const string query = "INSERT INTO OrderLogs (OrderId, LogDate, Message) VALUES (@OrderId, @LogDate, @Message)";

        using var command = connection.CreateCommand();
        command.CommandText = query;

        var orderIdParam = command.CreateParameter();
        orderIdParam.ParameterName = "@OrderId";
        orderIdParam.Value = orderId;
        command.Parameters.Add(orderIdParam);

        var logDateParam = command.CreateParameter();
        logDateParam.ParameterName = "@LogDate";
        logDateParam.Value = DateTime.Now;
        command.Parameters.Add(logDateParam);

        var messageParam = command.CreateParameter();
        messageParam.ParameterName = "@Message";
        messageParam.Value = message;
        command.Parameters.Add(messageParam);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        const string query = "SELECT Id, CustomerId, OrderDate, TotalAmount, DiscountPercent, DiscountAmount, Status " +
                             "FROM Orders WHERE Id = @OrderId";

        using var command = connection.CreateCommand();
        command.CommandText = query;

        var orderIdParam = command.CreateParameter();
        orderIdParam.ParameterName = "@OrderId";
        orderIdParam.Value = orderId;
        command.Parameters.Add(orderIdParam);

        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        var order = new Order
        {
            Id = reader.GetInt32(0),
            CustomerId = reader.GetInt32(1),
            OrderDate = reader.GetDateTime(2),
            TotalAmount = reader.GetDecimal(3),
            DiscountPercent = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
            DiscountAmount = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
            Status = reader.GetString(6),
            Items = new List<OrderItem>()
        };

        // Close the current reader before starting a new command
        reader.Close();

        // Get the order items
        order.Items = await GetOrderItemsByOrderIdAsync(order.Id);

        return order;
    }
}
