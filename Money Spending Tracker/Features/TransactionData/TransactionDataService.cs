using Microsoft.EntityFrameworkCore;
using Money_Spending_Tracker.Data;
using Money_Spending_Tracker.Features.Api;
using Money_Spending_Tracker.Features.Database;
using Money_Spending_Tracker.Features.Settings;
using Money_Spending_Tracker.Features.Storage;
using Money_Spending_Tracker.Features.Tags;
using System.Diagnostics;
using static Money_Spending_Tracker.Features.TransactionData.ITransactionDataService;

namespace Money_Spending_Tracker.Features.TransactionData;

internal class TransactionDataService : ITransactionDataService
{
    public event AccountUpdateStartedHandler? AccountUpdateStarted;
    public event AccountUpdateProgressHandler? AccountUpdateProgress;
    public event EventHandler? TimeoutOccurred;
    public event EventHandler? TransactionUpdateEnded;

    private readonly DatabaseService _databaseService;
    private readonly SemaphoreSlim _updateLock = new(initialCount: 1, maxCount: 1);

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

        var balance = await GetMonthsBalance(dbContext, new DateOnly(now.Year, now.Month, 1));
        var spendings = await GetMonthsSpendings(dbContext, new DateOnly(now.Year, now.Month, 1));
        var tagBalances = await GetTagBalances(dbContext, new DateOnly(now.Year, now.Month, 1));

        AppCache.CurrentBalance = balance;
        AppCache.RemainingMonthlyBudget = AppSettings.MonthlyBudget + spendings;

