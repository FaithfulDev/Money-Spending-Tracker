using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;
using System.Numerics.Tensors;

namespace Money_Spending_Tracker.Features.Tags;

internal class TagPredictionService : IDisposable
{
    private const int MAX_LENGTH = 128;
    private const int PAD_TOKEN_ID = 0; // Assuming [PAD] token ID is 0, adjust if different
    private const double MIN_SIMILARITY = 0.8;
    private const double MIN_SCORE_THRESHOLD = 0.05;

    private BertTokenizer? _tokenizer;
    private InferenceSession? _session;
    private bool _isDisposed = false;

    public async Task InitializeAsync()
    {
        _tokenizer = await BertTokenizer.CreateAsync(
            await FileSystem.OpenAppPackageFileAsync("vocab.txt"), new BertOptions() { PaddingToken = "[PAD]" }
        );
        _session = new InferenceSession(await LoadModelAsync());
    }

    public float[] GetTransactionEmbedding(string text)
    {
        if (_tokenizer == null || _session == null)
        {
            throw new InvalidOperationException("TagPredictionService is not initialized. Call InitializeAsync() before using this method.");
        }

        // Tokenize
        var inputIds = _tokenizer.EncodeToIds(text).Select(id => (long)id).ToList();

        // Pad or truncate input IDs
        inputIds = PadOrTruncate(inputIds, MAX_LENGTH, PAD_TOKEN_ID);

        // Attention mask: 1 for non-pad, 0 for pad
        var attentionMask = inputIds.Select(id => id == PAD_TOKEN_ID ? 0L : 1L).ToList();

        // Token type IDs: all zeros for single sentence
        var tokenTypeIds = Enumerable.Repeat(0L, MAX_LENGTH).ToList();

        // Convert to tensors
        var inputIdsTensor = new DenseTensor<long>(inputIds.ToArray(), [1, MAX_LENGTH]);
        var attentionMaskTensor = new DenseTensor<long>(attentionMask.ToArray(), [1, MAX_LENGTH]);
        var tokenTypeIdsTensor = new DenseTensor<long>(tokenTypeIds.ToArray(), [1, MAX_LENGTH]);

        // Prepare ONNX input
        var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
                NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor),
                NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIdsTensor)
            };

        using var results = _session.Run(inputs);
        var pooledTensor = results[1].AsTensor<float>();

        return pooledTensor.ToArray();
    }

    public static List<int> PredictTags(
        float[] newEmbedding,
        List<(float[] embedding, int tagId)> positiveEmbeddings,
        List<(float[] embedding, int tagId)> negativeEmbeddings
    )
    {
        var tags = positiveEmbeddings.Select(pe => pe.tagId)
                                     .Distinct();

        List<int> bestTagIds = [];

        foreach (var tagId in tags)
        {
            var posEmbeds = positiveEmbeddings.Where(pe => pe.tagId == tagId).Select(pe => pe.embedding);
            double maxPos = posEmbeds.Any() ? posEmbeds.Max(e => TensorPrimitives.CosineSimilarity(newEmbedding, e)) : 0;

            if (maxPos < MIN_SIMILARITY)
            {
                continue;
            }

            var negEmbeds = negativeEmbeddings.Where(ne => ne.tagId == tagId).Select(ne => ne.embedding);
            double maxNeg = negEmbeds.Any() ? negEmbeds.Max(e => TensorPrimitives.CosineSimilarity(newEmbedding, e)) : 0;

            if (maxNeg < MIN_SIMILARITY)
            {
                maxNeg = 0;
            }

            double score = maxPos - maxNeg;

            if (score > MIN_SCORE_THRESHOLD)
            {
                bestTagIds.Add(tagId);
            }
        }

        return bestTagIds;
    }

    private static async Task<byte[]> LoadModelAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("all-MiniLM-L6-v2.onnx");
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }

    private static List<long> PadOrTruncate(List<long> source, int length, long padValue)
    {
        var arr = source.Take(length).ToList();
        while (arr.Count < length)
        {
            arr.Add(padValue);
        }
        return arr;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing)
        {
            _session?.Dispose();
            _session = null;
            _tokenizer = null;
        }

        _isDisposed = true;
    }
}
