using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Money_Spending_Tracker.Features.Api;
using Money_Spending_Tracker.Features.ChromeTabs;
using Money_Spending_Tracker.Features.Database;
using Money_Spending_Tracker.Features.Onboarding;
using Money_Spending_Tracker.Features.Storage;
using Money_Spending_Tracker.Features.TransactionData;
using System.Collections.ObjectModel;

namespace Money_Spending_Tracker.Features.Accounts;

internal partial class AccountsViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<AccountModel> _accounts = [];

    [ObservableProperty]
    private bool _isWorking = false;

    [ObservableProperty]
    private bool _didTimeoutOccur = false;

    [ObservableProperty]
    private string? _accountBeingUpdated = null;

    private readonly DatabaseService _databaseService;
    private readonly ITransactionDataService _transactionDataService;
    private readonly ICustomTabService _customTabService;

    public AccountsViewModel(DatabaseService databaseService, ITransactionDataService transactionDataService,
        ICustomTabService customTabService)
    {
        _databaseService = databaseService;
        _customTabService = customTabService;
        _transactionDataService = transactionDataService;

        _transactionDataService.AccountUpdateStarted += TransactionDataService_AccountUpdateStarted;
        _transactionDataService.TimeoutOccurred += TransactionDataService_TimeoutOccurred;
    }

    private void TransactionDataService_TimeoutOccurred(object? sender, EventArgs e)
    {
        DidTimeoutOccur = true;
    }

    private void TransactionDataService_AccountUpdateStarted(object sender, ITransactionDataService.AccountUpdateStartedEventArgs e)
    {
        AccountBeingUpdated = e.AccountBeingUpdated;
    }

    public async Task StartAsync(bool accountsAdded)
    {
        IsWorking = true;

        var linkStatus = await _transactionDataService.CheckAccountLinkStatus();

        var dbContext = _databaseService.CreateDbContext();
        var thisViewModel = this;

        var accountEntities = await dbContext.Accounts.ToListAsync();
        Accounts = [];

        foreach (var accountEntity in accountEntities)
        {
            Accounts.Add(new AccountModel(
                accountEntity.AccountName,
                accountEntity.AccountIban,
                accountEntity.AccountId,
                thisViewModel,
                linkStatus.First(ls => ls.accountId == accountEntity.AccountId).expiresInDays,
                accountEntity.InstitutionLogo
            ));
        }

        // This indicates that new accounts were added and we came back to this page
        if (accountsAdded)
        {
            await _transactionDataService.UpdateTransactionsAndCacheAsync();

            // Navigate to clear the navigation stack.
            await Shell.Current.GoToAsync($"//{nameof(AccountsPage)}");
        }

        IsWorking = false;
    }

    [RelayCommand]
    private async Task AddAccount()
    {
        AppCache.OnboardingFinalPage = $"{nameof(AccountsPage)}";
        AppCache.OnboardingPath = $"//{nameof(AccountsPage)}";

        await Shell.Current.GoToAsync($"{nameof(OnboardingAuthenticationPage)}");
    }

    [RelayCommand]
    private async Task DeleteAccount(AccountModel accountModel)
    {
        if (!await Shell.Current.DisplayAlert(
            "Delete Account",
            $"Are you sure you want to delete the account '{accountModel.AccountName}' ({accountModel.AccountIban})?",
            "Yes", "No"))
        {
            return;
        }

        var dbContext = _databaseService.CreateDbContext();
        var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountModel.AccountId);

        if (account == null)
        {
            return;
        }

        await dbContext.Transactions.Where(t => t.AccountId == account.AccountId)
            .ExecuteDeleteAsync();

        dbContext.Accounts.Remove(account);
        await dbContext.SaveChangesAsync();

        Accounts.Remove(accountModel);

        // Recalculate balance and remaining budget after deletion
        await _transactionDataService.UpdateCacheAsync();
    }

    [RelayCommand]
    private async Task ReAuthenticateAccount(AccountModel accountModel)
    {
        if (!await Shell.Current.DisplayAlert(
            "Re-Authenticate Account",
            $"Do you want to try to re-authenticate the account '{accountModel.AccountName}' ({accountModel.AccountIban})?",
            "Yes", "No"))
        {
            return;
        }

        IsWorking = true;

        var dbContext = _databaseService.CreateDbContext();
        var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountModel.AccountId);

        if (account == null)
        {
            return;
        }

        var apiClient = new Client(new());

        var agreement = await apiClient.Create_EUAAsync(new()
        {
            Institution_id = account.InstitutionId,
            Access_scope = ["details", "transactions"],
        });

        var requisition = await apiClient.Create_requisitionAsync(new()
        {
            Institution_id = account.InstitutionId,
            Redirect = new Uri("de.faithfuldevapps.moneyspendingtracker://authredirect"),
            Agreement = agreement.Id,
        });

        AppCache.ReAuthenticationInProgress = true;

        var authUrl = requisition.Link;
        _customTabService.OpenUrl(authUrl);

        IsWorking = false;
    }
}
