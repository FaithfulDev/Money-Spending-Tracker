using Microsoft.EntityFrameworkCore;
using Money_Spending_Tracker.Data;
using Money_Spending_Tracker.Features.Api;
using Money_Spending_Tracker.Features.Database;
using Money_Spending_Tracker.Features.Settings;
using Money_Spending_Tracker.Features.Storage;
using System.Diagnostics;
using static Money_Spending_Tracker.Features.TransactionData.ITransactionDataService;

namespace Money_Spending_Tracker.Features.TransactionData;

internal class TransactionDataService : ITransactionDataService
{
    public event AccountUpdateStartedHandler? AccountUpdateStarted;
    public event EventHandler? TimeoutOccurred;

    private readonly DatabaseService _databaseService;

    // We assume that requisitions expire after 90 days.
    private const int REQUISTION_LIFETIME_DAYS = 90;

    private List<(Guid accountId, bool isLinked, int expiresInDays)> _accountLinkStatusCache = [];

    public TransactionDataService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<List<(Guid accountId, bool isLinked, int expiresInDays)>> CheckAccountLinkStatus()
    {
        List<(Guid accountId, bool isLinked, int expiresInDays)> results = [];

        var dbContext = _databaseService.CreateDbContext();
        var accountIds = dbContext.Accounts.Select(a => a.AccountId);

        var apiClient = new Client(new());
        var requisitions = (await apiClient.Retrieve_all_requisitionsAsync()).Results;

        foreach (var accountId in accountIds)
        {
            var requisition = requisitions.FirstOrDefault(r => r.Accounts.Contains(accountId) && r.Status == StatusEnum.LN);

            if (requisition == null)
            {
                // Account is not linked
                results.Add((accountId, isLinked: false, expiresInDays: 0));
                continue;
            }

            if (requisition.Created == null)
            {
                // If the requisition has no created date, we cannot determine the expiration
                results.Add((accountId, isLinked: true, expiresInDays: 0));
                continue;
            }

            // Account is linked, check the expiration date
            var expiresInDays = (int)Math.Floor(
                REQUISTION_LIFETIME_DAYS - (DateTimeOffset.UtcNow - (DateTimeOffset)requisition.Created).TotalDays
            );

            results.Add((accountId, isLinked: true, expiresInDays));
        }

        return results;
    }

    public async Task UpdateCacheAsync()
    {
        var dbContext = _databaseService.CreateDbContext();
        var now = DateTime.UtcNow;

        var balance = await GetMonthsBalance(dbContext, now);
        var spendings = await GetMonthsSpendings(dbContext, now);

        AppCache.CurrentBalance = balance;
        AppCache.RemainingMonthlyBudget = AppSettings.MonthlyBudget + spendings;

#if ANDROID
        MainApplication.TriggerWidgetUpdate();
#endif
    }

    public async Task UpdateTransactionsAndCacheAsync()
    {
        _accountLinkStatusCache = await CheckAccountLinkStatus();

        await UpdateRecentTransactionsAsync();
        await UpdateCacheAsync();
    }

