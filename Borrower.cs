// A person who borrows equipment. Maps to the Borrowers table.
public class Borrower
{
    public int Id { get; set; }                          // primary key
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // One borrower can have many loans.
    public List<Loan> Loans { get; set; } = new();
}
