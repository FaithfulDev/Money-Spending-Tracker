using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Money_Spending_Tracker.Data;
using Money_Spending_Tracker.Features.Database;
using System.Collections.ObjectModel;

namespace Money_Spending_Tracker.Features.Transactions;

public partial class TransactionTagsViewModel : ObservableObject, IQueryAttributable
{
    // Query parameter keys
    public const string QueryParamAccountId = nameof(_accountId);
    public const string QueryParamInternalTransactionId = nameof(_internalTransactionId);

    private readonly DatabaseService _databaseService;
    private readonly IPopupService _popupService;
    private Guid? _accountId;
    private Guid? _internalTransactionId;

    [ObservableProperty]
    private ObservableCollection<TransactionTagsModel> _tagModels = [];

    public TransactionTagsViewModel(DatabaseService databaseService, IPopupService popupService)
    {
        _databaseService = databaseService;
        _popupService = popupService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(QueryParamInternalTransactionId, out object? value) && value is Guid internalTransactionId)
        {
            _internalTransactionId = internalTransactionId;
        }

        if (query.TryGetValue(QueryParamAccountId, out object? value3) && value3 is Guid accountId)
        {
            _accountId = accountId;
        }
    }

    public async Task StartAsync()
    {
        var dbContext = _databaseService.CreateDbContext();

        var transactionTags = await dbContext.TransactionTags
            .Where(tt => tt.InternalTransactionId == _internalTransactionId && tt.AccountId == _accountId)
            .ToListAsync();

        var tags = await dbContext.Tags
            .ToListAsync();

        var tagModels = tags
            .Select(
                t => new TransactionTagsModel(
                    id: t.Id,
                    name: t.Name,
                    isSelected: transactionTags.Any(tt => tt.TagId == t.Id),
                    isTaggedBySystem: transactionTags.Any(tt => tt.TagId == t.Id && tt.TaggedBy == TaggedBy.SYSTEM)
                )
            );

        TagModels = tagModels.ToObservableCollection();
        OrderTagModels();
    }

    private void OrderTagModels()
    {
        TagModels = TagModels
            .OrderByDescending(tm => tm.IsSelected)
            .ThenBy(tm => tm.Name)
            .ToObservableCollection();
    }

    [RelayCommand]
    private async Task SaveTags()
    {
        var dbContext = _databaseService.CreateDbContext();

        var transactionTags = await dbContext.TransactionTags
            .Where(tt => tt.InternalTransactionId == _internalTransactionId && tt.AccountId == _accountId)
            .Include(tt => tt.Transaction)
            .ToListAsync();

        //Check if system tags where removed and if so, handle case.
        var removedSystemTag = transactionTags
            .Where(tt => tt.TaggedBy == TaggedBy.SYSTEM)
            .FirstOrDefault(
                tt => TagModels.FirstOrDefault(tm => tm.Id == tt.TagId && !tm.IsSelected) != null &&
                      tt.Transaction?.Embedding != null
            );

        bool doRemeberChoice = false;

        if (removedSystemTag != null)
        {
            doRemeberChoice = await Shell.Current.DisplayAlertAsync(
                "System Tag Removal",
                "You removed a tag that was added automatically. Do you want to remeber this choice for future similiar transaction?",
                "Yes", "No");
        }

        //Find all tags that were removed
        var removedTransactionTags = transactionTags
            .Where(tt => TagModels.Any(tm => tm.Id == tt.TagId && !tm.IsSelected))
            .ToList();

        foreach (var transactionTag in removedTransactionTags)
        {
            dbContext.TransactionTags.Remove(transactionTag);

            // If user chose to remember the choice, save negative embedding.
            if (doRemeberChoice && transactionTag.Transaction?.Embedding != null)
            {
                TagNegativeEmbedding tagNegativeEmbedding = new(
                    tagId: transactionTag.TagId,
                    embedding: transactionTag.Transaction.Embedding);

                // Before saving, check for duplicates
                bool alreadyExists = await dbContext.TagNegativeEmbeddings
                    .AnyAsync(ne => ne.TagId == transactionTag.TagId && ne.Hash == tagNegativeEmbedding.Hash);

                if (!alreadyExists)
                {
                    dbContext.TagNegativeEmbeddings.Add(tagNegativeEmbedding);
                }
            }
        }

        //Loop through ui taglist and add newlay selected ones to db
        foreach (var tagModel in TagModels.Where(tm => tm.IsSelected && !transactionTags.Any(tt => tt.TagId == tm.Id)))
        {
            dbContext.TransactionTags.Add(new TransactionTag(
                 internalTransactionId: (Guid)_internalTransactionId!,
                 accountId: (Guid)_accountId!,
                 tagId: tagModel.Id,
                 taggedBy: TaggedBy.USER
             ));
        }

        await dbContext.SaveChangesAsync();

        await _popupService.ClosePopupAsync(Shell.Current);
    }
}
