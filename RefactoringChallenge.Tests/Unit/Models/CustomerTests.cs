using RefactoringChallenge.Models;
using System.Reflection;

namespace RefactoringChallenge.Tests.Unit.Models
{
    [TestFixture]
    public class CustomerTests
    {
        [Test]
        public void Customer_Properties_WorkAsExpected()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                Name = "Test User",
                Email = "test@example.com",
                IsVip = true,
                RegistrationDate = new DateTime(2023, 1, 1)
            };

            // Assert
            Assert.That(customer.Id, Is.EqualTo(1));
            Assert.That(customer.Name, Is.EqualTo("Test User"));
            Assert.That(customer.Email, Is.EqualTo("test@example.com"));
            Assert.That(customer.IsVip, Is.True);
            Assert.That(customer.RegistrationDate, Is.EqualTo(new DateTime(2023, 1, 1)));
        }

        [Test]
        public void Customer_GetYearsAsCustomer_ReturnsCorrectValue()
        {
            // Arrange
            var currentYear = DateTime.Now.Year;
            var customer = new Customer
            {
                RegistrationDate = new DateTime(currentYear - 3, 1, 1)
            };

            // Act
            var years = customer.GetYearsAsCustomer();

            // Assert
            Assert.That(years, Is.EqualTo(3));
        }

        [Test]
        public void Customer_ToString_ReturnsFormattedString()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 42,
                Name = "John Doe",
                Email = "john@example.com"
            };

            // Act
            var result = customer.ToString();

            // Assert
            Assert.That(result, Is.EqualTo("Customer 42: John Doe (john@example.com)"));
        }

        // Test removed: Customer_ForcePropertyAccessorsToBeExecuted
        // This test was removed because it was using reflection to artificially increase code coverage
        // without properly testing business logic. The basic property functionality is already
        // covered in the Customer_Properties_WorkAsExpected test.
    }
}
