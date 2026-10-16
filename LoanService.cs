using Microsoft.EntityFrameworkCore;

// All database work lives here, so any program can reuse it.
// Every method is async and checks its input before touching the database.
public class LoanService
{
    private readonly CampusAssetContext _db;

    public LoanService(CampusAssetContext db)
    {
        _db = db;
    }

    // ================= BORROWERS =================

    // CREATE
    public async Task<int> AddBorrowerAsync(string fullName, string email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("A valid email is required.");

        email = email.Trim().ToLower();

        bool emailTaken = await _db.Borrowers.AnyAsync(b => b.Email == email);
        if (emailTaken)
            throw new InvalidOperationException("A borrower with this email already exists.");

        var borrower = new Borrower { FullName = fullName.Trim(), Email = email };
        _db.Borrowers.Add(borrower);
        await _db.SaveChangesAsync();
        return borrower.Id;
    }

    // READ: only a report, so AsNoTracking.
    public async Task<List<Borrower>> ListBorrowersAsync()
    {
        return await _db.Borrowers
            .AsNoTracking()
            .OrderBy(b => b.FullName)
            .ToListAsync();
    }

    // UPDATE: load the tracked borrower, change it, save.
    public async Task UpdateBorrowerNameAsync(int borrowerId, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Full name is required.");

        var borrower = await _db.Borrowers.SingleOrDefaultAsync(b => b.Id == borrowerId);
        if (borrower is null)
            throw new InvalidOperationException("Borrower not found.");

        borrower.FullName = newName.Trim();
        await _db.SaveChangesAsync();
    }

    // DELETE: refuse if the borrower still has loans.
    public async Task DeleteBorrowerAsync(int borrowerId)
    {
        var borrower = await _db.Borrowers.SingleOrDefaultAsync(b => b.Id == borrowerId);
        if (borrower is null)
            throw new InvalidOperationException("Borrower not found.");

        bool hasLoans = await _db.Loans.AnyAsync(l => l.BorrowerId == borrowerId);
        if (hasLoans)
            throw new InvalidOperationException("This borrower has loans and cannot be deleted.");

        _db.Borrowers.Remove(borrower);
        await _db.SaveChangesAsync();
    }

    // ================= EQUIPMENT =================

    // CREATE
    public async Task<int> AddEquipmentAsync(string assetTag, string name)
    {
        if (string.IsNullOrWhiteSpace(assetTag))
            throw new ArgumentException("Asset tag is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Equipment name is required.");

        assetTag = assetTag.Trim().ToUpper();

        bool tagTaken = await _db.EquipmentItems.AnyAsync(e => e.AssetTag == assetTag);
        if (tagTaken)
            throw new InvalidOperationException("This asset tag already exists.");

        var equipment = new Equipment { AssetTag = assetTag, Name = name.Trim() };
        _db.EquipmentItems.Add(equipment);
        await _db.SaveChangesAsync();
        return equipment.Id;
    }

    // READ
    public async Task<List<Equipment>> ListEquipmentAsync()
    {
        return await _db.EquipmentItems
            .AsNoTracking()
            .OrderBy(e => e.AssetTag)
            .ToListAsync();
    }

    // UPDATE
    public async Task RenameEquipmentAsync(int equipmentId, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Equipment name is required.");

        var equipment = await _db.EquipmentItems.SingleOrDefaultAsync(e => e.Id == equipmentId);
        if (equipment is null)
            throw new InvalidOperationException("Equipment not found.");

        equipment.Name = newName.Trim();
        await _db.SaveChangesAsync();
    }

    // DELETE: refuse if the item has any loans.
    public async Task DeleteEquipmentAsync(int equipmentId)
    {
        var equipment = await _db.EquipmentItems.SingleOrDefaultAsync(e => e.Id == equipmentId);
        if (equipment is null)
            throw new InvalidOperationException("Equipment not found.");

        bool hasLoans = await _db.Loans.AnyAsync(l => l.EquipmentId == equipmentId);
        if (hasLoans)
            throw new InvalidOperationException("This equipment has loans and cannot be deleted.");

        _db.EquipmentItems.Remove(equipment);
        await _db.SaveChangesAsync();
    }

    // ================= LOANS =================

