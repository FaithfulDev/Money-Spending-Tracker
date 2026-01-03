using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Money_Spending_Tracker.Features.Database;
using Money_Spending_Tracker.Features.Storage;
using System.Collections.ObjectModel;

namespace Money_Spending_Tracker.Features.Tags;

public partial class TagListViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<TagListModel> items = [];

    [ObservableProperty]
    private TagListModel? _selectedItem;

    private readonly DatabaseService _databaseService;

    public TagListViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public void Start()
    {
        Items.Clear();

        var dbContext = _databaseService.CreateDbContext();

        var thisViewModel = this;

        Items = dbContext.Tags
            .AsNoTracking()
            .Select(tag => new TagListModel(tag.Id, tag.Name, thisViewModel))
            .ToObservableCollection();
    }

    [RelayCommand]
    private async Task SelectionChanged()
    {
        if (SelectedItem == null)
        {
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(EditTagNameModal)}?{nameof(EditTagNameModal.TagId)}={SelectedItem.Id}");

        SelectedItem = null;
    }

    [RelayCommand]
    private async Task AddNewTag()
    {
        await Shell.Current.GoToAsync(nameof(EditTagNameModal));
    }

    [RelayCommand]
    private async Task DeleteTag(TagListModel tagModel)
    {
        if (!await Shell.Current.DisplayAlertAsync(
            "Delete Tag",
            $"Are you sure you want to delete the tag '{tagModel.Name}'?",
            "Yes", "No"))
        {
            return;
        }

        var dbContext = _databaseService.CreateDbContext();
        var tag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == tagModel.Id);

        if (tag == null)
        {
            return;
        }

        await dbContext.TransactionTags.Where(t => t.TagId == tag.Id)
            .ExecuteDeleteAsync();

        dbContext.Tags.Remove(tag);
        await dbContext.SaveChangesAsync();

        AppCache.ClearTagValue(tag.Id);

#if ANDROID
        MainApplication.TriggerWidgetUpdate();
#endif

        Items.Remove(tagModel);
    }
}
