namespace RefactoringChallenge.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public Product Product { get; set; } = null!;

    // Calculate the subtotal for this item
    public decimal GetSubtotal()
    {
        return Quantity * UnitPrice;
    }

    // Override ToString to provide a useful representation
    public override string ToString()
    {
        return $"OrderItem {Id}: {Quantity}x Product {ProductId} at ${UnitPrice:F2} each";
    }
}
