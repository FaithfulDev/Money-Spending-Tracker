namespace Money_Spending_Tracker.Features.TransactionData;

public interface ITransactionDataService
{
    delegate void AccountUpdateStartedHandler(object sender, AccountUpdateStartedEventArgs e);
    event AccountUpdateStartedHandler AccountUpdateStarted;

    event EventHandler TimeoutOccurred;
    event EventHandler TransactionUpdateEnded;

    /// <summary>
    /// Pulls transaction data from the API and updates the database. 
    /// Updates the cache with the new values.
    /// </summary>
    public Task UpdateTransactionsAndCacheAsync();

    /// <summary>
    /// Recalculates the current balance and remaining budget based on the transactions for the current month.
    /// New values are stored in the AppCache. Also updates the widget with the new values.
    /// </summary>
    public Task UpdateCacheAsync();

    public Task<List<(Guid accountId, bool isLinked, int expiresInDays)>> CheckAccountLinkStatus();

    public Task<(List<(DateOnly date, double balance)> data, bool hasMore)> GetBalancesGroupedByMonth(int page, int pageSize);

    public Task<double> GetMonthsBalance(DateOnly monthYear);

    public class AccountUpdateStartedEventArgs : EventArgs
    {
        /// <summary>
        /// IBAN of the account being updated.
        /// </summary>
        public string AccountBeingUpdated { get; set; }

        public AccountUpdateStartedEventArgs(string accountBeingUpdated)
        {
            AccountBeingUpdated = accountBeingUpdated;
        }
    }
}
