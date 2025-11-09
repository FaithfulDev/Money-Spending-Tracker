using Microsoft.EntityFrameworkCore;

namespace Money_Spending_Tracker.Data;

public class AppDbContext : DbContext
{
    public DbSet<Transaction> Transactions { get; set; }

    public DbSet<Account> Accounts { get; set; }

    public DbSet<JobLog> JobLogs { get; set; }

    public DbSet<Tag> Tags { get; set; }

    public DbSet<TransactionTag> TransactionTags { get; set; }

    public DbSet<TagNegativeEmbedding> TagNegativeEmbeddings { get; set; }

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
        modelBuilder.Entity<Transaction>()
            .HasKey(t => new { t.InternalTransactionId, t.AccountId });

        modelBuilder.Entity<Account>()
            .HasKey(a => a.AccountId);

        modelBuilder.Entity<TransactionTag>()
            .HasKey(tt => new { tt.InternalTransactionId, tt.AccountId, tt.TagId });

        modelBuilder.Entity<TransactionTag>()
            .HasOne(tt => tt.Transaction)
            .WithMany(t => t.TransactionTags)
            .HasForeignKey(tt => new { tt.InternalTransactionId, tt.AccountId })
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TransactionTag>()
            .HasOne(tt => tt.Tag)
            .WithMany(t => t.TransactionTags)
            .HasForeignKey(tt => tt.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TagNegativeEmbedding>()
            .HasOne(tne => tne.Tag)
            .WithMany(t => t.NegativeEmbeddings)
            .HasForeignKey(tne => tne.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
