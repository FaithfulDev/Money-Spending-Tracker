using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Money_Spending_Tracker.Features.Accounts;

internal partial class AccountModel : ObservableObject
{
    public string AccountName { get; }
    public string AccountIban { get; }
    public Guid AccountId { get; }
    public int ExpiresInApproximatelyDays { get; set; } = 0;
    public bool IsExpired => ExpiresInApproximatelyDays <= 0;

    private readonly AccountsViewModel _accountsViewModel;

    public AccountModel(string accountName, string accountIban, Guid accountId, AccountsViewModel accountsViewModel,
        int expiresInApproximatelyDays)
    {
        AccountName = accountName;
        AccountIban = accountIban;
        AccountId = accountId;
        ExpiresInApproximatelyDays = expiresInApproximatelyDays;

        _accountsViewModel = accountsViewModel;
    }

    [RelayCommand]
    private async Task Delete()
    {
        await _accountsViewModel.DeleteAccountCommand.ExecuteAsync(this);
    }

    [RelayCommand]
    private async Task ReAuthenticate()
    {
        await _accountsViewModel.ReAuthenticateAccountCommand.ExecuteAsync(this);
    }
}
