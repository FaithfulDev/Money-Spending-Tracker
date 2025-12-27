namespace Money_Spending_Tracker.Data;

public class Transaction
{
    public Guid InternalTransactionId { get; set; }
    public string TransactionId { get; set; }
    public Guid AccountId { get; set; }
    public string? EntryReference { get; set; }
    public string? EndToEndId { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime ValueDate { get; set; }
    public double TransactionAmount { get; set; }
    public string? CreditorName { get; set; }
    public string? UltimateCreditor { get; set; }
    public string? DebtorName { get; set; }
    public string? RemittanceInformationStructured { get; set; }
    public string? RemittanceInformationUnstructured { get; set; }
    public string? AdditionalInformation { get; set; }
    public string? PurposeCode { get; set; }
    public string? ProprietaryBankTransactionCode { get; set; }

    public List<TransactionTag> TransactionTags { get; set; } = [];

    /// <summary>
    /// Used to store the embedding vector for the transaction description. Helps to identify similar transactions.
    /// </summary>
    public byte[]? Embedding { get; set; }

    public Transaction(string transactionId, Guid accountId, string? entryReference, string? endToEndId, DateTime bookingDate,
        DateTime valueDate, double transactionAmount, string? creditorName, string? ultimateCreditor, string? debtorName,
        string? remittanceInformationStructured, string? remittanceInformationUnstructured, string? additionalInformation,
        string? purposeCode, string? proprietaryBankTransactionCode, Guid internalTransactionId, byte[]? embedding = null)
    {
        TransactionId = transactionId;
        AccountId = accountId;
        EntryReference = entryReference;
        EndToEndId = endToEndId;
        BookingDate = bookingDate;
        ValueDate = valueDate;
        TransactionAmount = transactionAmount;
        CreditorName = creditorName;
        UltimateCreditor = ultimateCreditor;
        DebtorName = debtorName;
        RemittanceInformationStructured = remittanceInformationStructured;
        RemittanceInformationUnstructured = remittanceInformationUnstructured;
        AdditionalInformation = additionalInformation;
        PurposeCode = purposeCode;
        ProprietaryBankTransactionCode = proprietaryBankTransactionCode;
        InternalTransactionId = internalTransactionId;
        Embedding = embedding;
    }
}
