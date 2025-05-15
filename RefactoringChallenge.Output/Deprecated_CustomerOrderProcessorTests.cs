
using System.Data.SqlClient;
using NUnit.Framework;

namespace RefactoringChallenge;
/// <summary>
/// DEPRECATED: These tests are kept for historical reference only.
/// Please use the refactored tests in Tests folder.
/// </summary>
[Obsolete("These tests use the deprecated CustomerOrderProcessor. Use OrderProcessingServiceTests instead.", true)]

[TestFixture]
[Category("Integration")]
public class Deprecated_CustomerOrderProcessorTests
{

    private readonly string _connectionString = "Server=mssql,1433;Database=refactoringchallenge;User ID=sa;Password=RCPassword1!;";


    [SetUp]
    public void SetUp()
    {
        SetupDatabase();
    }

    [Test]
    public void ProcessCustomerOrders_ForVipCustomerWithLargeOrder_AppliesCorrectDiscounts()
    {
        int customerId = 1; // VIP customer
        var processor = new Deprecated_CustomerOrderProcessor();

        var result = processor.ProcessCustomerOrders(customerId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));

        var largeOrder = result.Find(o => o.Id == 1);
        Assert.That(largeOrder, Is.Not.Null);
        Assert.That(largeOrder.DiscountPercent, Is.EqualTo(25)); // Max. discount 25%
        Assert.That(largeOrder.Status, Is.EqualTo("Ready"));

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand("SELECT StockQuantity FROM Inventory WHERE ProductId = 1", connection))
            {
                var newStock = (int)command.ExecuteScalar();
                Assert.That(newStock, Is.EqualTo(90)); // Origin qty 100, ordered 10
            }
        }
    }

    [Test]
    public void ProcessCustomerOrders_ForRegularCustomerWithSmallOrder_AppliesMinimalDiscount()
    {
        int customerId = 2; // Regular customer
        var processor = new Deprecated_CustomerOrderProcessor();

        var result = processor.ProcessCustomerOrders(customerId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));

        var smallOrder = result[0];
        Assert.That(smallOrder.DiscountPercent, Is.EqualTo(2)); // 2% loyalty discount
        Assert.That(smallOrder.Status, Is.EqualTo("Ready"));
    }

    [Test]
    public void ProcessCustomerOrders_ForOrderWithUnavailableProducts_SetsOrderOnHold()
    {
        int customerId = 3; // Customer with order with non-available items
        var processor = new Deprecated_CustomerOrderProcessor();

        var result = processor.ProcessCustomerOrders(customerId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));

        var onHoldOrder = result[0];
        Assert.That(onHoldOrder.Status, Is.EqualTo("OnHold"));

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand("SELECT Message FROM OrderLogs WHERE OrderId = @OrderId ORDER BY LogDate DESC", connection))
            {
                command.Parameters.AddWithValue("@OrderId", onHoldOrder.Id);
                var message = (string)command.ExecuteScalar();
                Assert.That(message, Is.EqualTo("Order on hold. Some items are not on stock."));
            }
        }
    }

    private void SetupDatabase()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            ExecuteNonQuery(connection, "DELETE FROM OrderLogs");
            ExecuteNonQuery(connection, "DELETE FROM OrderItems");
            ExecuteNonQuery(connection, "DELETE FROM Orders");
            ExecuteNonQuery(connection, "DELETE FROM Inventory");
            ExecuteNonQuery(connection, "DELETE FROM Products");
            ExecuteNonQuery(connection, "DELETE FROM Customers");

            ExecuteNonQuery(connection, @"
                INSERT INTO Customers (Id, Name, Email, IsVip, RegistrationDate) VALUES 
                (1, 'Joe Doe', 'joe.doe@example.com', 1, '2015-01-01'),
                (2, 'John Smith', 'john@example.com', 0, '2023-03-15'),
                (3, 'James Miller', 'miller@example.com', 0, '2024-01-01')
            ");

            ExecuteNonQuery(connection, @"
                INSERT INTO Products (Id, Name, Category, Price) VALUES 
                (1, 'White', 'T-Shirts', 25000),
                (2, 'Gray', 'T-Shirts', 800),
                (3, 'Gold', 'T-Shirts', 5000),
                (4, 'Black', 'T-Shirts', 500)
            ");

            ExecuteNonQuery(connection, @"
                INSERT INTO Inventory (ProductId, StockQuantity) VALUES 
                (1, 100),
                (2, 200),
                (3, 5),
                (4, 50)
            ");

            ExecuteNonQuery(connection, @"
                INSERT INTO Orders (Id, CustomerId, OrderDate, TotalAmount, Status) VALUES 
                (1, 1, '2025-04-10', 0, 'Pending'),
                (2, 1, '2025-04-12', 0, 'Pending'),
                (3, 2, '2025-04-13', 0, 'Pending'),
                (4, 3, '2025-04-14', 0, 'Pending')
            ");

            ExecuteNonQuery(connection, @"
                INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice) VALUES 
                (1, 1, 10, 25000),
                (1, 2, 5, 800),
                (2, 4, 3, 500),
                (3, 2, 1, 800),
                (4, 3, 10, 5000)
            ");
        }
    }

    private void ExecuteNonQuery(SqlConnection connection, string commandText)
    {
        using (var command = new SqlCommand(commandText, connection))
        {
            command.ExecuteNonQuery();
        }
    }
}
