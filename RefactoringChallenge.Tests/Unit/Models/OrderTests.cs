using RefactoringChallenge.Models;
using System.Reflection;

namespace RefactoringChallenge.Tests.Unit.Models
{
    [TestFixture]
    public class OrderTests
    {
        [SetUp]
        public void Setup()
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        }

        [Test]
        public void Order_Properties_WorkAsExpected()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                CustomerId = 42,
                OrderDate = new DateTime(2023, 1, 1),
                TotalAmount = 100.50m,
                DiscountPercent = 10.5m,
                DiscountAmount = 10.55m,
                Status = "Ready",
                Items = new List<OrderItem>()
            };

            // Assert
            Assert.That(order.Id, Is.EqualTo(1));
            Assert.That(order.CustomerId, Is.EqualTo(42));
            Assert.That(order.OrderDate, Is.EqualTo(new DateTime(2023, 1, 1)));
            Assert.That(order.TotalAmount, Is.EqualTo(100.50m));
            Assert.That(order.DiscountPercent, Is.EqualTo(10.5m));
            Assert.That(order.DiscountAmount, Is.EqualTo(10.55m));
            Assert.That(order.Status, Is.EqualTo("Ready"));
            Assert.That(order.Items, Is.Not.Null);
        }

        [Test]
        public void Order_GetFinalAmount_ReturnsCorrectValue()
        {
            // Arrange
            var order = new Order
            {
                TotalAmount = 100.00m,
                DiscountAmount = 20.00m
            };

            // Act
            var finalAmount = order.GetFinalAmount();

            // Assert
            Assert.That(finalAmount, Is.EqualTo(80.00m));
        }

        [Test]
        public void Order_IsReadyForShipping_ReturnsCorrectValue()
        {
            // Arrange
            var readyOrder = new Order { Status = "Ready" };
            var pendingOrder = new Order { Status = "Pending" };
            var readyLowercaseOrder = new Order { Status = "ready" };

            // Act & Assert
            Assert.That(readyOrder.IsReadyForShipping(), Is.True);
            Assert.That(pendingOrder.IsReadyForShipping(), Is.False);
            Assert.That(readyLowercaseOrder.IsReadyForShipping(), Is.True);
        }

        [Test]
        public void Order_ToString_ReturnsFormattedString()
        {
            // Arrange
            var order = new Order
            {
                Id = 101,
                CustomerId = 42,
                Status = "Processing",
                TotalAmount = 199.99m
            };

            // Act
            var result = order.ToString();

            // Assert
            Assert.That(result, Is.EqualTo("Order 101: Customer 42, Status: Processing, Total: $199.99"));
        }

        [Test]
        public void Order_ForcePropertyAccessorsToBeExecuted()
        {
            // This test uses reflection to force accessors to be executed
            // which should ensure the model classes are included in coverage
            var order = new Order();

            // Get all properties
            var properties = typeof(Order).GetProperties();

            // Set and get each property to ensure code coverage
            foreach (PropertyInfo prop in properties)
            {
                if (prop.Name == "Id")
                {
                    prop.SetValue(order, 42);
                    var value = (int)prop.GetValue(order);
                    Assert.That(value, Is.EqualTo(42));
                }
                else if (prop.Name == "CustomerId")
                {
                    prop.SetValue(order, 123);
                    var value = (int)prop.GetValue(order);
                    Assert.That(value, Is.EqualTo(123));
                }
                else if (prop.Name == "OrderDate")
                {
                    var date = new DateTime(2023, 5, 15);
                    prop.SetValue(order, date);
                    var value = (DateTime)prop.GetValue(order);
                    Assert.That(value, Is.EqualTo(date));
                }
                else if (prop.Name == "TotalAmount")
                {
                    prop.SetValue(order, 999.99m);
                    var value = (decimal)prop.GetValue(order);
                    Assert.That(value, Is.EqualTo(999.99m));
                }
                else if (prop.Name == "DiscountPercent")
                {
                    prop.SetValue(order, 15.5m);
                    var value = (decimal)prop.GetValue(order);
                    Assert.That(value, Is.EqualTo(15.5m));
                }
                else if (prop.Name == "DiscountAmount")
                {
                    prop.SetValue(order, 155.0m);
                    var value = (decimal)prop.GetValue(order);
                    Assert.That(value, Is.EqualTo(155.0m));
                }
                else if (prop.Name == "Status")
                {
                    prop.SetValue(order, "Pending");
                    var value = (string)prop.GetValue(order);
                    Assert.That(value, Is.EqualTo("Pending"));
                }
                else if (prop.Name == "Items")
                {
                    var items = new List<OrderItem>();
                    prop.SetValue(order, items);
                    var value = prop.GetValue(order);
                    Assert.That(value, Is.SameAs(items));
                }
            }
        }
    }
}
