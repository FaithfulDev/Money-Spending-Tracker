using System.Security.Cryptography;

namespace Money_Spending_Tracker.Data;

public class TagNegativeEmbedding
{
    public int Id { get; set; }

    public int TagId { get; set; }
    public Tag? Tag { get; set; }

    public byte[] Embedding { get; set; }
    public string Hash { get; set; }

    public TagNegativeEmbedding(int tagId, byte[] embedding)
    {
        TagId = tagId;
        Embedding = embedding;
        Hash = ComputeEmbeddingHash(embedding);
    }

    /// <summary>
    /// Computes a SHA256 hash of the embedding byte array for deduplication purposes.
    /// </summary>
    private static string ComputeEmbeddingHash(byte[] embedding)
    {
        byte[] hashBytes = SHA256.HashData(embedding);
        return Convert.ToHexString(hashBytes);
    }
}
