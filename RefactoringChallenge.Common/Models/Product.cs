namespace RefactoringChallenge.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }

    // Calculate the price with tax
    public decimal GetPriceWithTax(decimal taxRate = 0.2m)
    {
        return Price * (1 + taxRate);
    }

    // Override ToString to provide a useful representation
    public override string ToString()
    {
        return $"Product {Id}: {Name} ({Category}) - ${Price:F2}";
    }
}
