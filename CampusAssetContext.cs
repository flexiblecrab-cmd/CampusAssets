using Microsoft.EntityFrameworkCore;

// The context coordinates queries and changes for this unit of work.
public class CampusAssetContext : DbContext
{
    public DbSet<Borrower> Borrowers => Set<Borrower>();
    public DbSet<Equipment> EquipmentItems => Set<Equipment>();
    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // SQLite stores the database in the application's local folder.
        options.UseSqlite("Data Source=campus-assets.db");

        // To use SQL Server instead, 
        // options.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=CampusAssets;Trusted_Connection=True;TrustServerCertificate=True");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Borrower: required fields and an email that must be unique.
        modelBuilder.Entity<Borrower>()
            .Property(b => b.FullName)
            .HasMaxLength(120)
            .IsRequired();

        modelBuilder.Entity<Borrower>()
            .Property(b => b.Email)
            .HasMaxLength(200)
            .IsRequired();

        modelBuilder.Entity<Borrower>()
            .HasIndex(b => b.Email)
            .IsUnique();

        // Equipment: required fields and an asset tag that must be unique.
        modelBuilder.Entity<Equipment>()
            .Property(e => e.AssetTag)
            .HasMaxLength(40)
            .IsRequired();

        modelBuilder.Entity<Equipment>()
            .Property(e => e.Name)
            .HasMaxLength(120)
            .IsRequired();

        modelBuilder.Entity<Equipment>()
            .HasIndex(e => e.AssetTag)
            .IsUnique();

        // Loan: each loan belongs to one borrower and one equipment item.
        // Restrict means the database refuses to delete a borrower or an
        // equipment item that still has loans.
        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Borrower)
            .WithMany(b => b.Loans)
            .HasForeignKey(l => l.BorrowerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Equipment)
            .WithMany(e => e.Loans)
            .HasForeignKey(l => l.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Loan>()
            .Property(l => l.BorrowedAt)
            .IsRequired();
    }
}
