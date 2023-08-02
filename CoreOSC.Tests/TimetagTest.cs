using System;
using Xunit;

namespace LucHeart.CoreOSC.Tests;

public class TimetagTest
{
    [Fact]
    public void TestTimetag()
    {
        var time = (ulong) 60 * 60 * 24 * 365 * 108;
        time <<= 32;
        time += (ulong)(Math.Pow(2, 32) / 2);
        var date = Utils.TimeTagToDateTime(time);

        Assert.Equal(DateTime.Parse("2007-12-06 00:00:00.500"), date);
    }

    [Fact]
    public void TestDateTimeToTimetag()
    {
        var dt = DateTime.UtcNow;

        var l = Utils.DateTimeToTimeTag(dt);
        var dtBack = Utils.TimeTagToDateTime(l);

        Assert.Equal(dt.Date, dtBack.Date);
        Assert.Equal(dt.Hour, dtBack.Hour);
        Assert.Equal(dt.Minute, dtBack.Minute);
        Assert.Equal(dt.Second, dtBack.Second);
        Assert.Equal(dt.Millisecond, dtBack.Millisecond);
    }
}