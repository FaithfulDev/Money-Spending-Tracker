using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Money_Spending_Tracker.Data;
using Money_Spending_Tracker.Features.Storage;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using System.Diagnostics;

namespace Money_Spending_Tracker.Features.Database;

public class DatabaseService
{
    private string? _password;
    private readonly string _dbPath = Path.Combine(FileSystem.AppDataDirectory, "money_spending_tracker.db");

    /// <summary>
    /// Unlocks the database using fingerprint authentication and initializes it.
    /// </summary>
    public async Task<bool> UnlockAndInitializeAsync()
    {
        var authResult = await CrossFingerprint.Current.AuthenticateAsync(
            new AuthenticationRequestConfiguration("Unlock", "Authenticate to access your data"));

        if (!authResult.Authenticated)
        {
            return false;
        }

        var password = await SecureStorage.GetAsync(StorageKeys.DB_PASSWORD);

        if (string.IsNullOrEmpty(password))
        {
            return false;
        }

        _password = password;

        await InitializeDatabaseAsync();

        return true;
    }

    /// <summary>
    /// Unlocks the database with a provided password and initializes it.
    /// </summary>
    public async Task<bool> UnlockAndInitializeAsync(string password)
    {
        try
        {
            _password = password;
            await InitializeDatabaseAsync();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 26) // SQLITE_NOTADB
        {
            // This exception indicates that the database file is not a valid SQLite database.
            // In our case it means the password is incorrect or the database file is corrupted.
            _password = null;
            return false;
        }
        catch (Exception)
        {
            _password = null;
            throw;
        }

        await SecureStorage.SetAsync(StorageKeys.DB_PASSWORD, password);

        return true;
    }

    private async Task InitializeDatabaseAsync()
    {
        using var context = CreateDbContext();
        await context.Database.MigrateAsync();

#if DEBUG
        SeedData(context);
#endif
    }

    public AppDbContext CreateDbContext()
    {
        if (string.IsNullOrEmpty(_password))
        {
            throw new InvalidOperationException("Database is not unlocked.");
        }

        var connectionString = $"Data Source={_dbPath};Password={_password};";

        var connection = new SqliteConnection(connectionString);
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
#if DEBUG
            .LogTo(message => Debug.WriteLine(message, "ef core"), LogLevel.Information)
            .EnableSensitiveDataLogging()
#endif
            .Options;

        return new AppDbContext(options);
    }

    public void Lock()
    {
        _password = null;
    }

    public bool DoesDatabaseExist()
    {
        return File.Exists(_dbPath);
    }

    public void DeleteDatabase()
    {
        if (DoesDatabaseExist())
        {
            File.Delete(_dbPath);
        }
    }

#if DEBUG
    private static void SeedData(AppDbContext dbContext)
    {
        Account? seedAccount = dbContext.Accounts.FirstOrDefault(a => a.AccountName == "Test Account");

        if (seedAccount != null)
        {
            // If the test account already exists, we do not need to seed data again.
            return;
        }

        seedAccount = new(
            accountId: Guid.NewGuid(),
            accountName: "Test Account",
            accountIban: "DE89370400440532013000",
            institutionId: "SANDBOXFINANCE_SFIN0000",
            institutionName: "Sandbox Finance",
            institutionLogo: null,
            institutionBic: "SFIN0000"
        );

        dbContext.Accounts.Add(seedAccount);

        DateTime start = DateTime.Now;
        Random random = new();

        for (int i = 0; i < 72; i++)
        {
            var transactionAmountBase = random.Next(14, 340);
            var transactionAmountDouble = random.NextDouble();
            var transactionAmountCombined = transactionAmountBase + transactionAmountDouble + transactionAmountDouble / 10;

            if (i % 2 != 0)
            {
                transactionAmountCombined *= -1;
            }

            for (int j = 0; j < 3; j++)
            {
                Transaction transaction = new(
                    transactionId: Guid.NewGuid().ToString(),
                    accountId: seedAccount.AccountId,
                    entryReference: $"TEST-{i}-{j}",
                    endToEndId: string.Empty,
                    bookingDate: start.AddMonths(-i).AddDays(j),
                    valueDate: start.AddMonths(-i).AddDays(j),
                    transactionAmount: transactionAmountCombined + random.Next(2, 9),
                    creditorName: "Test Creditor",
                    ultimateCreditor: "Ultimate Creditor",
                    remittanceInformationStructured: $"Structured Info {i}-{j}",
                    additionalInformation: $"Additional Info {i}-{j}",
                    purposeCode: "PURPOSE",
                    proprietaryBankTransactionCode: "PROPRIETARY",
                    internalTransactionId: Guid.NewGuid()
                );

                dbContext.Transactions.Add(transaction);
            }
        }

        dbContext.SaveChanges();
    }
#endif
}