    // CREATE: check out equipment.
    // The transaction makes the checks and the save one all-or-nothing step.
    // If anything fails before Commit, nothing is saved.
    public async Task<int> CheckOutAsync(int borrowerId, int equipmentId)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();

        bool borrowerExists = await _db.Borrowers.AnyAsync(b => b.Id == borrowerId);
        if (!borrowerExists)
            throw new InvalidOperationException("Borrower not found.");

        bool equipmentExists = await _db.EquipmentItems.AnyAsync(e => e.Id == equipmentId);
        if (!equipmentExists)
            throw new InvalidOperationException("Equipment not found.");

        bool alreadyOut = await _db.Loans.AnyAsync(l => l.EquipmentId == equipmentId && !l.IsReturned);
        if (alreadyOut)
            throw new InvalidOperationException("This equipment is already on loan.");

        var loan = new Loan
        {
            BorrowerId = borrowerId,
            EquipmentId = equipmentId,
            BorrowedAt = DateTime.UtcNow,
            IsReturned = false
        };

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return loan.Id;
    }

    // UPDATE: mark a loan as returned.
    public async Task ReturnLoanAsync(int loanId)
    {
        var loan = await _db.Loans.SingleOrDefaultAsync(l => l.Id == loanId);
        if (loan is null)
            throw new InvalidOperationException("Loan not found.");

        if (loan.IsReturned)
            throw new InvalidOperationException("This loan was already returned.");

        loan.IsReturned = true;

        // MIGRATION 2: 
           loan.ReturnedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    // DELETE: only returned loans can be deleted.
    public async Task DeleteLoanAsync(int loanId)
    {
        var loan = await _db.Loans.SingleOrDefaultAsync(l => l.Id == loanId);
        if (loan is null)
            throw new InvalidOperationException("Loan not found.");

        if (!loan.IsReturned)
            throw new InvalidOperationException("Return the equipment before deleting the loan.");

        _db.Loans.Remove(loan);
        await _db.SaveChangesAsync();
    }

    // SEARCH: every filter is optional. Leave one as null to skip it.
    // The query is built one filter at a time, and runs once at ToListAsync.
    public async Task<List<LoanReport>> SearchLoansAsync(
        string? borrowerName = null,
        string? equipmentName = null,
        DateTime? borrowedFrom = null,
        DateTime? borrowedTo = null,
        bool? isReturned = null)
    {
        if (borrowedFrom != null && borrowedTo != null && borrowedFrom.Value.Date > borrowedTo.Value.Date)
            throw new ArgumentException("The 'from' date cannot be after the 'to' date.");

        IQueryable<Loan> query = _db.Loans.AsNoTracking();

       if (!string.IsNullOrWhiteSpace(borrowerName))
    {
       string borrowerText = borrowerName.Trim().ToLower();
       query = query.Where(l => l.Borrower.FullName.ToLower().Contains(borrowerText));
    }

       if (!string.IsNullOrWhiteSpace(equipmentName))
    {
       string equipmentText = equipmentName.Trim().ToLower();
       query = query.Where(l => l.Equipment.Name.ToLower().Contains(equipmentText)
                          || l.Equipment.AssetTag.ToLower().Contains(equipmentText));
    }
        if (borrowedFrom != null)
        {
            var start = borrowedFrom.Value.Date;
            query = query.Where(l => l.BorrowedAt >= start);
        }

        if (borrowedTo != null)
        {
            // Add one day so the whole "to" day is included.
            var end = borrowedTo.Value.Date.AddDays(1);
            query = query.Where(l => l.BorrowedAt < end);
        }

        if (isReturned != null)
            query = query.Where(l => l.IsReturned == isReturned.Value);

        // Projection: select only the fields the report needs.
        return await query
            .OrderByDescending(l => l.BorrowedAt)
            .Select(l => new LoanReport
            {
                LoanId = l.Id,
                BorrowerName = l.Borrower.FullName,
                EquipmentName = l.Equipment.Name,
                BorrowedAt = l.BorrowedAt,
                IsReturned = l.IsReturned
            })
            .ToListAsync();
    }

    // The last applied migration is the schema version.
    public async Task<string> GetSchemaVersionAsync()
    {
        var applied = await _db.Database.GetAppliedMigrationsAsync();
        return applied.LastOrDefault() ?? "(no migrations applied)";
    }
}
