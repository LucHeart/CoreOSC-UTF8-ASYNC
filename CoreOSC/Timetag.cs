using System;

namespace LucHeart.CoreOSC;

public struct Timetag
{
    public ulong Tag;

    public DateTime Timestamp
    {
        get => Utils.TimetagToDateTime(Tag);
        set => Tag = Utils.DateTimeToTimetag(value);
    }

    /// <summary>
    /// Gets or sets the fraction of a second in the timestamp. the double precision number is multiplied by 2^32
    /// giving an accuracy down to about 230 picoseconds ( 1/(2^32) of a second)
    /// </summary>
    public double Fraction
    {
        get => Utils.TimetagToFraction(Tag);
        set => Tag = (Tag & 0xFFFFFFFF00000000) + (uint)(value * 0xFFFFFFFF);
    }

    public Timetag(ulong value)
    {
        Tag = value;
    }

    public Timetag(DateTime value)
    {
        Tag = 0;
        Timestamp = value;
    }

    public override bool Equals(object? obj) => obj switch
    {
        Timetag timeTag => Tag == timeTag.Tag,
        ulong ulongValue => Tag == ulongValue,
        _ => false
    };
    
    public static bool operator ==(Timetag a, Timetag b) => a.Equals(b);
    public static bool operator !=(Timetag a, Timetag b) => a.Equals(b);
    public override int GetHashCode() => (int)(((uint)(Tag >> 32) + (uint)(Tag & 0x00000000FFFFFFFF)) / 2);
}