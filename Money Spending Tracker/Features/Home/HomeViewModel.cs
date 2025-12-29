using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Money_Spending_Tracker.Features.Accounts;
using Money_Spending_Tracker.Features.MonthSummary;
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
    private double? _remainingBudget;

    [ObservableProperty]
    private double _balance;

    [ObservableProperty]
    private List<TagBalanceModel>? _tagBalances;

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

    [ObservableProperty]
    private string? _accountUpdateProgressInfo = null;

    [ObservableProperty]
    private DateTime? _lastTransactionUpdate = null;

    [ObservableProperty]
    private DateOnly _monthYear = new(DateTime.Now.Year, DateTime.Now.Month, 1);

    private readonly ITransactionDataService _transactionDataService;

    public HomeViewModel(ITransactionDataService transactionDataService)
    {
        _transactionDataService = transactionDataService;

        _transactionDataService.AccountUpdateStarted += TransactionDataService_AccountUpdateStarted;
        _transactionDataService.TimeoutOccurred += TransactionDataService_TimeoutOccurred;
        _transactionDataService.TransactionUpdateEnded += TransactionDataService_TransactionUpdateEnded;
        _transactionDataService.AccountUpdateProgress += TransactionDataService_AccountUpdateProgress;
    }

    private void TransactionDataService_AccountUpdateProgress(object sender, ITransactionDataService.AccountUpdateProgressEventArgs e)
    {
        AccountUpdateProgressInfo = e.ProgressInfo;
    }

    private async void TransactionDataService_TransactionUpdateEnded(object? sender, EventArgs e)
    {
        Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
        SetIsWorking(false);

        await UpdateUIValues();
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
        Title = MonthYear.ToString("MMMM yyyy", System.Globalization.CultureInfo.CurrentCulture);

        await UpdateUIValues();

        // If accounts have been added, we always update transactions and widget.
        // If no accounts have been added, we only update if the last transaction update was not today.
        if (!accountsAdded && AppCache.LastTransactionUpdate?.Date == DateTime.UtcNow.Date)
        {
            return;
        }

        // If the last transaction update was last month, reset the remaining budget and balance.
        if (AppCache.LastTransactionUpdate?.Date.Month != DateTime.UtcNow.Date.Month ||
           AppCache.LastTransactionUpdate?.Date.Year != DateTime.UtcNow.Date.Year)
        {
            AppCache.RemainingMonthlyBudget = AppSettings.MonthlyBudget;
            AppCache.CurrentBalance = 0;

            RemainingBudget = AppCache.RemainingMonthlyBudget;
            Balance = AppCache.CurrentBalance;
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

        SetIsWorking(true);

        Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;

        await CheckAccountLinks();
        await UpdateTransactionsAndWidgetAsync();

        // Ending logic happens in transactionDataService_TransactionUpdateEnded event handler.
    }

    [RelayCommand]
    private async Task CheckAccounts()
    {
        await Shell.Current.GoToAsync($"//{nameof(AccountsPage)}");
    }

    private void SetIsWorking(bool value)
    {
        IsWorking = value;
        Shell.Current.FlyoutBehavior = value ? FlyoutBehavior.Disabled : FlyoutBehavior.Flyout;
    }

    private async Task UpdateUIValues()
    {
        RemainingBudget = AppCache.RemainingMonthlyBudget;
        Balance = AppCache.CurrentBalance;
        LastTransactionUpdate = AppCache.LastTransactionUpdate;

        TagBalances?.Clear();

        var tagBalances = await _transactionDataService.GetTagBalances(MonthYear);

        TagBalances = [.. tagBalances.Select(tb =>
            new TagBalanceModel(
                tagId: tb.tag.Id,
                tagName: tb.tag.Name,
                balance: tb.balance
            )
        )];

        var untaggedBalance = await _transactionDataService.GetUntaggedBalance(MonthYear);

        TagBalances.Add(
            new TagBalanceModel(
                tagId: -1,
                tagName: "Untagged",
                balance: untaggedBalance
            )
        );

        TagBalances = [.. TagBalances.OrderBy(tb => tb.Balance)];
    }
}
