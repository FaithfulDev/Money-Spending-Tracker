using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Money_Spending_Tracker.Data;
using Money_Spending_Tracker.Features.Database;

namespace Money_Spending_Tracker.Features.Tags;

public partial class EditTagNameViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string? _name;

    private readonly DatabaseService _databaseService;
    private int _tagId;

    public EditTagNameViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task StartAsync(int tagId)
    {
        _tagId = tagId;

        if (_tagId == 0)
        {
            Name = string.Empty;
            return;
        }

        using var dbContext = _databaseService.CreateDbContext();
        Tag? tag = await dbContext.Tags.FindAsync(_tagId);
        Name = tag?.Name;
    }

    [RelayCommand(CanExecute = nameof(IsSaveExecutable))]
    private async Task Save()
    {
        var dbContext = _databaseService.CreateDbContext();

        if (_tagId == 0)
        {
            AddNewTag(dbContext);
        }
        else
        {
            await UpdateTag(dbContext);
        }

        await dbContext.SaveChangesAsync();

        await Shell.Current.GoToAsync($"..");
    }

    private void AddNewTag(AppDbContext dbContext)
    {
        var tag = new Tag(Name!);
        dbContext.Tags.Add(tag);
    }

    private async Task UpdateTag(AppDbContext dbContext)
    {
        Tag? tag = await dbContext.Tags.FindAsync(_tagId);
        tag!.Name = Name!;
    }

    private bool IsSaveExecutable()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }

    [RelayCommand]
    private static async Task Cancel()
    {
        await Shell.Current.GoToAsync($"..");
    }
}
