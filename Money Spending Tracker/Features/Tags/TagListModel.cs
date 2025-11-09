using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Money_Spending_Tracker.Features.Tags;

public partial class TagListModel : ObservableObject
{
    public int Id { get; set; }
    public string Name { get; set; }

    private readonly TagListViewModel _tagListViewModel;

    public TagListModel(int id, string name, TagListViewModel tagListViewModel)
    {
        Id = id;
        Name = name;
        _tagListViewModel = tagListViewModel;
    }

    [RelayCommand]
    private async Task Delete()
    {
        await _tagListViewModel.DeleteTagCommand.ExecuteAsync(this);
    }
}
