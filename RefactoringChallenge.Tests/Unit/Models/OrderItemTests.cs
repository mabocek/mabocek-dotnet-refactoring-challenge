using RefactoringChallenge.Models;
using System.Reflection;

namespace RefactoringChallenge.Tests.Unit.Models
{
    [TestFixture]
    public class OrderItemTests
    {
        [SetUp]
        public void Setup()
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        }

        [Test]
        public void OrderItem_Properties_WorkAsExpected()
        {
            // Arrange
            var product = new Product
            {
                Id = 5,
                Name = "Test Product",
                Category = "Test Category",
                Price = 29.99m
            };

            var orderItem = new OrderItem
            {
                Id = 1,
                OrderId = 100,
                ProductId = 5,
                Quantity = 3,
                UnitPrice = 29.99m,
                Product = product
            };

            // Assert
            Assert.That(orderItem.Id, Is.EqualTo(1));
            Assert.That(orderItem.OrderId, Is.EqualTo(100));
            Assert.That(orderItem.ProductId, Is.EqualTo(5));
            Assert.That(orderItem.Quantity, Is.EqualTo(3));
            Assert.That(orderItem.UnitPrice, Is.EqualTo(29.99m));
            Assert.That(orderItem.Product, Is.SameAs(product));
        }

        [Test]
        public void OrderItem_GetSubtotal_ReturnsCorrectValue()
        {
            // Arrange
            var orderItem = new OrderItem
            {
                Quantity = 5,
                UnitPrice = 19.99m
            };

            // Act
            var subtotal = orderItem.GetSubtotal();

            // Assert
            Assert.That(subtotal, Is.EqualTo(99.95m));
        }

        [Test]
        public void OrderItem_ToString_ReturnsFormattedString()
        {
            // Arrange
            var orderItem = new OrderItem
            {
                Id = 42,
                ProductId = 123,
                Quantity = 7,
                UnitPrice = 9.99m
            };

            // Act
            var result = orderItem.ToString();

            // Assert
            Assert.That(result, Is.EqualTo("OrderItem 42: 7x Product 123 at $9.99 each"));
        }

        [Test]
        public void OrderItem_ForcePropertyAccessorsToBeExecuted()
        {
            // This test uses reflection to force accessors to be executed
            // which should ensure the model classes are included in coverage
            var orderItem = new OrderItem();
            var product = new Product();

            // Get all properties
            var properties = typeof(OrderItem).GetProperties();

            // Set and get each property to ensure code coverage
            foreach (PropertyInfo prop in properties)
            {
                if (prop.Name == "Id")
                {
                    prop.SetValue(orderItem, 42);
                    var value = (int)prop.GetValue(orderItem);
                    Assert.That(value, Is.EqualTo(42));
                }
                else if (prop.Name == "OrderId")
                {
                    prop.SetValue(orderItem, 123);
                    var value = (int)prop.GetValue(orderItem);
                    Assert.That(value, Is.EqualTo(123));
                }
                else if (prop.Name == "ProductId")
                {
                    prop.SetValue(orderItem, 456);
                    var value = (int)prop.GetValue(orderItem);
                    Assert.That(value, Is.EqualTo(456));
                }
                else if (prop.Name == "Quantity")
                {
                    prop.SetValue(orderItem, 10);
                    var value = (int)prop.GetValue(orderItem);
                    Assert.That(value, Is.EqualTo(10));
                }
                else if (prop.Name == "UnitPrice")
                {
                    prop.SetValue(orderItem, 59.99m);
                    var value = (decimal)prop.GetValue(orderItem);
                    Assert.That(value, Is.EqualTo(59.99m));
                }
                else if (prop.Name == "Product")
                {
                    prop.SetValue(orderItem, product);
                    var value = prop.GetValue(orderItem);
                    Assert.That(value, Is.SameAs(product));
                }
            }
        }
    }
}
