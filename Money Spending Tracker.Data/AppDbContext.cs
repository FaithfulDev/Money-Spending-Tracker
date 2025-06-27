using Microsoft.EntityFrameworkCore;

namespace Money_Spending_Tracker.Data;

public class AppDbContext : DbContext
{
    public DbSet<Transaction> Transactions { get; set; }

    public DbSet<Account> Accounts { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// This constructor is used by EF Core migrations and should not be used directly.
    /// </summary>
    public AppDbContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Transaction>(
                eb =>
                {
                    eb.HasKey(t => t.TransactionId);
                });
    }
}
