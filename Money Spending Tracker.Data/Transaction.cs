namespace Money_Spending_Tracker.Data;

public class Transaction
{
    public string TransactionId { get; set; }
    public string EntryReference { get; set; }
    public string EndToEndId { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime ValueDate { get; set; }
    public double TransactionAmount { get; set; }
    public string CreditorName { get; set; }
    public string UltimateCreditor { get; set; }
    public string RemittanceInformationStructured { get; set; }
    public string AdditionalInformation { get; set; }
    public string PurposeCode { get; set; }
    public string ProprietaryBankTransactionCode { get; set; }
    public string CreditorAgent { get; set; }
    public string DebtorAgent { get; set; }
    public string InternalTransactionId { get; set; }

    public Transaction(string transactionId, string entryReference, string endToEndId, DateTime bookingDate, DateTime valueDate,
        double transactionAmount, string creditorName, string ultimateCreditor, string remittanceInformationStructured,
        string additionalInformation, string purposeCode, string proprietaryBankTransactionCode, string creditorAgent,
        string debtorAgent, string internalTransactionId)
    {
        TransactionId = transactionId;
        EntryReference = entryReference;
        EndToEndId = endToEndId;
        BookingDate = bookingDate;
        ValueDate = valueDate;
        TransactionAmount = transactionAmount;
        CreditorName = creditorName;
        UltimateCreditor = ultimateCreditor;
        RemittanceInformationStructured = remittanceInformationStructured;
        AdditionalInformation = additionalInformation;
        PurposeCode = purposeCode;
        ProprietaryBankTransactionCode = proprietaryBankTransactionCode;
        CreditorAgent = creditorAgent;
        DebtorAgent = debtorAgent;
        InternalTransactionId = internalTransactionId;
    }
}
