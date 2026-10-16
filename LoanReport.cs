// A small read-only shape used for reports. The search query selects
// only these fields (projection) instead of loading whole entities.
public class LoanReport
{
    public int LoanId { get; set; }
    public string BorrowerName { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public DateTime BorrowedAt { get; set; }
    public bool IsReturned { get; set; }
}
