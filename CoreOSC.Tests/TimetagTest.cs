using System;
using NUnit.Framework;

namespace LucHeart.CoreOSC.Tests;

[TestFixture]
public class TimetagTest
{
    [TestCase]
    public void TestTimetag()
    {
        var time = (ulong) 60 * 60 * 24 * 365 * 108;
        time <<= 32;
        time += (ulong)(Math.Pow(2, 32) / 2);
        var date = Utils.TimetagToDateTime(time);

        Assert.AreEqual(DateTime.Parse("2007-12-06 00:00:00.500"), date);
    }

    [TestCase]
    public void TestDateTimeToTimetag()
    {
        var dt = DateTime.UtcNow;

        var l = Utils.DateTimeToTimetag(dt);
        Console.WriteLine(l);
        var dtBack = Utils.TimetagToDateTime(l);

        Assert.AreEqual(dt.Date, dtBack.Date);
        Assert.AreEqual(dt.Hour, dtBack.Hour);
        Assert.AreEqual(dt.Minute, dtBack.Minute);
        Assert.AreEqual(dt.Second, dtBack.Second);
        Assert.AreEqual(dt.Millisecond, dtBack.Millisecond);
    }
}