        //TODO store tag balances in cache

#if ANDROID
        MainApplication.TriggerWidgetUpdate();
#endif
    }

    public async Task UpdateTransactionsAndCacheAsync()
    {
        if (!await _updateLock.WaitAsync(0))
        {
            // If the lock is already held, we return early to prevent concurrent updates.
            Debug.WriteLine("UpdateTransactionsAndCacheAsync is already running, skipping this call.");
            return;
        }

        try
        {
            _accountLinkStatusCache = await CheckAccountLinkStatus();

            await Task.Run(UpdateRecentTransactionsAsync);
            await UpdateCacheAsync();

            await OnTransactionUpdateEnded();
        }
        finally
        {
            _updateLock.Release();
        }
    }

    private async Task UpdateRecentTransactionsAsync()
    {
        using var dbContext = _databaseService.CreateDbContext();

        var HundredDaysAgo = DateTime.UtcNow.AddDays(-100);

        var apiClient = new Client(new() { Timeout = TimeSpan.FromSeconds(120) });
        var accounts = await dbContext.Accounts.Select(a => new { a.AccountId, a.AccountIban }).ToListAsync();

        List<Transaction> newTransactions = [];

        foreach (var account in accounts)
        {
            await OnAccountUpdateStarted(account.AccountIban);
            await OnAccountUpdateProgress("Fetching transactions...");

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

            await OnAccountUpdateProgress($"Processing 0/{transactionsToAdd.Count} (0%)");

            using var tagPredictionService = new TagPredictionService();
            await tagPredictionService.InitializeAsync();

            // Add new transactions to the database
            foreach (var transaction in transactionsToAdd)
            {
                byte[]? embeddingBytes = null;

                var combinedText =
                    (transaction.UltimateCreditor ?? transaction.CreditorName ?? transaction.DebtorName ?? "") +
                    " " + (transaction.RemittanceInformationStructured ?? transaction.RemittanceInformationUnstructured ?? "" +
                    " " + (transaction.AdditionalInformation ?? ""));

                if (!string.IsNullOrWhiteSpace(combinedText))
                {
                    var embedding = tagPredictionService.GetTransactionEmbedding(
                        combinedText);

                    embeddingBytes = ByteFloatConversionHelper.FloatArrayToByteArray(embedding);
                }

                var newTransaction = new Transaction(
                    transactionId: transaction.TransactionId,
                    accountId: account.AccountId,
                    entryReference: transaction.EntryReference,
                    endToEndId: transaction.EndToEndId,
                    bookingDate: DateTime.ParseExact(
                        transaction.BookingDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                    valueDate: DateTime.ParseExact(
                        transaction.ValueDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                    transactionAmount: double.Parse(
                        transaction.TransactionAmount.Amount,
                        System.Globalization.CultureInfo.InvariantCulture
                    ),
                    creditorName: transaction.CreditorName,
                    ultimateCreditor: transaction.UltimateCreditor,
                    debtorName: transaction.DebtorName,
                    remittanceInformationStructured: transaction.RemittanceInformationStructured,
                    remittanceInformationUnstructured: transaction.RemittanceInformationUnstructured,
                    additionalInformation: transaction.AdditionalInformation,
                    purposeCode: transaction.PurposeCode,
                    proprietaryBankTransactionCode: transaction.ProprietaryBankTransactionCode,
                    internalTransactionId: Guid.Parse(transaction.InternalTransactionId),
                    embedding: embeddingBytes
                );

                dbContext.Transactions.Add(newTransaction);
                newTransactions.Add(newTransaction);

                if (newTransactions.Count % 10 == 0)
                {
                    await OnAccountUpdateProgress(
                        $"Processing {newTransactions.Count}/{transactionsToAdd.Count} " +
                        $"({Math.Round((double)newTransactions.Count / transactionsToAdd.Count * 100, 0)}%)");
                }
            }
        }

        await dbContext.SaveChangesAsync();

        await TryAutomaticallyTagging(newTransactions, dbContext);

        AppCache.LastTransactionUpdate = DateTime.UtcNow;
    }

    private async Task TryAutomaticallyTagging(List<Transaction> transactions, AppDbContext dbContext)
    {
        if (transactions.Count == 0)
        {
            return;
        }

        await OnAccountUpdateProgress($"Tagging 0/{transactions.Count}");

        var negativeEmbeddingsRaw = await dbContext.TagNegativeEmbeddings
            .ToListAsync();

        var transactionEmbeddingsRaw = await dbContext.TransactionTags
            .Include(tt => tt.Transaction)
            .Where(tt => tt.Transaction!.Embedding != null)
            .Select(tt => new
            {
                Embedding = tt.Transaction!.Embedding!,
                tt.TagId
            }).ToListAsync();

        List<(float[] embedding, int tagId)> negativeEmbeddings = [..
            negativeEmbeddingsRaw.Select(ne => (
                ByteFloatConversionHelper.ByteArrayToFloatArray(ne.Embedding)!,
                ne.TagId
            ))
        ];

        List<(float[] embedding, int tagId)> positiveEmbeddings = [..
            transactionEmbeddingsRaw.Select(pe => (
                ByteFloatConversionHelper.ByteArrayToFloatArray(pe.Embedding)!,
                pe.TagId
            ))
        ];

        int counter = 0;

        foreach (var transaction in transactions.Where(t => t.Embedding != null))
        {
            var bestTagIds = TagPredictionService.PredictTags(
                newEmbedding: ByteFloatConversionHelper.ByteArrayToFloatArray(transaction.Embedding!)!,
                positiveEmbeddings: positiveEmbeddings,
                negativeEmbeddings: negativeEmbeddings
            );

            foreach (var tagId in bestTagIds)
            {
                dbContext.TransactionTags.Add(new(
                    internalTransactionId: transaction.InternalTransactionId,
                    accountId: transaction.AccountId,
                    tagId: tagId,
                    taggedBy: TaggedBy.SYSTEM
                ));
            }

            counter++;
            await OnAccountUpdateProgress(
                $"Tagging {counter}/{transactions.Count} " +
                $"({Math.Round((double)counter / transactions.Count * 100, 0)}%)");
        }

        await OnAccountUpdateProgress($"Finalizing...");
        await dbContext.SaveChangesAsync();
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
            await OnTimeoutOccurred();
        }
        catch (OperationCanceledException)
        {
            // Operation was cancelled, we can ignore this
            return [];
        }

        return [];
    }

    protected virtual async Task OnAccountUpdateStarted(string accountBeingUpdated)
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            AccountUpdateStarted?.Invoke(this, new(accountBeingUpdated));
        });
    }

    protected virtual async Task OnAccountUpdateProgress(string progressInfo)
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            AccountUpdateProgress?.Invoke(this, new(progressInfo));
        });
    }

    protected virtual async Task OnTimeoutOccurred()
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            TimeoutOccurred?.Invoke(this, EventArgs.Empty);
        });
    }

    protected virtual async Task OnTransactionUpdateEnded()
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            TransactionUpdateEnded?.Invoke(this, EventArgs.Empty);
        });
    }

    public async Task<double> GetMonthsBalance(DateOnly monthYear)
    {
        var dbContext = _databaseService.CreateDbContext();
        return await GetMonthsBalance(dbContext, monthYear);
    }

    private static async Task<double> GetMonthsBalance(AppDbContext dbContext, DateOnly monthYear)
    {
        var balanceOrSpendings = await dbContext.Transactions
            .Where(t => t.ValueDate.Year == monthYear.Year && t.ValueDate.Month == monthYear.Month)
            .SumAsync(t => t.TransactionAmount);

        return balanceOrSpendings;
    }

    private static async Task<double> GetMonthsSpendings(AppDbContext dbContext, DateOnly monthYear)
    {
        var spendings = await dbContext.Transactions
            .Where(t => t.ValueDate.Year == monthYear.Year && t.ValueDate.Month == monthYear.Month)
            .Where(t => t.TransactionAmount < 0)
            .SumAsync(t => t.TransactionAmount);

        return spendings;
    }

    public async Task<(List<(DateOnly date, double balance)> data, bool hasMore)> GetBalancesGroupedByMonth(int page, int pageSize)
    {
        var dbContext = _databaseService.CreateDbContext();

        var balances = await dbContext.Transactions
            .Select(t => new { t.ValueDate, t.TransactionAmount })
            .GroupBy(t => new { t.ValueDate.Year, t.ValueDate.Month })
            .OrderByDescending(g => g.Key.Year)
            .ThenByDescending(g => g.Key.Month)
            .Select(g => new
            {
                Date = new DateOnly(g.Key.Year, g.Key.Month, 1),
                Balance = g.Sum(t => t.TransactionAmount)
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize + 1)
            .ToListAsync();

        bool hasMore = balances.Count > pageSize;

        return ([.. balances.Select(x => (new DateOnly(x.Date.Year, x.Date.Month, 1), x.Balance))], hasMore);
    }

    public async Task<(List<(DateOnly date, List<Transaction> transactions)> data, bool hasMore)>
        GetTransactionsGroupedMyMonth(int page, int pageSize, DateTime? dateBegin, DateTime? dateEnd, int? tagId, Guid? accountId)
    {
        var dbContext = _databaseService.CreateDbContext();

        var transactions = dbContext.Transactions.AsQueryable();

        if (dateBegin.HasValue)
        {
            transactions = transactions.Where(t => t.ValueDate >= dateBegin.Value);
        }

        if (dateEnd.HasValue)
        {
            transactions = transactions.Where(t => t.ValueDate <= dateEnd.Value);
        }

        if (tagId.HasValue)
        {
            if (tagId.Value == -1)
            {
                // Special case: transactions without any tags
                transactions = transactions.Where(t => !t.TransactionTags.Any());
            }
            else
            {
                transactions = transactions.Where(t => t.TransactionTags.Any(tt => tt.TagId == tagId.Value));
            }
        }

        if (accountId.HasValue)
        {
            transactions = transactions
                .Where(t => t.AccountId == accountId.Value);
        }

        transactions = transactions
            .OrderByDescending(t => t.ValueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize + 1);

        bool hasMore = await transactions.CountAsync() > pageSize;

        var transactionGroups = await transactions
            .GroupBy(t => new { t.ValueDate.Year, t.ValueDate.Month })
            .OrderByDescending(g => g.Key.Year)
            .ThenByDescending(g => g.Key.Month)
            .Select(g => new
            {
                Date = new DateOnly(g.Key.Year, g.Key.Month, 1),
                Items = g.OrderByDescending(t => t.ValueDate).ToList()
            })
            .ToListAsync();

        return ([.. transactionGroups.Select(x => (new DateOnly(x.Date.Year, x.Date.Month, 1), x.Items))], hasMore);
    }

    public async Task<List<(Tag tag, double balance)>> GetTagBalances(DateOnly monthYear)
    {
        var dbContext = _databaseService.CreateDbContext();
        return await GetTagBalances(dbContext, monthYear);
    }

    private static async Task<List<(Tag tag, double balance)>> GetTagBalances(AppDbContext dbContext, DateOnly monthYear)
    {
        var tagBalances = await dbContext.TransactionTags
            .Include(tt => tt.Tag)
            .Include(tt => tt.Transaction)
            .Where(tt => tt.Transaction!.ValueDate.Year == monthYear.Year && tt.Transaction!.ValueDate.Month == monthYear.Month)
            .GroupBy(tt => tt.Tag)
            .Select(g => new
            {
                Tag = g.Key,
                Balance = g.Sum(tt => tt.Transaction!.TransactionAmount)
            })
            .OrderBy(x => x.Balance)
            .ThenBy(x => x.Tag!.Name)
            .ToListAsync();

        return [.. tagBalances.Select(tb => (tb.Tag!, tb.Balance))];
    }

    public async Task<double> GetUntaggedBalance(DateOnly monthYear)
    {
        var dbContext = _databaseService.CreateDbContext();
        return await GetUntaggedBalance(dbContext, monthYear);
    }

    private static async Task<double> GetUntaggedBalance(AppDbContext dbContext, DateOnly monthYear)
    {
        var untaggedBalance = await dbContext.Transactions
            .Include(t => t.TransactionTags)
            .Where(t => t.ValueDate.Year == monthYear.Year && t.ValueDate.Month == monthYear.Month)
            .Where(t => !t.TransactionTags.Any())
            .SumAsync(t => t.TransactionAmount);

        return untaggedBalance;
    }
}
