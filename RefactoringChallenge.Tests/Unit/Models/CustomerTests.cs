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

        [Test]
        public void Customer_ForcePropertyAccessorsToBeExecuted()
        {
            // This test uses reflection to force accessors to be executed
            // which should ensure the model classes are included in coverage
            var customer = new Customer();

            // Get all properties
            var properties = typeof(Customer).GetProperties();

            // Set and get each property to ensure code coverage
            foreach (PropertyInfo prop in properties)
            {
                if (prop.Name == "Id")
                {
                    prop.SetValue(customer, 42);
                    var value = (int)prop.GetValue(customer);
                    Assert.That(value, Is.EqualTo(42));
                }
                else if (prop.Name == "Name")
                {
                    prop.SetValue(customer, "Test Name");
                    var value = (string)prop.GetValue(customer);
                    Assert.That(value, Is.EqualTo("Test Name"));
                }
                else if (prop.Name == "Email")
                {
                    prop.SetValue(customer, "test@example.net");
                    var value = (string)prop.GetValue(customer);
                    Assert.That(value, Is.EqualTo("test@example.net"));
                }
                else if (prop.Name == "IsVip")
                {
                    prop.SetValue(customer, true);
                    var value = (bool)prop.GetValue(customer);
                    Assert.That(value, Is.True);
                }
                else if (prop.Name == "RegistrationDate")
                {
                    var date = new DateTime(2023, 5, 15);
                    prop.SetValue(customer, date);
                    var value = (DateTime)prop.GetValue(customer);
                    Assert.That(value, Is.EqualTo(date));
                }
            }
        }
    }
}
