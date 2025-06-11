namespace LucHeart.CoreOSC.Tests;

public class DuplexTest
{
    /// <summary>
    /// Duplex connection simple message test
    /// </summary>
    [Test, Timeout(5000)]
    public async Task DuplexSendReceive(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var duplex = new OscDuplex(endpoint, endpoint);
        
        var msg = new OscMessage("/test/", 23.42f);
        await duplex.SendAsync(msg);
        var received = await duplex.ReceiveMessageAsync();
        await Verify(received);
    }
    
    /// <summary>
    /// Duplex connection message with utf8 content
    /// </summary>
    [Test, Timeout(5000)]
    public async Task DuplexUft8(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var duplex = new OscDuplex(endpoint, endpoint);
    
        var msg = new OscMessage("/test/", "⚡");
        await duplex.SendAsync(msg);
        var received = await duplex.ReceiveMessageAsync();
        await Verify(received);
    }
}