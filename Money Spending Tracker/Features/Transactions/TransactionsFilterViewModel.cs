using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Money_Spending_Tracker.Data;
using Money_Spending_Tracker.Features.Database;
using System.Collections.ObjectModel;

namespace Money_Spending_Tracker.Features.Transactions;

public partial class TransactionsFilterViewModel : ObservableObject, IQueryAttributable
{
    // Query parameter keys
    public const string QueryParamDateBegin = nameof(DateBegin);
    public const string QueryParamDateEnd = nameof(DateEnd);
    public const string QueryParamTagId = nameof(_tagId);
    public const string QueryParamAccountId = nameof(_accountId);

    [ObservableProperty]
    private DateTime _dateBegin;

    [ObservableProperty]
    private DateTime _dateEnd;

    [ObservableProperty]
    private ObservableCollection<Tag> _tags = [];

    [ObservableProperty]
    private Tag? _selectedTag;

    [ObservableProperty]
    private ObservableCollection<Account> _accounts = [];

    [ObservableProperty]
    private Account? _selectedAccount;

    private DatabaseService _databaseService;
    private IPopupService _popupService;
    private int? _tagId;
    private Guid? _accountId;

    public TransactionsFilterViewModel(DatabaseService databaseService, IPopupService popupService)
    {
        _databaseService = databaseService;
        _popupService = popupService;

        DateBegin = new(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0, DateTimeKind.Local);
        DateEnd = DateTime.Now;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(QueryParamDateBegin, out object? value) && value is DateTime dateBegin)
        {
            DateBegin = dateBegin;
        }

        if (query.TryGetValue(QueryParamDateEnd, out object? value1) && value1 is DateTime dateEnd)
        {
            DateEnd = dateEnd;
        }

        if (query.TryGetValue(QueryParamTagId, out object? value2) && value2 is int tagId)
        {
            _tagId = tagId;
        }

        if (query.TryGetValue(QueryParamAccountId, out object? value3) && value3 is Guid accountId)
        {
            _accountId = accountId;
        }
    }

    public async Task StartAsync()
    {
        Tags.Add(new Tag("All Tags") { Id = -2 });
        Tags.Add(new Tag("No Tag") { Id = -1 });

        Accounts.Add(new Account(Guid.Empty, "All Accounts", string.Empty, string.Empty, string.Empty, null, string.Empty));

        var dbContext = _databaseService.CreateDbContext();

        var tags = await dbContext.Tags.OrderBy(t => t.Name).ToListAsync();
        var accounts = await dbContext.Accounts.OrderBy(a => a.AccountName).ToListAsync();

        Tags = [.. Tags, .. tags];
        Accounts = [.. Accounts, .. accounts];

        if (_tagId.HasValue)
        {
            SelectedTag = Tags.FirstOrDefault(t => t.Id == _tagId.Value);
        }
        else
        {
            SelectedTag = Tags[0];
        }

        if (_accountId.HasValue)
        {
            SelectedAccount = Accounts.FirstOrDefault(a => a.AccountId == _accountId.Value);
        }
        else
        {
            SelectedAccount = Accounts[0];
        }
    }

    [RelayCommand]
    private async Task ApplyFilters()
    {
        int? tagId = SelectedTag?.Id;

        if (tagId.HasValue && tagId.Value == -2)
        {
            tagId = null;
        }

        Guid? accountId = SelectedAccount?.AccountId;

        if (accountId.HasValue && accountId.Value == Guid.Empty)
        {
            accountId = null;
        }

        var results = new TransactionFilterPopupResults(
            filterDateBegin: DateBegin,
            filterDateEnd: DateEnd,
            filterTagId: tagId,
            filterAccountId: accountId
        );

        await _popupService.ClosePopupAsync(Shell.Current, results);
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await _popupService.ClosePopupAsync(Shell.Current);
    }
}