    private async Task UpdateRecentTransactionsAsync()
    {
        using var dbContext = _databaseService.CreateDbContext();

        var HundredDaysAgo = DateTime.UtcNow.AddDays(-100);

        var apiClient = new Client(new() { Timeout = TimeSpan.FromSeconds(120) });
        var accounts = await dbContext.Accounts.Select(a => new { a.AccountId, a.AccountIban }).ToListAsync();

        foreach (var account in accounts)
        {
            OnAccountUpdateStarted(account.AccountIban);

            var recentTransactionIds = new HashSet<Guid>(
                await dbContext.Transactions
                    .Where(t => t.ValueDate >= HundredDaysAgo)
                    .Where(t => t.AccountId == account.AccountId)
                    .Select(t => t.InternalTransactionId)
                    .ToListAsync()
            );

            var transactionsToAdd =
                await GetNewTransactionsForAccountAsync(recentTransactionIds, apiClient, account.AccountId);

            if (transactionsToAdd.Count == 0)
            {
                continue; // No new transactions for this account
            }

            // Add new transactions to the database
            foreach (var transaction in transactionsToAdd)
            {
                var newTransaction = new Transaction(
                    transactionId: transaction.TransactionId,
                    accountId: account.AccountId,
                    entryReference: transaction.EntryReference,
                    endToEndId: transaction.EndToEndId,
                    bookingDate: DateTime.ParseExact(
                        transaction.BookingDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                    valueDate: DateTime.ParseExact(
                        transaction.ValueDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                    transactionAmount: double.Parse(transaction.TransactionAmount.Amount),
                    creditorName: transaction.CreditorName,
                    ultimateCreditor: transaction.UltimateCreditor,
                    remittanceInformationStructured: transaction.RemittanceInformationStructured,
                    additionalInformation: transaction.AdditionalInformation,
                    purposeCode: transaction.PurposeCode,
                    proprietaryBankTransactionCode: transaction.ProprietaryBankTransactionCode,
                    internalTransactionId: Guid.Parse(transaction.InternalTransactionId)
                );

                dbContext.Transactions.Add(newTransaction);
            }
        }

        await dbContext.SaveChangesAsync();

        AppCache.LastTransactionUpdate = DateTimeOffset.UtcNow;
    }

    private async Task<List<TransactionSchema>> GetNewTransactionsForAccountAsync(
        HashSet<Guid> recentTransactionIds, Client apiClient, Guid accountId)
    {
        try
        {
            if (!_accountLinkStatusCache.Any(ls => ls.accountId == accountId && ls.isLinked))
            {
                // No valid requisition for this account, skip updating transactions
                return [];
            }

            var newTransactions = await apiClient.Retrieve_account_transactionsAsync(
                accountId.ToString());

            var transactionsToAdd = newTransactions.Transactions.Booked
                .Where(t => !recentTransactionIds.Contains(Guid.Parse(t.InternalTransactionId)))
                .ToList();

            return transactionsToAdd;
        }
        catch (ApiException ex)
        {
            Debug.WriteLine($"ApiException while retrieving transactions: {ex.Message}");

            // We ignore any API exceptions here.
            // Requisitions are checked by a different mechanism, so we can safely ignore this.
            // Transactions can't always be updated, e.g. rate limiting, banking maintenance, etc.
        }
        catch (System.Net.WebException ex)
        {
            // We show a warning to the user if there is a WebException, but we don't throw it.
            Debug.WriteLine($"WebException while retrieving transactions: {ex.Message}");
            OnTimeoutOccurred();
        }
        catch (OperationCanceledException)
        {
            // Operation was cancelled, we can ignore this
            return [];
        }

        return [];
    }

    protected virtual void OnAccountUpdateStarted(string accountBeingUpdated)
    {
        AccountUpdateStarted?.Invoke(this, new(accountBeingUpdated));
    }

    protected virtual void OnTimeoutOccurred()
    {
        TimeoutOccurred?.Invoke(this, EventArgs.Empty);
    }

    private static async Task<double> GetMonthsBalance(AppDbContext dbContext, DateTime dateTime)
    {
        var balanceOrSpendings = await dbContext.Transactions
            .Where(t => t.ValueDate.Year == dateTime.Year && t.ValueDate.Month == dateTime.Month)
            .SumAsync(t => t.TransactionAmount);

        return balanceOrSpendings;
    }

    private static async Task<double> GetMonthsSpendings(AppDbContext dbContext, DateTime dateTime)
    {
        var spendings = await dbContext.Transactions
            .Where(t => t.ValueDate.Year == dateTime.Year && t.ValueDate.Month == dateTime.Month)
            .Where(t => t.TransactionAmount < 0)
            .SumAsync(t => t.TransactionAmount);

        return spendings;
    }
}
