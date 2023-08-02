using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace LucHeart.CoreOSC.Tests;

public class IntegrationTest
{
    [Fact(Timeout = 1000)]
    public async Task TestMessage()
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var listener = new UdpListener(endpoint);
        using var sender = new UdpSender(endpoint);

        // Test every message type (except Symbol)
        var msg1 = new OscMessage(
            "/test/address",
            23,
            42.42f,
            "hello world",
            new byte[] { 2, 3, 4 },
            -123456789123,
            new TimeTag(DateTime.Now.Date).Tag,
            new TimeTag(DateTime.Now.Date.AddMonths(1)),
            1234567.890,
            new Symbol("wut wut"),
            'x',
            new RGBA(20, 40, 60, 255),
            new Midi(3, 110, 55, 66),
            true,
            false,
            null,
            double.PositiveInfinity
        );
        

        await sender.SendAsync(msg1);
        
        var msgRevc = (await listener.ReceiveAsync()).AsT0;
        Assert.NotNull(msgRevc);
        
        Assert.Equal("/test/address", msgRevc.Address);
        Assert.Equal(16, msgRevc.Arguments.Length);

        Assert.Equal(23, msgRevc.Arguments[0]);
        Assert.Equal(42.42f, msgRevc.Arguments[1]);
        Assert.Equal("hello world", msgRevc.Arguments[2]);
        Assert.Equal(new byte[] { 2, 3, 4 }, msgRevc.Arguments[3]);
        Assert.Equal(-123456789123, msgRevc.Arguments[4]);
        Assert.Equal(new TimeTag(DateTime.Now.Date), msgRevc.Arguments[5]);
        Assert.Equal(new TimeTag(DateTime.Now.Date.AddMonths(1)), msgRevc.Arguments[6]);
        Assert.Equal(1234567.890, msgRevc.Arguments[7]);
        Assert.Equal(new Symbol("wut wut"), msgRevc.Arguments[8]);
        Assert.Equal('x', msgRevc.Arguments[9]);
        Assert.Equal(new RGBA(20, 40, 60, 255), msgRevc.Arguments[10]);
        Assert.Equal(new Midi(3, 110, 55, 66), msgRevc.Arguments[11]);
        Assert.Equal(true, msgRevc.Arguments[12]);
        Assert.Equal(false, msgRevc.Arguments[13]);
        Assert.Null(msgRevc.Arguments[14]);
        Assert.Equal(double.PositiveInfinity, msgRevc.Arguments[15]);
    }

    [Fact(Timeout = 1000)]
    public async Task TestBundle()
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var listener = new UdpListener(endpoint);
        using var sender1 = new UdpSender(endpoint);
        var msg1 = new OscMessage("/test/address1", 23, 42.42f, "hello world", new byte[] { 2, 3, 4 });
        var msg2 = new OscMessage("/test/address2", 34, 24.24f, "hello again", new byte[] { 5, 6, 7, 8, 9 });
        var dt = DateTime.Now;
        var bundle = new OscBundle(Utils.DateTimeToTimeTag(dt), msg1, msg2);

        await sender1.SendAsync(bundle);
        await sender1.SendAsync(bundle);
        await sender1.SendAsync(bundle);

        await listener.ReceiveAsync();
        await listener.ReceiveAsync();
        var receivedMessage = (await listener.ReceiveAsync()).AsT1;

        Assert.Equal(dt.Date, receivedMessage.Timestamp.Date);
        Assert.Equal(dt.Hour, receivedMessage.Timestamp.Hour);
        Assert.Equal(dt.Minute, receivedMessage.Timestamp.Minute);
        Assert.Equal(dt.Second, receivedMessage.Timestamp.Second);
        Assert.Equal(dt.Millisecond, receivedMessage.Timestamp.Millisecond);
        
        Assert.Equal("/test/address1", receivedMessage.Messages[0].Address);
        Assert.Equal(4, receivedMessage.Messages[0].Arguments.Length);
        Assert.Equal(23, receivedMessage.Messages[0].Arguments[0]);
        Assert.Equal(42.42f, receivedMessage.Messages[0].Arguments[1]);
        Assert.Equal("hello world", receivedMessage.Messages[0].Arguments[2]);
        Assert.Equal(new byte[] { 2, 3, 4 }, receivedMessage.Messages[0].Arguments[3]);

        Assert.Equal("/test/address2", receivedMessage.Messages[1].Address);
        Assert.Equal(4, receivedMessage.Messages[1].Arguments.Length);
        Assert.Equal(34, receivedMessage.Messages[1].Arguments[0]);
        Assert.Equal(24.24f, receivedMessage.Messages[1].Arguments[1]);
        Assert.Equal("hello again", receivedMessage.Messages[1].Arguments[2]);
        Assert.Equal(new byte[] { 5, 6, 7, 8, 9 }, receivedMessage.Messages[1].Arguments[3]);
    }
}