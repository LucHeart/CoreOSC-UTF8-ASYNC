using System;

namespace LucHeart.CoreOSC;

public static class Utils
{
    public static DateTime TimeTagToDateTime(ulong val)
    {
        if (val == 1)
            return DateTime.Now;

        var seconds = (uint)(val >> 32);
        var time = DateTime.Parse("1900-01-01 00:00:00");
        time = time.AddSeconds(seconds);
        var fraction = TimeTagToFraction(val);
        time = time.AddSeconds(fraction);
        return time;
    }

    public static double TimeTagToFraction(ulong val)
    {
        if (val == 1)
            return 0.0;

        var seconds = (uint)(val & 0x00000000FFFFFFFF);
        var fraction = (double)seconds / 0xFFFFFFFF;
        return fraction;
    }

    public static ulong DateTimeToTimeTag(DateTime value)
    {
        ulong seconds = (uint)(value - DateTime.Parse("1900-01-01 00:00:00.000")).TotalSeconds;
        var fraction = (uint)Math.Ceiling(0xFFFFFFFF * ((double)value.Millisecond / 1000));

        var output = (seconds << 32) + fraction;
        return output;
    }

    public static int AlignedStringLength(string val)
    {
        var len = val.Length + (4 - val.Length % 4);
        if (len <= val.Length) len += 4;

        return len;
    }
}