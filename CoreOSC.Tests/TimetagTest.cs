using System;
using Xunit;

namespace LucHeart.CoreOSC.Tests;

public class TimetagTest
{
    [Fact]
    public void TestTimetag()
    {
        ulong tag = 0;

        tag |= (ulong)3424309443 << 32; // https://www.timeanddate.com/date/durationresult.html?d1=1&m1=1&y1=1900&d2=6&m2=7&y2=2008&h1=0&i1=0&s1=0&h2=5&i2=4&s2=3
        tag |= (ulong)(0xFFFFFFFF / 10); // 100ms

        var date = Utils.TimeTagToDateTime(tag);

        Assert.Equal(DateTime.Parse("2008-7-06 05:04:03.100"), date);
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
        Assert.Equal(dt.Microsecond, dtBack.Microsecond);
    }
}