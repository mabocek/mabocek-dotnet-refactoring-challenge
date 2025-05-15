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

        // Test removed: OrderItem_ForcePropertyAccessorsToBeExecuted
        // This test was removed because it was using reflection to artificially increase code coverage
        // without properly testing business logic. The basic property functionality is already
        // covered in the OrderItem_Properties_WorkAsExpected test.
    }
}
