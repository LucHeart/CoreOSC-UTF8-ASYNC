using System.Net;

namespace LucHeart.CoreOSC.Tests;

/// <summary>
/// Network integration tests covering untested OscListener, OscSender, and OscDuplex features.
/// </summary>
public class NetworkTest
{
    #region TransparentBundleToMessageConversion

    [Test, Timeout(5000)]
    public async Task ListenerTransparentBundleConversion(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var listener = new OscListener(endpoint);
        using var sender = new OscSender(endpoint);

        listener.EnableTransparentBundleToMessageConversion = true;

        var msg1 = new OscMessage("/a", 1);
        var msg2 = new OscMessage("/b", 2);
        var msg3 = new OscMessage("/c", 3);
        var bundle = new OscBundle(1, msg1, msg2, msg3);

        await sender.SendAsync(bundle);

        // Should receive each message individually from the bundle
        var r1 = await listener.ReceiveMessageAsync(ct);
        var r2 = await listener.ReceiveMessageAsync(ct);
        var r3 = await listener.ReceiveMessageAsync(ct);

        await Assert.That(r1.Address).IsEqualTo("/a");
        await Assert.That(r1.Arguments[0]).IsEqualTo(1);
        await Assert.That(r2.Address).IsEqualTo("/b");
        await Assert.That(r2.Arguments[0]).IsEqualTo(2);
        await Assert.That(r3.Address).IsEqualTo("/c");
        await Assert.That(r3.Arguments[0]).IsEqualTo(3);
    }

    [Test, Timeout(5000)]
    public async Task ListenerTransparentPassesThroughRegularMessages(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var listener = new OscListener(endpoint);
        using var sender = new OscSender(endpoint);

        listener.EnableTransparentBundleToMessageConversion = true;

        var msg = new OscMessage("/regular", 42);
        await sender.SendAsync(msg);

        var received = await listener.ReceiveMessageAsync(ct);
        await Assert.That(received.Address).IsEqualTo("/regular");
        await Assert.That(received.Arguments[0]).IsEqualTo(42);
    }

    [Test, Timeout(5000)]
    public async Task ListenerTransparentMultipleBundles(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var listener = new OscListener(endpoint);
        using var sender = new OscSender(endpoint);

        listener.EnableTransparentBundleToMessageConversion = true;

        var bundle1 = new OscBundle(1, new OscMessage("/b1a", 1), new OscMessage("/b1b", 2));
        var bundle2 = new OscBundle(1, new OscMessage("/b2a", 3));

        await sender.SendAsync(bundle1);
        await sender.SendAsync(bundle2);

        // Bundle 1 messages
        var r1 = await listener.ReceiveMessageAsync(ct);
        var r2 = await listener.ReceiveMessageAsync(ct);
        await Assert.That(r1.Address).IsEqualTo("/b1a");
        await Assert.That(r2.Address).IsEqualTo("/b1b");

        // Bundle 2 messages
        var r3 = await listener.ReceiveMessageAsync(ct);
        await Assert.That(r3.Address).IsEqualTo("/b2a");
    }

    #endregion

    #region ReceiveMessageExAsync / ReceiveBundleExAsync

    [Test, Timeout(5000)]
    public async Task ListenerReceiveMessageExReturnsEndpoint(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var listener = new OscListener(endpoint);
        using var sender = new OscSender(endpoint);

        await sender.SendAsync(new OscMessage("/test", 1));

        var (message, remoteEndPoint) = await listener.ReceiveMessageExAsync(ct);

        await Assert.That(message.Address).IsEqualTo("/test");
        await Assert.That(message.Arguments[0]).IsEqualTo(1);
        await Assert.That(remoteEndPoint).IsNotNull();
        await Assert.That(remoteEndPoint.Address).IsEqualTo(IPAddress.Loopback);
    }

    [Test, Timeout(5000)]
    public async Task ListenerReceiveBundleExReturnsEndpoint(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var listener = new OscListener(endpoint);
        using var sender = new OscSender(endpoint);

        var bundle = new OscBundle(1, new OscMessage("/test", 42));
        await sender.SendAsync(bundle);

        var (receivedBundle, remoteEndPoint) = await listener.ReceiveBundleExAsync(ct);

        await Assert.That(receivedBundle.Messages.Count).IsEqualTo(1);
        await Assert.That(receivedBundle.Messages[0].Address).IsEqualTo("/test");
        await Assert.That(remoteEndPoint).IsNotNull();
        await Assert.That(remoteEndPoint.Address).IsEqualTo(IPAddress.Loopback);
    }

    #endregion

    #region SendAsync overloads

    [Test, Timeout(5000)]
    public async Task SenderSendRawBytes(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var listener = new OscListener(endpoint);
        using var sender = new OscSender(endpoint);

        var msg = new OscMessage("/raw", 99);
        var bytes = msg.GetBytes();

        await sender.SendAsync(bytes);

        var received = await listener.ReceiveMessageAsync(ct);
        await Assert.That(received.Address).IsEqualTo("/raw");
        await Assert.That(received.Arguments[0]).IsEqualTo(99);
    }

