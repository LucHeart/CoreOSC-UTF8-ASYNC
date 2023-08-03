using System;
using System.Collections.Generic;
using Xunit;

namespace LucHeart.CoreOSC.Tests;

public class ParseTest
{
    [Fact]
    public void TestDouble()
    {
        var val = 1234567.2324521e36;

        var msg = new OscMessage("/test/1", val);
        var bytes = msg.GetBytes();

        var msg2 = OscMessage.ParseMessage(bytes);
        Assert.Equal(val, (double)msg2.Arguments[0]!);
    }

    [Fact]
    public void TestBlob()
    {
        var blob = new byte[] { 23, 65, 255, 12, 6 };

        var msg = new OscMessage("/test/1", blob);
        var bytes = msg.GetBytes();

        var msg2 = OscMessage.ParseMessage(bytes);
        Assert.Equal(blob, (byte[])msg2.Arguments[0]!);
    }

    [Fact]
    public void TestTimetag()
    {
        var val = DateTime.Now;
        var tag = new TimeTag(val);

        var msg = new OscMessage("/test/1", tag);
        var bytes = msg.GetBytes();

        var msg2 = OscMessage.ParseMessage(bytes);
        Assert.Equal(tag.Tag, ((TimeTag)msg2.Arguments[0]!).Tag);
    }

    [Fact]
    public void TestLong()
    {
        var num = 123456789012345;
        var msg = new OscMessage("/test/1", num);
        var bytes = msg.GetBytes();

        var msg2 = OscMessage.ParseMessage(bytes);

        Assert.Equal(num, msg2.Arguments[0]);
    }

    [Fact]
    public void TestArray()
    {
        var list = new List<object> { 23, true, "hello world" };
        var msg = new OscMessage("/test/1", 9999, list, 24.24f);
        var bytes = msg.GetBytes();

        var msg2 = OscMessage.ParseMessage(bytes);

        Assert.Equal(9999, msg2.Arguments[0]);
        Assert.Equal(list, msg2.Arguments[1]);
        Assert.Equal(list.Count, ((List<object>)msg2.Arguments[1]!).Count);
        Assert.Equal(24.24f, msg2.Arguments[2]);
    }

    [Fact]
    public void TestNoAddress()
    {
        var msg = new OscMessage("", 9999, 24.24f);
        var bytes = msg.GetBytes();

        var msg2 = OscMessage.ParseMessage(bytes);

        Assert.Equal("", msg2.Address);
        Assert.Equal(9999, msg2.Arguments[0]);
        Assert.Equal(24.24f, msg2.Arguments[1]);
    }
}