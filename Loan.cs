// One borrower taking one piece of equipment. Maps to the Loans table.
public class Loan
{
    public int Id { get; set; }                          // primary key

    public int BorrowerId { get; set; }                  // foreign key
    public Borrower Borrower { get; set; } = null!;      // navigation property

    public int EquipmentId { get; set; }                 // foreign key
    public Equipment Equipment { get; set; } = null!;    // navigation property

    public DateTime BorrowedAt { get; set; }
    public bool IsReturned { get; set; }

    // ---------------------------------------------------------------
    // MIGRATION 2: when the README tells you, remove the two slashes
    // in front of the next line, then create the second migration.
    // ---------------------------------------------------------------
    public DateTime? ReturnedAt { get; set; }
}
