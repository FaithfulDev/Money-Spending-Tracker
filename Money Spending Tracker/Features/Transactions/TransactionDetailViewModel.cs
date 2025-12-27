using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Money_Spending_Tracker.Data;
using Money_Spending_Tracker.Features.Database;

namespace Money_Spending_Tracker.Features.Transactions;

public partial class TransactionDetailViewModel : ObservableObject
{
    [ObservableProperty]
    private Transaction? _transaction;

    private readonly DatabaseService _databaseService;
    private readonly IPopupService _popupService;

    private Guid? _internalTransactionId;
    private Guid? _accountId;

    private bool _isInitialized = false;

    public TransactionDetailViewModel(DatabaseService databaseService, IPopupService popupService)
    {
        _databaseService = databaseService;
        _popupService = popupService;
    }

    public async Task StartAsync(Guid transactionId, Guid accountId)
    {
        if (_isInitialized)
        {
            return;
        }

        _internalTransactionId = transactionId;
        _accountId = accountId;

        await FetchTransactionAsync();

        _isInitialized = true;
    }

    [RelayCommand]
    private async Task EditTags()
    {
        var shellParameters = new Dictionary<string, object>
        {
            [TransactionTagsViewModel.QueryParamInternalTransactionId] = _internalTransactionId!,
            [TransactionTagsViewModel.QueryParamAccountId] = _accountId!,
        };

        var result = await _popupService.ShowPopupAsync<TransactionTagsPopup>(
                shell: Shell.Current,
                options: null,
                shellParameters: shellParameters
            );

        if (result.WasDismissedByTappingOutsideOfPopup)
        {
            return;
        }

        await FetchTransactionAsync();
    }

    private async Task FetchTransactionAsync()
    {
        var dbContext = _databaseService.CreateDbContext();

        Transaction = await dbContext.Transactions
            .Include(t => t.TransactionTags).ThenInclude(tt => tt.Tag)
            .SingleOrDefaultAsync(t => t.InternalTransactionId == _internalTransactionId && t.AccountId == _accountId);
    }
}
