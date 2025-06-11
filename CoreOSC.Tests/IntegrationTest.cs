using System;
using System.Threading;
using System.Threading.Tasks;

namespace LucHeart.CoreOSC.Tests;

public class IntegrationTest
{
    [Test, Timeout(5000)]
    public async Task TestMessage(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var listener = new OscListener(endpoint);
        using var sender = new OscSender(endpoint);

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
        
        var msgRevc = await listener.ReceiveMessageAsync();
        await Assert.That(msgRevc).IsNotNull();
        await Assert.That(msgRevc.Address).IsEqualTo("/test/address");
        await Assert.That(msgRevc.Arguments.Length).IsEqualTo(16);
        await Assert.That(msgRevc.Arguments[0]).IsEqualTo(23);
        await Assert.That(msgRevc.Arguments[1]).IsEqualTo(42.42f);
        await Assert.That(msgRevc.Arguments[2]).IsEqualTo("hello world");
        await Assert.That(msgRevc.Arguments[3]).IsEquivalentTo(new byte[] { 2, 3, 4 });
        await Assert.That(msgRevc.Arguments[4]).IsEqualTo(-123456789123);
        await Assert.That(msgRevc.Arguments[5]).IsEqualTo(new TimeTag(DateTime.Now.Date));
        await Assert.That(msgRevc.Arguments[6]).IsEqualTo(new TimeTag(DateTime.Now.Date.AddMonths(1)));
        await Assert.That(msgRevc.Arguments[7]).IsEqualTo(1234567.890);
        await Assert.That(msgRevc.Arguments[8]).IsEqualTo(new Symbol("wut wut"));
        await Assert.That(msgRevc.Arguments[9]).IsEqualTo('x');
        await Assert.That(msgRevc.Arguments[10]).IsEqualTo(new RGBA(20, 40, 60, 255));
        await Assert.That(msgRevc.Arguments[11]).IsEqualTo(new Midi(3, 110, 55, 66));
        await Assert.That(msgRevc.Arguments[12]).IsEqualTo(true);
        await Assert.That(msgRevc.Arguments[13]).IsEqualTo(false);
        await Assert.That(msgRevc.Arguments[14]).IsNull();
        await Assert.That(msgRevc.Arguments[15]).IsEqualTo(double.PositiveInfinity);
    }

    [Test, Timeout(5000)]
    public async Task TestBundle(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var listener = new OscListener(endpoint);
        using var sender1 = new OscSender(endpoint);
        var msg1 = new OscMessage("/test/address1", 23, 42.42f, "hello world", new byte[] { 2, 3, 4 });
        var msg2 = new OscMessage("/test/address2", 34, 24.24f, "hello again", new byte[] { 5, 6, 7, 8, 9 });
        var dt = DateTime.Now;
        var bundle = new OscBundle(Utils.DateTimeToTimeTag(dt), msg1, msg2);

        await sender1.SendAsync(bundle);
        await sender1.SendAsync(bundle);
        await sender1.SendAsync(bundle);

        await listener.ReceiveBundleAsync();
        await listener.ReceiveBundleAsync();
        var receivedMessage = await listener.ReceiveBundleAsync();

        await Assert.That(receivedMessage.Timestamp.Date).IsEqualTo(dt.Date);
        await Assert.That(receivedMessage.Timestamp.Hour).IsEqualTo(dt.Hour);
        await Assert.That(receivedMessage.Timestamp.Minute).IsEqualTo(dt.Minute);
        await Assert.That(receivedMessage.Timestamp.Second).IsEqualTo(dt.Second);
        await Assert.That(receivedMessage.Timestamp.Millisecond).IsEqualTo(dt.Millisecond);
        
        await Assert.That(receivedMessage.Messages[0].Address).IsEqualTo("/test/address1");
        await Assert.That(receivedMessage.Messages[0].Arguments.Length).IsEqualTo(4);
        await Assert.That(receivedMessage.Messages[0].Arguments[0]).IsEqualTo(23);
        await Assert.That(receivedMessage.Messages[0].Arguments[1]).IsEqualTo(42.42f);
        await Assert.That(receivedMessage.Messages[0].Arguments[2]).IsEqualTo("hello world");
        await Assert.That(receivedMessage.Messages[0].Arguments[3]).IsEquivalentTo(new byte[] { 2, 3, 4 });

        await Assert.That(receivedMessage.Messages[1].Address).IsEqualTo("/test/address2");
        await Assert.That(receivedMessage.Messages[1].Arguments.Length).IsEqualTo(4);
        await Assert.That(receivedMessage.Messages[1].Arguments[0]).IsEqualTo(34);
        await Assert.That(receivedMessage.Messages[1].Arguments[1]).IsEqualTo(24.24f);
        await Assert.That(receivedMessage.Messages[1].Arguments[2]).IsEqualTo("hello again");
        await Assert.That(receivedMessage.Messages[1].Arguments[3]).IsEquivalentTo(new byte[] { 5, 6, 7, 8, 9 });
    }
}