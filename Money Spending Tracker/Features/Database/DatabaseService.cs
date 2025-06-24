using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Money_Spending_Tracker.Data;
using Money_Spending_Tracker.Features.Storage;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

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

        //TODO remove debug code
        password = "test";

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
        var storedPassword = await SecureStorage.GetAsync(StorageKeys.DB_PASSWORD);

        if (string.IsNullOrEmpty(storedPassword))
        {
            return false;
        }

        if (storedPassword != password)
        {
            return false;
        }

        _password = password;

        await InitializeDatabaseAsync();

        return true;
    }

    private async Task InitializeDatabaseAsync()
    {
        using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();
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
            .Options;

        return new AppDbContext(options);
    }

    public void Lock()
    {
        _password = null;
    }

    public bool DoesDatabaseExist()
    {
        //File.Delete(_dbPath); // For testing purposes, delete the database file if it exists
        return File.Exists(_dbPath);
    }
}
