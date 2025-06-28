using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Money_Spending_Tracker.Features.Onboarding;

public partial class AccountSelectionModel : ObservableObject
{
    public string AccountName { get; }
    public string AccountIban { get; }
    public string AccountId { get; }

    [ObservableProperty]
    private bool _isSelected;

    public AccountSelectionModel(string accountName, string accountIban, string accountId)
    {
        AccountName = accountName;
        AccountIban = accountIban;
        AccountId = accountId;
    }

    [RelayCommand]
    private void ToggleSelection()
    {
        IsSelected = !IsSelected;
    }
}
