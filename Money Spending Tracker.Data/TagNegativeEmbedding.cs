namespace Money_Spending_Tracker.Data;

public class TagNegativeEmbedding
{
    public int Id { get; set; }

    public int TagId { get; set; }
    public Tag? Tag { get; set; }

    public byte[] Embedding { get; set; }
    public string Hash { get; set; }

    public TagNegativeEmbedding(int tagId, byte[] embedding, string hash)
    {
        TagId = tagId;
        Embedding = embedding;
        Hash = hash;
    }
}
