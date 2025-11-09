namespace Money_Spending_Tracker.Data;

public class Tag
{
    public int Id { get; set; }

    public string Name { get; set; }

    public List<TransactionTag> TransactionTags { get; set; } = [];

    public List<TagNegativeEmbedding> NegativeEmbeddings { get; set; } = [];

    public Tag(string name)
    {
        Name = name;
    }
}
