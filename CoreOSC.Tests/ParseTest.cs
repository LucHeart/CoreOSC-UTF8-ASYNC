using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LucHeart.CoreOSC.Tests;

public class ParseTest
{
    [Test]
    public async Task TestDouble()
    {
        var val = 1234567.2324521e36;

        var msg = new OscMessage("/test/1", val);
        var bytes = msg.GetBytes();

        var msg2 = OscMessage.ParseMessage(bytes);
        await Assert.That(msg2.Arguments[0]).IsEqualTo(val);
    }

    [Test]
    public async Task TestBlob()
    {
        var blob = new byte[] { 23, 65, 255, 12, 6 };

        var msg = new OscMessage("/test/1", blob);
        var bytes = msg.GetBytes();

        var msg2 = OscMessage.ParseMessage(bytes);
        await Assert.That(msg2.Arguments[0]).IsEquivalentTo(blob);
    }

    [Test]
    public async Task TestTimetag()
    {
        var val = DateTime.Now;
        var tag = new TimeTag(val);

        var msg = new OscMessage("/test/1", tag);
        var bytes = msg.GetBytes();

        var msg2 = OscMessage.ParseMessage(bytes);
        await Assert.That(((TimeTag)msg2.Arguments[0]!).Tag).IsEqualTo(tag.Tag);
    }

    [Test]
    public async Task TestLong()
    {
        var num = 123456789012345;
        var msg = new OscMessage("/test/1", num);
        var bytes = msg.GetBytes();

        var msg2 = OscMessage.ParseMessage(bytes);

        await Assert.That(msg2.Arguments[0]).IsEqualTo(num);
    }

    [Test]
    public async Task TestArray()
    {
        var list = new List<object> { 23, true, "hello world" };
        var msg = new OscMessage("/test/1", 9999, list, 24.24f);
        var bytes = msg.GetBytes();

        var msg2 = OscMessage.ParseMessage(bytes);

        await Assert.That(msg2.Arguments[0]).IsEqualTo(9999);
        await Assert.That(msg2.Arguments[1]).IsEquivalentTo(list);
        await Assert.That(((List<object>)msg2.Arguments[1]!).Count).IsEqualTo(list.Count);
        await Assert.That(msg2.Arguments[2]).IsEqualTo(24.24f);
    }

    [Test]
    public async Task TestNoAddress()
    {
        var msg = new OscMessage("", 9999, 24.24f);
        var bytes = msg.GetBytes();

        var msg2 = OscMessage.ParseMessage(bytes);

        await Assert.That(msg2.Address).IsEqualTo("");
        await Assert.That(msg2.Arguments[0]).IsEqualTo(9999);
        await Assert.That(msg2.Arguments[1]).IsEqualTo(24.24f);
    }
}