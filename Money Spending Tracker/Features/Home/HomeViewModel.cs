using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Money_Spending_Tracker.Data;
using Money_Spending_Tracker.Features.Api;
using Money_Spending_Tracker.Features.Database;
using Money_Spending_Tracker.Features.Settings;
using Money_Spending_Tracker.Features.Storage;
using System.Diagnostics;

namespace Money_Spending_Tracker.Features.Home;

internal partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isWorking = false;

    [ObservableProperty]
    private bool _pullToRefreshIndicatorVisible = false;

    [ObservableProperty]
    private string? _remainingBudgetString;

    [ObservableProperty]
    private Color _remainingBudgetColor = Colors.Black;

    [ObservableProperty]
    private string? _balanceString;

    [ObservableProperty]
    private Color __balanceColor = Colors.Black;

    [ObservableProperty]
    private bool _didTimeout = false;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private string? _accountBeingUpdated = null;

    private readonly DatabaseService _databaseService;

    public HomeViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task StartAsync()
    {
        Title = DateTime.Now.ToString("MMMM yyyy", System.Globalization.CultureInfo.CurrentCulture);
        RemainingBudgetString = FormatCurrency(AppCache.RemainingMonthlyBudget);
        RemainingBudgetColor = AppCache.RemainingMonthlyBudget < 0 ? Colors.Red : Colors.Black;
        BalanceString = FormatCurrency(AppCache.CurrentBalance);
        BalanceColor = AppCache.CurrentBalance < 0 ? Colors.Red : Colors.Green;

        if (AppCache.LastTransactionUpdate.UtcDateTime.Date == DateTime.UtcNow.Date)
        {
            // Already updated today, skip updating transactions and widget
            return;
        }

        if (AppCache.LastTransactionUpdate.UtcDateTime.Date.Month != DateTime.UtcNow.Date.Month ||
           AppCache.LastTransactionUpdate.UtcDateTime.Date.Year != DateTime.UtcNow.Date.Year)
        {
            // If the last transaction update is not in the current month, reset the remaining budget
            AppCache.RemainingMonthlyBudget = AppSettings.MonthlyBudget;
            AppCache.CurrentBalance = 0;

            RemainingBudgetString = FormatCurrency(AppCache.RemainingMonthlyBudget);
            RemainingBudgetColor = Colors.Black;
            BalanceString = FormatCurrency(0);
            BalanceColor = Colors.Black;
        }

        await Refresh();
    }

    private async Task UpdateTransactionsAndWidgetAsync()
    {
        // Reset the timeout flag
        DidTimeout = false;

        await UpdateRecentTransactionsAsync();
        await UpdateWidgetCacheAsync();

        var balance = await GetThisMonthsBalance(_databaseService.CreateDbContext());

        RemainingBudgetString = FormatCurrency(AppCache.RemainingMonthlyBudget);
        RemainingBudgetColor = AppCache.RemainingMonthlyBudget < 0 ? Colors.Red : Colors.Black;
        BalanceString = FormatCurrency(balance);
        BalanceColor = balance < 0 ? Colors.Red : Colors.Green;

        AppCache.CurrentBalance = balance;
    }

    private async Task UpdateRecentTransactionsAsync()
    {
        using var dbContext = _databaseService.CreateDbContext();

        var HundredDaysAgo = DateTime.UtcNow.AddDays(-100);

        var apiClient = new Client(new() { Timeout = TimeSpan.FromSeconds(120) });
        var accounts = await dbContext.Accounts.Select(a => new { a.AccountId, a.AccountIban }).ToListAsync();

        foreach (var account in accounts)
        {
            AccountBeingUpdated = account.AccountIban;

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

        AccountBeingUpdated = null;

        // Update the last transaction update time. We set it to one day ago to avoid missing transactions.
        AppCache.LastTransactionUpdate = DateTimeOffset.UtcNow;
    }

    private async Task<List<TransactionSchema>> GetNewTransactionsForAccountAsync(
        HashSet<Guid> recentTransactionIds, Client apiClient, Guid accountId)
    {
        try
        {
            var requisitions = (await apiClient.Retrieve_all_requisitionsAsync()).Results;

            if (!requisitions.Any(r => r.Accounts.Contains(accountId) && r.Status == StatusEnum.LN))
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
        catch (ApiException)
        {
            // We ignore any API exceptions here.
            // Requisitions are checked by a different mechanism, so we can safely ignore this.
            // Transactions can't always be updated, e.g. rate limiting, banking maintenance, etc.
        }
        catch (System.Net.WebException ex)
        {
            // We show a warning to the user if there is a WebException, but we don't throw it.
            Debug.WriteLine($"WebException while retrieving transactions: {ex.Message}");
            DidTimeout = true;
        }
        catch (OperationCanceledException)
        {
            // Operation was cancelled, we can ignore this
            return [];
        }

        return [];
    }

    private async Task UpdateWidgetCacheAsync()
    {
        using var dbContext = _databaseService.CreateDbContext();
        var spendings = await GetThisMonthsBalance(dbContext, onlySpendings: true);

        AppCache.RemainingMonthlyBudget = AppSettings.MonthlyBudget - Math.Abs(spendings);

#if ANDROID
        MainApplication.TriggerWidgetUpdate();
#endif
    }

    private static async Task<double> GetThisMonthsBalance(AppDbContext dbContext, bool onlySpendings = false)
    {
        var now = DateTime.UtcNow;
        var balanceOrSpendings = await dbContext.Transactions
            .Where(t => t.ValueDate.Year == now.Year && t.ValueDate.Month == now.Month)
            .Where(t => !onlySpendings || (onlySpendings && t.TransactionAmount < 0))
            .SumAsync(t => t.TransactionAmount);

        return balanceOrSpendings;
    }

    [RelayCommand]
    private async Task Refresh()
    {
        PullToRefreshIndicatorVisible = false;

        if (IsWorking)
        {
            return;
        }

        IsWorking = true;

        //TODO: Check if there are any requisitions that are expired or to expire soon and notify the user. (maybe parallelize this)

        await UpdateTransactionsAndWidgetAsync();

        IsWorking = false;
    }

    private static string FormatCurrency(double amount)
    {
        return amount.ToString("C2", System.Globalization.CultureInfo.CurrentCulture);
    }
}
