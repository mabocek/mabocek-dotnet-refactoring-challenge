namespace RefactoringChallenge.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();

    // Calculate the final amount after discount
    public decimal GetFinalAmount()
    {
        return TotalAmount - DiscountAmount;
    }

    // Check if the order is ready for shipping
    public bool IsReadyForShipping()
    {
        return Status.Equals(ModelConstants.OrderConstants.OrderStatusReady, StringComparison.OrdinalIgnoreCase);
    }

    // Override ToString to provide a useful representation
    public override string ToString()
    {
        return $"Order {Id}: Customer {CustomerId}, Status: {Status}, Total: ${TotalAmount:F2}";
    }
}
