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
        await Verify(msgRevc);
    }

    [Test, Timeout(5000)]
    public async Task TestBundle(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var listener = new OscListener(endpoint);
        using var sender1 = new OscSender(endpoint);
        var msg1 = new OscMessage("/test/address1", 23, 42.42f, "hello world", new byte[] { 2, 3, 4 });
        var msg2 = new OscMessage("/test/address2", 34, 24.24f, "hello again", new byte[] { 5, 6, 7, 8, 9 });
        var dt = Utils.DateTimeToTimeTag(DateTime.Parse("2025-12-12T12:34:56Z"));
        var bundle = new OscBundle(dt, msg1, msg2);

        await sender1.SendAsync(bundle);
        await sender1.SendAsync(bundle);
        await sender1.SendAsync(bundle);

        await listener.ReceiveBundleAsync();
        await listener.ReceiveBundleAsync();
        var receivedMessage = await listener.ReceiveBundleAsync();
        await Verify(receivedMessage);
    }
}