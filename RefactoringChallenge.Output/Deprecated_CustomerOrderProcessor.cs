
using System.Data.SqlClient;
using RefactoringChallenge.Models;

namespace RefactoringChallenge;
/// <summary>
/// DEPRECATED: This class is kept for historical reference only.
/// Please use the refactored implementation with OrderProcessingService and related repositories.
/// </summary>
[Obsolete("This implementation is deprecated. Use OrderProcessingService instead.", true)]

public class Deprecated_CustomerOrderProcessor
{
    private readonly string _connectionString = "Server=mssql,1433;Database=refactoringchallenge;User ID=sa;Password=RCPassword1!;";

    /// <summary>
    /// Process all new orders for specific customer. Update discount and status.
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>List of processed orders</returns>
    public List<Order> ProcessCustomerOrders(int customerId)
    {
        if (customerId <= 0)
            throw new ArgumentException("ID zákazníka musí být kladné číslo.", nameof(customerId));

        var processedOrders = new List<Order>();

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            Customer customer = null;
            using (var command = new SqlCommand("SELECT Id, Name, Email, IsVip, RegistrationDate FROM Customers WHERE Id = @CustomerId", connection))
            {
                command.Parameters.AddWithValue("@CustomerId", customerId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        customer = new Customer
                        {
                            Id = (int)reader["Id"],
                            Name = (string)reader["Name"],
                            Email = (string)reader["Email"],
                            IsVip = (bool)reader["IsVip"],
                            RegistrationDate = (DateTime)reader["RegistrationDate"]
                        };
                    }
                }
            }

            if (customer == null)
                throw new Exception($"Zákazník s ID {customerId} nebyl nalezen.");

