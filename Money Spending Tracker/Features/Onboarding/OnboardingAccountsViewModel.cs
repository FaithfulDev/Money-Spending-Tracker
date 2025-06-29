using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Money_Spending_Tracker.Features.Api;
using Money_Spending_Tracker.Features.Database;
using Money_Spending_Tracker.Features.Home;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Money_Spending_Tracker.Features.Onboarding;

internal partial class OnboardingAccountsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isWorking = false;

    private readonly Client _apiClient = new(new());

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmSelectionCommand))]
    private ObservableCollection<AccountSelectionModel> _accounts = [];

    private readonly DatabaseService _databaseService;
    private string? _institutionId;
    private string? _institutionName;
    private string? _institutionBic;
    private byte[]? _institutionLogo;

    public OnboardingAccountsViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task Start(string referenceId)
    {
        IsWorking = true;

        var requisition = await _apiClient.Requisition_by_idAsync(Guid.Parse(referenceId));
        var institution = await _apiClient.Retrieve_institutionAsync(requisition.Institution_id);

        _institutionId = institution.Id;
        _institutionName = institution.Name;
        _institutionBic = institution.Bic;

        if (!string.IsNullOrEmpty(institution.Logo))
        {
            // Get the logo from the logo url and store it as a byte array
            var httpClient = new HttpClient();

            var logoResponse = await httpClient.GetAsync(institution.Logo);
            if (logoResponse.IsSuccessStatusCode)
            {
                _institutionLogo = await logoResponse.Content.ReadAsByteArrayAsync();
            }
        }

        List<Account> accountDetailsList = [];

        // Query account details for each account
        foreach (var account in requisition.Accounts)
        {
            var accountDetails = await _apiClient.Retrieve_account_metadataAsync(account.ToString());
            if (accountDetails != null)
            {
                accountDetailsList.Add(accountDetails);
            }
        }

        // Show the list of accounts to the user
        Accounts = new ObservableCollection<AccountSelectionModel>(
            accountDetailsList.Select(a => new AccountSelectionModel(
                a.Name, a.Iban, a.Id.ToString()!)
            )
        );

        foreach (var account in Accounts)
        {
            account.PropertyChanged += AccountItemChanged;
        }

        IsWorking = false;
    }

    [RelayCommand(CanExecute = nameof(IsConfirmSelectionExecutable))]
    private async Task ConfirmSelection()
    {
        IsWorking = true;

        var dbContext = _databaseService.CreateDbContext();

        foreach (var account in Accounts.Where(a => a.IsSelected))
        {
            dbContext.Accounts.Add(
                new Data.Account(
                    accountId: Guid.Parse(account.AccountId),
                    accountName: account.AccountName,
                    accountIban: account.AccountIban,
                    institutionId: _institutionId!,
                    institutionName: _institutionName!,
                    institutionLogo: _institutionLogo,
                    institutionBic: _institutionBic!
                )
            );
        }

        await dbContext.SaveChangesAsync();

        IsWorking = false;

        await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
    }

    private void AccountItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AccountSelectionModel.IsSelected))
        {
            ConfirmSelectionCommand.NotifyCanExecuteChanged();
        }
    }

    private bool IsConfirmSelectionExecutable()
    {
        return Accounts.Any(a => a.IsSelected);
    }
}
