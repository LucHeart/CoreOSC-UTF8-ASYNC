using System;

namespace LucHeart.CoreOSC;

public static class Utils
{
    private static readonly long EpochTicks = new DateTime(1900, 1, 1).Ticks;
    private const uint OscTicksPerSecond = 0xFFFFFFFF;
    private const double OscTicksPerDotNetTick = OscTicksPerSecond / (double)TimeSpan.TicksPerSecond; // 429.4967295

    public static DateTime TimeTagToDateTime(ulong val)
    {
        if (val == 1)
            return DateTime.Now;

        var seconds = (uint)(val >> 32);
        var fraction = (uint)(val & 0xFFFFFFFF);

        long secondsTicks = seconds * TimeSpan.TicksPerSecond;
        var fractionTicks = (long)Math.Round(fraction / OscTicksPerDotNetTick); // We will loose accuracy in this conversion since there is about 430 OSC ticks per dotnet tick

        return new DateTime(EpochTicks + secondsTicks + fractionTicks);
    }

    public static ulong DateTimeToTimeTag(DateTime value)
    {
        long ticks = value.Ticks - EpochTicks;
        if (ticks < 0) return 0;

        var seconds = (uint)(ticks / TimeSpan.TicksPerSecond);
        var fractions = (uint)Math.Round((ticks - (seconds * TimeSpan.TicksPerSecond)) * OscTicksPerDotNetTick);

        var secondTicks = (ulong)seconds << 32;
        var fractionTicks = (ulong)fractions & 0xFFFFFFFF;

        return secondTicks | fractionTicks;
    }

    public static double TimeTagToSeconds(ulong val)
    {
        if (val == 1)
            return 0.0;

        return (double)val / OscTicksPerSecond;
    }

    public static ulong SecondsToTimeTag(double value)
    {
        return (ulong)(value * OscTicksPerSecond);
    }

    public static double TimeTagToFraction(ulong val)
    {
        if (val == 1)
            return 0.0;

        return (double)(val & 0xFFFFFFFF) / OscTicksPerSecond;
    }

    public static int AlignedStringLength(string val)
    {
        var len = val.Length + (4 - val.Length % 4);
        if (len <= val.Length) len += 4;

        return len;
    }
}