            var pendingOrders = new List<Order>();
            using (var command = new SqlCommand("SELECT Id, CustomerId, OrderDate, TotalAmount, Status FROM Orders WHERE CustomerId = @CustomerId AND Status = 'Pending'", connection))
            {
                command.Parameters.AddWithValue("@CustomerId", customerId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pendingOrders.Add(new Order
                        {
                            Id = (int)reader["Id"],
                            CustomerId = (int)reader["CustomerId"],
                            OrderDate = (DateTime)reader["OrderDate"],
                            TotalAmount = (decimal)reader["TotalAmount"],
                            Status = (string)reader["Status"]
                        });
                    }
                }
            }

            foreach (var order in pendingOrders)
            {
                using (var command = new SqlCommand("SELECT Id, OrderId, ProductId, Quantity, UnitPrice FROM OrderItems WHERE OrderId = @OrderId", connection))
                {
                    command.Parameters.AddWithValue("@OrderId", order.Id);

                    order.Items = new List<OrderItem>();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            order.Items.Add(new OrderItem
                            {
                                Id = (int)reader["Id"],
                                OrderId = (int)reader["OrderId"],
                                ProductId = (int)reader["ProductId"],
                                Quantity = (int)reader["Quantity"],
                                UnitPrice = (decimal)reader["UnitPrice"]
                            });
                        }
                    }
                }

                foreach (var item in order.Items)
                {
                    using (var command = new SqlCommand("SELECT Id, Name, Category, Price FROM Products WHERE Id = @ProductId", connection))
                    {
                        command.Parameters.AddWithValue("@ProductId", item.ProductId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                item.Product = new Product
                                {
                                    Id = (int)reader["Id"],
                                    Name = (string)reader["Name"],
                                    Category = (string)reader["Category"],
                                    Price = (decimal)reader["Price"]
                                };
                            }
                        }
                    }
                }
            }

            foreach (var order in pendingOrders)
            {
                decimal totalAmount = 0;
                foreach (var item in order.Items)
                {
                    var subtotal = item.Quantity * item.UnitPrice;
                    totalAmount += subtotal;
                }

                decimal discountPercent = 0;

                if (customer.IsVip)
                {
                    discountPercent += 10;
                }

                int yearsAsCustomer = DateTime.Now.Year - customer.RegistrationDate.Year;
                if (yearsAsCustomer >= 5)
                {
                    discountPercent += 5;
                }
                else if (yearsAsCustomer >= 2)
                {
                    discountPercent += 2;
                }

                if (totalAmount > 10000)
                {
                    discountPercent += 15;
                }
                else if (totalAmount > 5000)
                {
                    discountPercent += 10;
                }
                else if (totalAmount > 1000)
                {
                    discountPercent += 5;
                }

                if (discountPercent > 25)
                {
                    discountPercent = 25;
                }

                decimal discountAmount = totalAmount * (discountPercent / 100);
                decimal finalAmount = totalAmount - discountAmount;

                order.DiscountPercent = discountPercent;
                order.DiscountAmount = discountAmount;
                order.TotalAmount = finalAmount;
                order.Status = "Processed";

                using (var command = new SqlCommand("UPDATE Orders SET TotalAmount = @TotalAmount, DiscountPercent = @DiscountPercent, DiscountAmount = @DiscountAmount, Status = @Status WHERE Id = @OrderId", connection))
                {
                    command.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                    command.Parameters.AddWithValue("@DiscountPercent", order.DiscountPercent);
                    command.Parameters.AddWithValue("@DiscountAmount", order.DiscountAmount);
                    command.Parameters.AddWithValue("@Status", order.Status);
                    command.Parameters.AddWithValue("@OrderId", order.Id);

                    command.ExecuteNonQuery();
                }

                bool allProductsAvailable = true;
                foreach (var item in order.Items)
                {
                    using (var command = new SqlCommand("SELECT StockQuantity FROM Inventory WHERE ProductId = @ProductId", connection))
                    {
                        command.Parameters.AddWithValue("@ProductId", item.ProductId);

                        var stockQuantity = (int?)command.ExecuteScalar();
                        if (stockQuantity == null || stockQuantity < item.Quantity)
                        {
                            allProductsAvailable = false;
                            break;
                        }
                    }
                }

                if (allProductsAvailable)
                {
                    foreach (var item in order.Items)
                    {
                        using (var command = new SqlCommand("UPDATE Inventory SET StockQuantity = StockQuantity - @Quantity WHERE ProductId = @ProductId", connection))
                        {
                            command.Parameters.AddWithValue("@Quantity", item.Quantity);
                            command.Parameters.AddWithValue("@ProductId", item.ProductId);

                            command.ExecuteNonQuery();
                        }
                    }

                    order.Status = "Ready";

                    using (var command = new SqlCommand("UPDATE Orders SET Status = @Status WHERE Id = @OrderId", connection))
                    {
                        command.Parameters.AddWithValue("@Status", order.Status);
                        command.Parameters.AddWithValue("@OrderId", order.Id);

                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqlCommand("INSERT INTO OrderLogs (OrderId, LogDate, Message) VALUES (@OrderId, @LogDate, @Message)", connection))
                    {
                        command.Parameters.AddWithValue("@OrderId", order.Id);
                        command.Parameters.AddWithValue("@LogDate", DateTime.Now);
                        command.Parameters.AddWithValue("@Message", $"Order completed with {order.DiscountPercent}% discount. Total price: {order.TotalAmount}");

                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    order.Status = "OnHold";

                    using (var command = new SqlCommand("UPDATE Orders SET Status = @Status WHERE Id = @OrderId", connection))
                    {
                        command.Parameters.AddWithValue("@Status", order.Status);
                        command.Parameters.AddWithValue("@OrderId", order.Id);

                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqlCommand("INSERT INTO OrderLogs (OrderId, LogDate, Message) VALUES (@OrderId, @LogDate, @Message)", connection))
                    {
                        command.Parameters.AddWithValue("@OrderId", order.Id);
                        command.Parameters.AddWithValue("@LogDate", DateTime.Now);
                        command.Parameters.AddWithValue("@Message", "Order on hold. Some items are not on stock.");

                        command.ExecuteNonQuery();
                    }
                }

                processedOrders.Add(order);
            }
        }

        return processedOrders;
    }
}
