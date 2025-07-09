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
    public ImageSource InstitutionLogo { get; set; }

    private readonly AccountsViewModel _accountsViewModel;

    public AccountModel(string accountName, string accountIban, Guid accountId, AccountsViewModel accountsViewModel,
        int expiresInApproximatelyDays, byte[]? institutionLogo)
    {
        AccountName = accountName;
        AccountIban = accountIban;
        AccountId = accountId;
        ExpiresInApproximatelyDays = expiresInApproximatelyDays;

        if (institutionLogo != null && institutionLogo.Length > 0)
        {
            InstitutionLogo = ImageSource.FromStream(() => new MemoryStream(institutionLogo));
        }
        else
        {
            // Use the app icon as fallback
            InstitutionLogo = ImageSource.FromFile("appiconpng.png");
        }

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
