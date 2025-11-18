namespace NahidaImpact.KcpSharp.Base;

internal sealed class DefaultArrayPoolBufferAllocator : IKcpBufferPool
{
    public static DefaultArrayPoolBufferAllocator Default { get; } = new();

    public KcpRentedBuffer Rent(KcpBufferPoolRentOptions options)
    {
        return KcpRentedBuffer.FromSharedArrayPool(options.Size);
    }
}