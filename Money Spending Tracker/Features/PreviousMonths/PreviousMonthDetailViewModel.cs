using CommunityToolkit.Mvvm.ComponentModel;
using Money_Spending_Tracker.Features.MonthSummary;
using Money_Spending_Tracker.Features.TransactionData;

namespace Money_Spending_Tracker.Features.PreviousMonths;

internal partial class PreviousMonthDetailViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private double _balance;

    [ObservableProperty]
    private List<TagBalanceModel>? _tagBalances;

    [ObservableProperty]
    private DateOnly? _monthYear;

    private readonly ITransactionDataService _transactionDataService;

    public PreviousMonthDetailViewModel(ITransactionDataService transactionDataService)
    {
        _transactionDataService = transactionDataService;
    }

    public async Task StartAsync(DateOnly monthYear)
    {
        MonthYear = monthYear;

        Title = monthYear.ToString("MMMM yyyy", System.Globalization.CultureInfo.CurrentCulture);
        Balance = await _transactionDataService.GetMonthsBalance(monthYear);

        var tagBalances = await _transactionDataService.GetTagBalances(monthYear);

        TagBalances = [.. tagBalances.Select(tb =>
            new TagBalanceModel(
                tagId: tb.tag.Id,
                tagName: tb.tag.Name,
                balance: tb.balance
            )
        )];
    }
}
