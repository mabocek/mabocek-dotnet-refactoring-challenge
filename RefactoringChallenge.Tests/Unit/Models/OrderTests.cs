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

        // Test removed: Order_ForcePropertyAccessorsToBeExecuted
        // This test was removed because it was using reflection to artificially increase code coverage
        // without properly testing business logic. The basic property functionality is already
        // covered in the Order_Properties_WorkAsExpected test.
    }
}
