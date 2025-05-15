using RefactoringChallenge.Models;

namespace RefactoringChallenge.Tests.TestHelpers;

/// <summary>
/// Provides test data for integration tests
/// </summary>
public static class TestData
{
    /// <summary>
    /// Get test customers
    /// </summary>
    public static List<Customer> GetTestCustomers()
    {
        return new List<Customer>
        {
            new Customer
            {
                Id = 1,
                Name = "John Doe",
                Email = "john@example.com",
                IsVip = true,
                RegistrationDate = new DateTime(2020, 1, 1)
            },
            new Customer
            {
                Id = 2,
                Name = "Jane Smith",
                Email = "jane@example.com",
                IsVip = false,
                RegistrationDate = new DateTime(2021, 6, 15)
            },
            new Customer
            {
                Id = 3,
                Name = "Bob Johnson",
                Email = "bob@example.com",
                IsVip = false,
                RegistrationDate = new DateTime(2022, 3, 10)
            }
        };
    }

    /// <summary>
    /// Get test products
    /// </summary>
    public static List<Product> GetTestProducts()
    {
        return new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "Product 1",
                Price = 10.99m,
                Category = "Category A"
            },
            new Product
            {
                Id = 2,
                Name = "Product 2",
                Price = 20.99m,
                Category = "Category A"
            },
            new Product
            {
                Id = 3,
                Name = "Product 3",
                Price = 30.99m,
                Category = "Category B"
            },
            new Product
            {
                Id = 4,
                Name = "Product 4",
                Price = 40.99m,
                Category = "Category B"
            },
            new Product
            {
                Id = 5,
                Name = "Product 5",
                Price = 50.99m,
                Category = "Category C"
            }
        };
    }
}