    [Test, Timeout(5000)]
    public async Task SenderSendToCustomEndpoint(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var listener = new OscListener(endpoint);
        // Sender has a dummy default endpoint — we send to the real one explicitly
        var dummyEndpoint = TestUtils.GetNextEndpoint();
        using var sender = new OscSender(dummyEndpoint);

        var msg = new OscMessage("/custom", 77);
        await sender.SendAsync(endpoint, msg);

        var received = await listener.ReceiveMessageAsync(ct);
        await Assert.That(received.Address).IsEqualTo("/custom");
        await Assert.That(received.Arguments[0]).IsEqualTo(77);
    }

    [Test, Timeout(5000)]
    public async Task SenderSendRawBytesToCustomEndpoint(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var listener = new OscListener(endpoint);
        var dummyEndpoint = TestUtils.GetNextEndpoint();
        using var sender = new OscSender(dummyEndpoint);

        var msg = new OscMessage("/rawcustom", 55);
        await sender.SendAsync(endpoint, msg.GetBytes());

        var received = await listener.ReceiveMessageAsync(ct);
        await Assert.That(received.Address).IsEqualTo("/rawcustom");
        await Assert.That(received.Arguments[0]).IsEqualTo(55);
    }

    #endregion

    #region Duplex bundle

    [Test, Timeout(5000)]
    public async Task DuplexSendReceiveBundle(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var duplex = new OscDuplex(endpoint, endpoint);

        var bundle = new OscBundle(1,
            new OscMessage("/d1", 10),
            new OscMessage("/d2", 20));

        await duplex.SendAsync(bundle);

        var received = await duplex.ReceiveBundleAsync(ct);
        await Assert.That(received.Messages.Count).IsEqualTo(2);
        await Assert.That(received.Messages[0].Address).IsEqualTo("/d1");
        await Assert.That(received.Messages[1].Address).IsEqualTo("/d2");
    }

    [Test, Timeout(5000)]
    public async Task DuplexSendToCustomEndpoint(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        var dummyEndpoint = TestUtils.GetNextEndpoint();
        using var listener = new OscListener(endpoint);
        using var duplex = new OscDuplex(dummyEndpoint, dummyEndpoint);

        var msg = new OscMessage("/duplexcustom", 33);
        await duplex.SendAsync(endpoint, msg);

        var received = await listener.ReceiveMessageAsync(ct);
        await Assert.That(received.Address).IsEqualTo("/duplexcustom");
        await Assert.That(received.Arguments[0]).IsEqualTo(33);
    }

    [Test, Timeout(5000)]
    public async Task DuplexSendRawBytesToCustomEndpoint(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        var dummyEndpoint = TestUtils.GetNextEndpoint();
        using var listener = new OscListener(endpoint);
        using var duplex = new OscDuplex(dummyEndpoint, dummyEndpoint);

        var msg = new OscMessage("/duplexraw", 44);
        await duplex.SendAsync(endpoint, msg.GetBytes());

        var received = await listener.ReceiveMessageAsync(ct);
        await Assert.That(received.Address).IsEqualTo("/duplexraw");
        await Assert.That(received.Arguments[0]).IsEqualTo(44);
    }

    #endregion

    #region All types over network

    [Test, Timeout(5000)]
    public async Task SendReceiveAllTypes(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var listener = new OscListener(endpoint);
        using var sender = new OscSender(endpoint);

        var blob = new byte[] { 10, 20, 30 };
        var tag = new TimeTag(DateTime.Parse("2025-01-01T00:00:00Z"));
        var msg = new OscMessage("/all",
            42,
            3.14f,
            "hello",
            blob,
            999999999999L,
            tag,
            2.718281828,
            new Symbol("sym"),
            'X',
            new RGBA(1, 2, 3, 4),
            new Midi(5, 6, 7, 8),
            true,
            false,
            null,
            double.PositiveInfinity
        );

        await sender.SendAsync(msg);
        var parsed = await listener.ReceiveMessageAsync(ct);

        await Assert.That(parsed.Address).IsEqualTo("/all");
        await Assert.That(parsed.Arguments.Length).IsEqualTo(15);
        await Assert.That(parsed.Arguments[0]).IsEqualTo(42);
        await Assert.That(parsed.Arguments[1]).IsEqualTo(3.14f);
        await Assert.That(parsed.Arguments[2]).IsEqualTo("hello");
        await Assert.That((byte[])parsed.Arguments[3]!).IsEquivalentTo(blob);
        await Assert.That(parsed.Arguments[4]).IsEqualTo(999999999999L);
        await Assert.That(((TimeTag)parsed.Arguments[5]!).Tag).IsEqualTo(tag.Tag);
        await Assert.That(parsed.Arguments[6]).IsEqualTo(2.718281828);
        await Assert.That(((Symbol)parsed.Arguments[7]!).Value).IsEqualTo("sym");
        await Assert.That(parsed.Arguments[8]).IsEqualTo('X');
        await Assert.That(parsed.Arguments[9]).IsEqualTo(new RGBA(1, 2, 3, 4));
        await Assert.That(parsed.Arguments[10]).IsEqualTo(new Midi(5, 6, 7, 8));
        await Assert.That((bool)parsed.Arguments[11]!).IsTrue();
        await Assert.That((bool)parsed.Arguments[12]!).IsFalse();
        await Assert.That(parsed.Arguments[13]).IsNull();
        await Assert.That(parsed.Arguments[14]).IsEqualTo(double.PositiveInfinity);
    }

    #endregion
}