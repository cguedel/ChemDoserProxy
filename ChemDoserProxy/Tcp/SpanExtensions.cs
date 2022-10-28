namespace ChemDoserProxy.Tcp;

public static class SpanExtensions
{
    public static ReadOnlySpan<byte> GetBytes(this Span<byte> buffer, int start, int length)
    {
        var reversed = buffer[start..(start + length)];
        reversed.Reverse();

        return reversed;
    }
}