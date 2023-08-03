using System.Threading.Tasks;
using Xunit;

namespace LucHeart.CoreOSC.Tests;

public class DuplexTest
{
    /// <summary>
    /// Duplex connection simple message test
    /// </summary>
    [Fact(Timeout = 5000)]
    public async Task DuplexSendReceive()
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var duplex = new UdpDuplex(endpoint, endpoint);
        
        var msg = new OscMessage("/test/", 23.42f);
        await duplex.SendAsync(msg);
        var received = await duplex.ReceiveMessageAsync();
        Assert.Equal("/test/", received.Address);
        Assert.Equal(23.42f, received.Arguments[0]);
    }
    
    /// <summary>
    /// Duplex connection message with utf8 content
    /// </summary>
    [Fact(Timeout = 5000)]
    public async Task DuplexUft8()
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var duplex = new UdpDuplex(endpoint, endpoint);
    
        var msg = new OscMessage("/test/", "⚡");
        await duplex.SendAsync(msg);
        var received = await duplex.ReceiveMessageAsync();
        Assert.Equal("/test/", received.Address);
        Assert.Equal("⚡", received.Arguments[0]);
    }
}