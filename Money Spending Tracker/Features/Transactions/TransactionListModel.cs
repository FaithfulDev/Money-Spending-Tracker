using System.Collections.ObjectModel;

namespace Money_Spending_Tracker.Features.Transactions;

public class TransactionListModel
{
    public Guid InternalTransactionId { get; set; }

    public Guid AccountId { get; set; }

    public DateTime ValueDate { get; set; }

    public string? CreditorOrDebtorName { get; set; }

    public string? RemittanceInformation { get; set; }

    public double TransactionAmount { get; set; }

    public TransactionListModel(Guid internalTransactionId, Guid accountId, DateTime valueDate, string? creditorOrDebtorName,
        string? remittanceInformation, double transactionAmount)
    {
        InternalTransactionId = internalTransactionId;
        AccountId = accountId;
        ValueDate = valueDate;
        CreditorOrDebtorName = creditorOrDebtorName;
        RemittanceInformation = remittanceInformation;
        TransactionAmount = transactionAmount;
    }
}

public class TransactionGroup : ObservableCollection<TransactionListModel>
{
    public DateOnly MonthAndYear { get; set; }

    public TransactionGroup(DateOnly monthAndyear, IEnumerable<TransactionListModel> items) : base(items)
    {
        MonthAndYear = monthAndyear;
    }
}
