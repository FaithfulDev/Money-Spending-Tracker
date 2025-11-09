using System.Runtime.InteropServices;

namespace Money_Spending_Tracker.Features.Tags;

internal static class ByteFloatConversionHelper
{
    public static byte[]? FloatArrayToByteArray(float[] floats)
    {
        if (floats == null) return null;
        return MemoryMarshal.AsBytes<float>(floats.AsSpan()).ToArray();
    }

    public static float[]? ByteArrayToFloatArray(byte[] bytes)
    {
        if (bytes == null) return null;
        return MemoryMarshal.Cast<byte, float>(bytes).ToArray();
    }
}
