# Campus Equipment Loan Data Access Layer
# .NET Fundamentals Topic 3

Code based on CampusAssets practical code walkthrough with same ideas
(entity classes, a DbContext, SQLite, async CRUD, migrations) and grows
`EquipmentLoan` into three related tables.

## Files

| File | What it is |

| `Borrower.cs`, `Equipment.cs`, `Loan.cs` | The three entities (tables) |
| `LoanReport.cs` | A small shape for search results (projection) |
| `CampusAssetContext.cs` | The DbContext: tables, required fields, keys, relationships |
| `LoanService.cs` | The reusable data layer: CRUD, search, validation |
| `Program.cs` | Demo that runs everything and prints the results |

## Relationships

- Borrower 1 --- many Loans
- Equipment 1 --- many Loans

A `Loan` holds `BorrowerId` and `EquipmentId` (foreign keys). Deleting a borrower
or equipment that still has loans is blocked (`DeleteBehavior.Restrict`).

## 1. Put it in your existing CampusAssets project

Back up the folder first. Then, in `C:\Users\Dell Inspiron\Documents\CampusAssets`:

1. Delete the files from the previous version: the `Models`, `Data` and `Services`
   folders (if present), the old `Program.cs`, and any old `EquipmentLoan.cs`
   or `CampusAssetContext.cs`.
2. Delete the whole `Migrations` folder.
3. Delete `campus-assets.db` and `campus-loans.db` (the table design changed).
4. Copy the files from this folder (`.cs` files) into the project folder.

You already have the Sqlite and Design packages, so nothing new to install.

## 2. Migration 1: InitialCreate

```
dotnet ef migrations add InitialCreate
```

Open `Migrations\<timestamp>_InitialCreate.cs` and read the `Up` method.
Expected: `CreateTable` for Borrowers, EquipmentItems and Loans, foreign keys with
`ReferentialAction.Restrict`, and unique indexes on Email and AssetTag.
Nothing should be dropped.

```
dotnet ef database update
```

## 3. Migration 2: AddLoanReturnDate

1. In `Loan.cs`, remove the `//` before `public DateTime? ReturnedAt { get; set; }`.
2. In `LoanService.cs`, in `ReturnLoanAsync`, remove the `//` before
   `loan.ReturnedAt = DateTime.UtcNow;`.
3. Create and read the migration:

```
dotnet ef migrations add AddLoanReturnDate
```

   `Up` should contain only one `AddColumn<DateTime>` for `ReturnedAt` on `Loans`,
   with `nullable: true`.

4. Apply it and confirm both migrations:

```
dotnet ef database update
dotnet ef migrations list
```

## 4. Run

```
dotnet run
```

Expected output (IDs, tag and times will differ):

```
Schema version: 2026..........._AddLoanReturnDate

Created borrower 1, equipment 1, loan 1.

Rejected as expected: Loan the same laptop twice -> This equipment is already on loan.
Rejected as expected: Borrower with an empty name -> Full name is required.
Rejected as expected: Borrower with a bad email -> A valid email is required.
Rejected as expected: Delete a borrower who has loans -> This borrower has loans and cannot be deleted.

Search: borrower contains 'maria', still out
  #1 Maria Santos - Dell Laptop, borrowed ..., still out
...
Deleted the sample loan, equipment and borrower.
```

A line starting with `PROBLEM:` means a rule that should reject was accepted.
Record the `Schema version` line as your schema version.

To see `ReturnedAt` filled in: comment out the three `Delete...` lines near the end
of `Program.cs`, run once, and open `campus-assets.db` in DB Browser for SQLite.

## 5. How each requirement is met

| Requirement | Where |

| 3+ related entities, keys, relationships | `Borrower`, `Equipment`, `Loan`; `OnModelCreating` |
| DbContext with explicit configuration, required fields | `CampusAssetContext.cs` (`IsRequired`, `HasMaxLength`, `HasIndex`, `HasOne/WithMany`) |
| Async CRUD | `LoanService` methods ending in `Async` |
| Search by borrower, equipment, date range, return status | `SearchLoansAsync(...)` (all filters optional, combinable) |
| 2 migrations, not only EnsureCreated | `InitialCreate`, `AddLoanReturnDate`; `Program.cs` uses `MigrateAsync()` |
| AsNoTracking | All list and search queries |
| Projection | `SearchLoansAsync` selects into `LoanReport` |
| Validation | `if` checks at the start of each service method |
| Exception handling | `try/catch` in `Program.cs` (bad input, broken rules, database errors) |
| No incomplete transactions | `CheckOutAsync` uses a database transaction and rolls back on any failure |

## 6. Changing from SQLite to SQL Server

Only the provider and the connection string change. The entities and the service stay the same.

1. Install the SQL Server package:
   ```
   dotnet add package Microsoft.EntityFrameworkCore.SqlServer
   ```
2. In `CampusAssetContext.cs`, in `OnConfiguring`, replace the `UseSqlite(...)` line with:
   ```csharp
   options.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=CampusAssets;Trusted_Connection=True;TrustServerCertificate=True");
   ```

| | SQLite (development) | SQL Server (deployment) |

| Package | `Microsoft.EntityFrameworkCore.Sqlite` | `Microsoft.EntityFrameworkCore.SqlServer` |
| Method | `UseSqlite("Data Source=campus-assets.db")` | `UseSqlServer("Server=...;Database=...;...")` |
| Login | none (a file) | `Trusted_Connection=True` or `User Id=...;Password=...` |

3. Regenerate the migrations. The SQLite migration files contain SQLite-specific
   details, so do not reuse them. Move the `Migrations` folder away, then run
   `dotnet ef migrations add InitialCreate`, enable the `ReturnedAt` lines as in section 3,
   run `dotnet ef migrations add AddLoanReturnDate`, then `dotnet ef database update`.
4. For a real server, have the administrator review a script instead:
   ```
   dotnet ef migrations script --idempotent -o deploy.sql
   ```

## Limits of this version

- Dates are stored in UTC.
- The "already on loan" rule is checked in code inside a transaction. Two people
  pressing check-out at the exact same moment is not blocked by the database.
- The search uses `Contains`, so it is not case-sensitive for English letters in SQLite.

## Troubleshooting

- **table already exists:**
- **A migration named InitialCreate already exists:** 
- **Pending model changes:** 
- **dotnet ef not found:**
