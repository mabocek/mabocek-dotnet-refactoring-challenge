using RefactoringChallenge.Models;

namespace RefactoringChallenge.Tests.Unit.Models;

[TestFixture]
public class ModelsTests
{
    [SetUp]
    public void Setup()
    {
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
    }

    [Test]
    public void Customer_Properties_AreCorrectlySet()
    {
        // Arrange
        int id = 1;
        string name = "John Doe";
        string email = "john@example.com";
        bool isVip = true;
        DateTime registrationDate = new DateTime(2020, 1, 1);

        // Act
        var customer = new Customer
        {
            Id = id,
            Name = name,
            Email = email,
            IsVip = isVip,
            RegistrationDate = registrationDate
        };

        // Assert
        Assert.That(customer.Id, Is.EqualTo(id));
        Assert.That(customer.Name, Is.EqualTo(name));
        Assert.That(customer.Email, Is.EqualTo(email));
        Assert.That(customer.IsVip, Is.EqualTo(isVip));
        Assert.That(customer.RegistrationDate, Is.EqualTo(registrationDate));
    }

    [Test]
    public void Product_Properties_AreCorrectlySet()
    {
        // Arrange
        int id = 1;
        string name = "Test Product";
        string category = "Test Category";
        decimal price = 19.99m;

        // Act
        var product = new Product
        {
            Id = id,
            Name = name,
            Category = category,
            Price = price
        };

        // Assert
        Assert.That(product.Id, Is.EqualTo(id));
        Assert.That(product.Name, Is.EqualTo(name));
        Assert.That(product.Category, Is.EqualTo(category));
        Assert.That(product.Price, Is.EqualTo(price));
    }

    [Test]
    public void OrderItem_Properties_AreCorrectlySet()
    {
        // Arrange
        int id = 1;
        int orderId = 101;
        int productId = 201;
        int quantity = 5;
        decimal unitPrice = 19.99m;
        var product = new Product { Id = productId, Name = "Test Product" };

        // Act
        var orderItem = new OrderItem
        {
            Id = id,
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Product = product
        };

        // Assert
        Assert.That(orderItem.Id, Is.EqualTo(id));
        Assert.That(orderItem.OrderId, Is.EqualTo(orderId));
        Assert.That(orderItem.ProductId, Is.EqualTo(productId));
        Assert.That(orderItem.Quantity, Is.EqualTo(quantity));
        Assert.That(orderItem.UnitPrice, Is.EqualTo(unitPrice));
        Assert.That(orderItem.Product, Is.SameAs(product));
    }

    [Test]
    public void Order_Properties_AreCorrectlySet()
    {
        // Arrange
        int id = 101;
        int customerId = 201;
        DateTime orderDate = DateTime.Now;
        string status = "Pending";
        decimal totalAmount = 99.99m;
        decimal discountPercent = 10m;
        decimal discountAmount = 10m;
        var items = new List<OrderItem>
        {
            new OrderItem { Id = 1, OrderId = id, ProductId = 301, Quantity = 2, UnitPrice = 19.99m }
        };

        // Act
        var order = new Order
        {
            Id = id,
            CustomerId = customerId,
            OrderDate = orderDate,
            Status = status,
            TotalAmount = totalAmount,
            DiscountPercent = discountPercent,
            DiscountAmount = discountAmount,
            Items = items
        };

        // Assert
        Assert.That(order.Id, Is.EqualTo(id));
        Assert.That(order.CustomerId, Is.EqualTo(customerId));
        Assert.That(order.OrderDate, Is.EqualTo(orderDate));
        Assert.That(order.Status, Is.EqualTo(status));
        Assert.That(order.TotalAmount, Is.EqualTo(totalAmount));
        Assert.That(order.DiscountPercent, Is.EqualTo(discountPercent));
        Assert.That(order.DiscountAmount, Is.EqualTo(discountAmount));
        Assert.That(order.Items, Is.SameAs(items));
    }
}
