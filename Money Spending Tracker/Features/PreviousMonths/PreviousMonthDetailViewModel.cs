using CommunityToolkit.Mvvm.ComponentModel;
using Money_Spending_Tracker.Features.TransactionData;

namespace Money_Spending_Tracker.Features.PreviousMonths;

internal partial class PreviousMonthDetailViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private double _balance;

    private readonly ITransactionDataService _transactionDataService;

    public PreviousMonthDetailViewModel(ITransactionDataService transactionDataService)
    {
        _transactionDataService = transactionDataService;
    }

    public async Task StartAsync(DateOnly monthYear)
    {
        Title = monthYear.ToString("MMMM yyyy", System.Globalization.CultureInfo.CurrentCulture);
        Balance = await _transactionDataService.GetMonthsBalance(monthYear);
    }
}
