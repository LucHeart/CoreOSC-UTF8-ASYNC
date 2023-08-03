using System;

namespace LucHeart.CoreOSC;

public static class SpanExtensions
{
    public static Span<T> ReverseReturning<T>(this Span<T> span)
    {
        span.Reverse();
        return span;
    }
    
    public static Span<T> ReverseSlice<T>(this Span<T> span, int index) => span[index..].ReverseReturning();
    public static Span<T> ReverseSlice<T>(this Span<T> span, int index, int length) => span.Slice(index, length).ReverseReturning();
}