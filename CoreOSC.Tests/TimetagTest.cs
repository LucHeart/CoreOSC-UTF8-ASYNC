using System;
using System.Threading.Tasks;

namespace LucHeart.CoreOSC.Tests;

public class TimetagTest
{
    [Test]
    public async Task TestTimetag()
    {
        ulong tag = 0;

        tag |= (ulong)3424309443 <<
               32; // https://www.timeanddate.com/date/durationresult.html?d1=1&m1=1&y1=1900&d2=6&m2=7&y2=2008&h1=0&i1=0&s1=0&h2=5&i2=4&s2=3
        tag |= 0xFFFFFFFF / 10; // 100ms

        var date = Utils.TimeTagToDateTime(tag);

        await Assert.That(date).IsEqualTo(DateTime.Parse("2008-7-06 05:04:03.100"));
    }

    [Test]
    public async Task TestDateTimeToTimetag()
    {
        var dt = DateTime.UtcNow;

        var l = Utils.DateTimeToTimeTag(dt);
        var dtBack = Utils.TimeTagToDateTime(l);

        await Assert.That(dtBack.Date).IsEqualTo(dt.Date);
        await Assert.That(dtBack.Hour).IsEqualTo(dt.Hour);
        await Assert.That(dtBack.Minute).IsEqualTo(dt.Minute);
        await Assert.That(dtBack.Second).IsEqualTo(dt.Second);
        await Assert.That(dtBack.Millisecond).IsEqualTo(dt.Millisecond);
#if !NETSTANDARD2_1
        await Assert.That(dtBack.Microsecond).IsEqualTo(dt.Microsecond);
#endif
    }
}