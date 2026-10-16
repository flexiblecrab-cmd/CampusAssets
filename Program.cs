using Microsoft.EntityFrameworkCore;

await using var db = new CampusAssetContext();
var service = new LoanService(db);

try
{
    // Apply the migrations.
    await db.Database.MigrateAsync();
    Console.WriteLine("Schema version: " + await service.GetSchemaVersionAsync());
    Console.WriteLine();

    // A random piece of text so the demo can run many times without duplicates.
    string tag = Guid.NewGuid().ToString("N").Substring(0, 6);

    // ---- CREATE ----
    int borrowerId = await service.AddBorrowerAsync("Maria Santos", $"maria.{tag}@campus.edu");
    int equipmentId = await service.AddEquipmentAsync($"LAP-{tag}", "Dell Laptop");
    int loanId = await service.CheckOutAsync(borrowerId, equipmentId);
    Console.WriteLine($"Created borrower {borrowerId}, equipment {equipmentId}, loan {loanId}.");
    Console.WriteLine();

    // ---- VALIDATION: these must all be rejected ----
    await ExpectErrorAsync("Loan the same laptop twice",
        () => service.CheckOutAsync(borrowerId, equipmentId));
    await ExpectErrorAsync("Borrower with an empty name",
        () => service.AddBorrowerAsync("   ", "empty@campus.edu"));
    await ExpectErrorAsync("Borrower with a bad email",
        () => service.AddBorrowerAsync("Test Person", "not-an-email"));
    await ExpectErrorAsync("Delete a borrower who has loans",
        () => service.DeleteBorrowerAsync(borrowerId));
    Console.WriteLine();

    // ---- READ / SEARCH ----
    Console.WriteLine("Search: borrower contains 'maria', still out");
    PrintLoans(await service.SearchLoansAsync(borrowerName: "maria", isReturned: false));

    Console.WriteLine("Search: equipment contains 'laptop', borrowed today");
    DateTime today = DateTime.UtcNow;
    PrintLoans(await service.SearchLoansAsync(equipmentName: "laptop", borrowedFrom: today, borrowedTo: today));

    // ---- UPDATE ----
    await service.UpdateBorrowerNameAsync(borrowerId, "Maria Santos-Reyes");
    await service.ReturnLoanAsync(loanId);
    Console.WriteLine("Updated the borrower's name and returned the laptop.");
    Console.WriteLine();

    Console.WriteLine("Search: returned loans");
    PrintLoans(await service.SearchLoansAsync(borrowerName: "santos", isReturned: true));

    // ---- DELETE (the loan first, because the others depend on it) ----
    await service.DeleteLoanAsync(loanId);
    await service.DeleteEquipmentAsync(equipmentId);
    await service.DeleteBorrowerAsync(borrowerId);
    Console.WriteLine("Deleted the sample loan, equipment and borrower.");
}
catch (ArgumentException ex)
{
    Console.WriteLine("Invalid input: " + ex.Message);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine("Rule broken: " + ex.Message);
}
catch (DbUpdateException ex)
{
    Console.WriteLine("Database error: " + ex.InnerException?.Message);
}
catch (Exception ex)
{
    Console.WriteLine("Unexpected error: " + ex.Message);
}

// ---- helper methods used above ----

static void PrintLoans(List<LoanReport> loans)
{
    if (loans.Count == 0)
        Console.WriteLine("  (no results)");

    foreach (var loan in loans)
    {
        string status = loan.IsReturned ? "returned" : "still out";
        Console.WriteLine($"  #{loan.LoanId} {loan.BorrowerName} - {loan.EquipmentName}, borrowed {loan.BorrowedAt:yyyy-MM-dd HH:mm}, {status}");
    }

    Console.WriteLine();
}

static async Task ExpectErrorAsync(string description, Func<Task> action)
{
    try
    {
        await action();
        Console.WriteLine($"PROBLEM: '{description}' was accepted but should be rejected.");
    }
    catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
    {
        Console.WriteLine($"Rejected as expected: {description} -> {ex.Message}");
    }
}
