using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Money_Spending_Tracker.Features.Transactions;

public partial class TransactionTagsModel : ObservableObject
{
    public int Id { get; set; }

    public string Name { get; set; }

    [ObservableProperty]
    private bool _isSelected;

    public bool IsTaggedBySystem { get; set; }

    public TransactionTagsModel(int id, string name, bool isSelected, bool isTaggedBySystem)
    {
        Id = id;
        Name = name;
        IsSelected = isSelected;
        IsTaggedBySystem = isTaggedBySystem;
    }

    [RelayCommand]
    private void ToggleSelection()
    {
        IsSelected = !IsSelected;
    }
}
