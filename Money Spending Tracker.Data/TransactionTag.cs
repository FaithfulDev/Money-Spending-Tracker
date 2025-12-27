namespace Money_Spending_Tracker.Data;

public class TransactionTag
{
    public Guid InternalTransactionId { get; set; }
    public Guid AccountId { get; set; }

    public Transaction? Transaction { get; set; }

    public int TagId { get; set; }
    public Tag? Tag { get; set; }

    public TaggedBy TaggedBy { get; set; }

    public TransactionTag(Guid internalTransactionId, Guid accountId, int tagId, TaggedBy taggedBy)
    {
        InternalTransactionId = internalTransactionId;
        AccountId = accountId;
        TagId = tagId;
        TaggedBy = taggedBy;
    }
}
