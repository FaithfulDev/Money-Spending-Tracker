using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Money_Spending_Tracker.Features.Accounts;
using Money_Spending_Tracker.Features.Settings;
using Money_Spending_Tracker.Features.Storage;
using Money_Spending_Tracker.Features.TransactionData;

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
    private Color _balanceColor = Colors.Black;

    [ObservableProperty]
    private bool _didTimeout = false;

    [ObservableProperty]
    private bool _showExpireWarning = false;

    [ObservableProperty]
    private bool _showExpireSoonWarning = false;

    [ObservableProperty]
    private bool _showAlreadyExpiredWarning = false;

    [ObservableProperty]
    private bool _showExpiresInDaysWarning = false;

    [ObservableProperty]
    private int _DaysUntilExpiry = 0;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private string? _accountBeingUpdated = null;

    private readonly ITransactionDataService _transactionDataService;

    public HomeViewModel(ITransactionDataService transactionDataService)
    {
        _transactionDataService = transactionDataService;

        _transactionDataService.AccountUpdateStarted += TransactionDataService_AccountUpdateStarted;
        _transactionDataService.TimeoutOccurred += TransactionDataService_TimeoutOccurred;
    }

    private void TransactionDataService_TimeoutOccurred(object? sender, EventArgs e)
    {
        DidTimeout = true;
    }

    private void TransactionDataService_AccountUpdateStarted(object sender, ITransactionDataService.AccountUpdateStartedEventArgs e)
    {
        AccountBeingUpdated = e.AccountBeingUpdated;
    }

    public async Task StartAsync(bool accountsAdded)
    {
        Title = DateTime.Now.ToString("MMMM yyyy", System.Globalization.CultureInfo.CurrentCulture);
        RemainingBudgetString = FormatCurrency(AppCache.RemainingMonthlyBudget);
        RemainingBudgetColor = AppCache.RemainingMonthlyBudget < 0 ? Colors.Red : Colors.Black;
        BalanceString = FormatCurrency(AppCache.CurrentBalance);
        BalanceColor = AppCache.CurrentBalance < 0 ? Colors.Red : Colors.Green;

        // If accounts have been added, we always update transactions and widget.
        // If no accounts have been added, we only update if the last transaction update was not today.
        if (!accountsAdded && AppCache.LastTransactionUpdate.UtcDateTime.Date == DateTime.UtcNow.Date)
        {
            return;
        }

        // If the last transaction update was last month, reset the remaining budget and balance.
        if (AppCache.LastTransactionUpdate.UtcDateTime.Date.Month != DateTime.UtcNow.Date.Month ||
           AppCache.LastTransactionUpdate.UtcDateTime.Date.Year != DateTime.UtcNow.Date.Year)
        {
            AppCache.RemainingMonthlyBudget = AppSettings.MonthlyBudget;
            AppCache.CurrentBalance = 0;

            RemainingBudgetString = FormatCurrency(AppCache.RemainingMonthlyBudget);
            RemainingBudgetColor = Colors.Black;
            BalanceString = FormatCurrency(0);
            BalanceColor = Colors.Black;
        }

        await Refresh();

        if (accountsAdded)
        {
            // Navigate to clear the stack.
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
        }
    }

    private async Task UpdateTransactionsAndWidgetAsync()
    {
        // Reset the timeout flag
        DidTimeout = false;

        await _transactionDataService.UpdateTransactionsAndCacheAsync();

        RemainingBudgetString = FormatCurrency(AppCache.RemainingMonthlyBudget);
        RemainingBudgetColor = AppCache.RemainingMonthlyBudget < 0 ? Colors.Red : Colors.Black;
        BalanceString = FormatCurrency(AppCache.CurrentBalance);
        BalanceColor = AppCache.CurrentBalance < 0 ? Colors.Red : Colors.Green;
    }

    private async Task CheckAccountLinks()
    {
        var linkStatus = await _transactionDataService.CheckAccountLinkStatus();
        if (linkStatus.Any(ls => !ls.isLinked || ls.expiresInDays <= 15))
        {
            ShowExpireWarning = true;
        }

        if (linkStatus.Any(ls => !ls.isLinked))
        {
            ShowAlreadyExpiredWarning = true;
            return;
        }

        if (linkStatus.Any(ls => ls.isLinked && ls.expiresInDays <= 0))
        {
            ShowExpireSoonWarning = true;
            return;
        }

        if (linkStatus.Any(ls => ls.isLinked && ls.expiresInDays <= 15))
        {
            var minExpiryItem = linkStatus
                .Where(ls => ls.isLinked && ls.expiresInDays <= 15)
                .OrderBy(ls => ls.expiresInDays)
                .First();

            ShowExpiresInDaysWarning = true;
            DaysUntilExpiry = minExpiryItem.expiresInDays;
        }
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

        await CheckAccountLinks();
        await UpdateTransactionsAndWidgetAsync();

        IsWorking = false;
    }

    [RelayCommand]
    private async Task CheckAccounts()
    {
        await Shell.Current.GoToAsync($"//{nameof(AccountsPage)}");
    }

    private static string FormatCurrency(double amount)
    {
        return amount.ToString("C2", System.Globalization.CultureInfo.CurrentCulture);
    }
}
