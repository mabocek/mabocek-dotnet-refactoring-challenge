namespace RefactoringChallenge.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public bool IsVip { get; set; }
    public DateTime RegistrationDate { get; set; }

    // Added method to have executable code that can be covered
    public int GetYearsAsCustomer()
    {
        return DateTime.Now.Year - RegistrationDate.Year;
    }

    // Override ToString to provide a useful representation
    public override string ToString()
    {
        return $"Customer {Id}: {Name} ({Email})";
    }
}
