using CommunityToolkit.Maui.Core;

namespace Money_Spending_Tracker.Features.Transactions;

public class TransactionFilterPopupResults : IPopupResult
{
    public DateTime FilterDateBegin { get; set; }
    public DateTime FilterDateEnd { get; set; }
    public int? FilterTagId { get; set; }
    public Guid? FilterAccountId { get; set; }
    public bool WasDismissedByTappingOutsideOfPopup => false;

    public TransactionFilterPopupResults(DateTime filterDateBegin, DateTime filterDateEnd, int? filterTagId, Guid? filterAccountId)
    {
        FilterDateBegin = filterDateBegin;
        FilterDateEnd = filterDateEnd;
        FilterTagId = filterTagId;
        FilterAccountId = filterAccountId;
    }
}
