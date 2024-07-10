using System;

namespace LucHeart.CoreOSC;

public struct TimeTag
{
    public static TimeTag Immediate => new(1);

    public ulong Tag;

    public DateTime Timestamp
    {
        get => Utils.TimeTagToDateTime(Tag);
        set => Tag = Utils.DateTimeToTimeTag(value);
    }

    /// <summary>
    /// Gets or sets the fraction of a second in the timestamp. the double precision number is multiplied by 2^32
    /// giving an accuracy down to about 230 picoseconds ( 1/(2^32) of a second)
    /// </summary>
    public double Fraction
    {
        get => Utils.TimeTagToFraction(Tag);
        set => Tag = Utils.FractionToTimeTag(value);
    }

    public TimeTag(ulong value)
    {
        Tag = value;
    }

    public TimeTag(DateTime value)
    {
        Tag = 0;
        Timestamp = value;
    }

    public override bool Equals(object? obj) => obj switch
    {
        TimeTag timeTag => Tag == timeTag.Tag,
        ulong ulongValue => Tag == ulongValue,
        _ => false
    };
    
    public static bool operator ==(TimeTag a, TimeTag b) => a.Equals(b);
    public static bool operator !=(TimeTag a, TimeTag b) => a.Equals(b);
    public override int GetHashCode() => (int)(((uint)(Tag >> 32) + (uint)(Tag & 0x00000000FFFFFFFF)) / 2);
}