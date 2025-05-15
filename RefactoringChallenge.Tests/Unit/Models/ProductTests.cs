using RefactoringChallenge.Models;
using System.Reflection;

namespace RefactoringChallenge.Tests.Unit.Models
{
    [TestFixture]
    public class ProductTests
    {
        [SetUp]
        public void Setup()
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        }

        [Test]
        public void Product_Properties_WorkAsExpected()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Category = "Test Category",
                Price = 99.99m
            };

            // Assert
            Assert.That(product.Id, Is.EqualTo(1));
            Assert.That(product.Name, Is.EqualTo("Test Product"));
            Assert.That(product.Category, Is.EqualTo("Test Category"));
            Assert.That(product.Price, Is.EqualTo(99.99m));
        }

        [Test]
        public void Product_GetPriceWithTax_ReturnsCorrectAmount()
        {
            // Arrange
            var product = new Product
            {
                Price = 100m
            };

            // Act
            var priceWithDefaultTax = product.GetPriceWithTax();
            var priceWithSpecifiedTax = product.GetPriceWithTax(0.1m);

            // Assert
            Assert.That(priceWithDefaultTax, Is.EqualTo(120m));
            Assert.That(priceWithSpecifiedTax, Is.EqualTo(110m));
        }

        [Test]
        public void Product_ToString_ReturnsFormattedString()
        {
            // Arrange
            var product = new Product
            {
                Id = 42,
                Name = "Awesome Widget",
                Category = "Gadgets",
                Price = 199.99m
            };

            // Act
            var result = product.ToString();

            // Assert
            Assert.That(result, Is.EqualTo("Product 42: Awesome Widget (Gadgets) - $199.99"));
        }

        [Test]
        public void Product_ForcePropertyAccessorsToBeExecuted()
        {
            // This test uses reflection to force accessors to be executed
            // which should ensure the model classes are included in coverage
            var product = new Product();

            // Get all properties
            var properties = typeof(Product).GetProperties();

            // Set and get each property to ensure code coverage
            foreach (PropertyInfo prop in properties)
            {
                if (prop.Name == "Id")
                {
                    prop.SetValue(product, 42);
                    var value = (int)prop.GetValue(product);
                    Assert.That(value, Is.EqualTo(42));
                }
                else if (prop.Name == "Name")
                {
                    prop.SetValue(product, "Awesome Product");
                    var value = (string)prop.GetValue(product);
                    Assert.That(value, Is.EqualTo("Awesome Product"));
                }
                else if (prop.Name == "Category")
                {
                    prop.SetValue(product, "Electronics");
                    var value = (string)prop.GetValue(product);
                    Assert.That(value, Is.EqualTo("Electronics"));
                }
                else if (prop.Name == "Price")
                {
                    prop.SetValue(product, 299.99m);
                    var value = (decimal)prop.GetValue(product);
                    Assert.That(value, Is.EqualTo(299.99m));
                }
            }
        }
    }
}
