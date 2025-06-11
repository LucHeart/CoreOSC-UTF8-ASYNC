using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace LucHeart.CoreOSC.Tests;

public class ListenerTest
{
    /// <summary>
    /// Opens a listener on a specified port, then closes it and attempts to open another on the same port
    /// Opening the second listener will fail unless the first one has been properly closed.
    /// </summary>
    [Test, Timeout(5000)]
    public async Task CloseListener(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using (new OscListener(endpoint))
        {
            // Logic would be within the lifetime of this listener
        } // Dispose happens here, compiler sugar :)
        using var l2 = new OscListener(endpoint);
    }

    /// <summary>
    /// Tries to open two listeners on the same port, results in an exception
    /// </summary>
    [Test, Timeout(5000)]
    public async Task CloseListenerException(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        Assert.Throws<SocketException>(() =>
        {
            using var l1 = new OscListener(endpoint);
            using var l2 = new OscListener(endpoint);
        });
    }
    
    /// <summary>
    /// Single message receive
    /// </summary>
    [Test, Timeout(5000)]
    public async Task ListenerSingleMsg(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var listener = new OscListener(endpoint);
        using var sender = new OscSender(endpoint);
    
        var msg = new OscMessage("/test/", 23.42f);
        await sender.SendAsync(msg);
        var received = await listener.ReceiveMessageAsync();
        await Assert.That(received.Address).IsEqualTo("/test/");
        await Assert.That(received.Arguments[0]).IsEqualTo(23.42f);
    }
    
    /// <summary>
    /// Bombard the listener with messages, check if they are all received
    /// </summary>
    [Test, Timeout(60_000)]
    public async Task ListenerLoadTest(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var listener = new OscListener(endpoint);
        using var sender = new OscSender(endpoint);
    
        var msg = new OscMessage("/test/", 23.42f);
    
        for (var i = 0; i < 100; i++)
            await sender.SendAsync(msg);
    
        for (var i = 0; i < 100; i++)
        {
            var receivedMessage = await listener.ReceiveMessageAsync();
            await Assert.That(receivedMessage.Address).IsEqualTo("/test/");
            await Assert.That(receivedMessage.Arguments[0]).IsEqualTo(23.42f);
        }
    }
    
    /// <summary>
    /// Single message receive with utf8 content
    /// </summary>
    [Test, Timeout(5000)]
    public async Task ListenerUtf8(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var listener = new OscListener(endpoint);
        using var sender = new OscSender(endpoint);
    
        var msg = new OscMessage("/test/", "⚡");
        await sender.SendAsync(msg);
        var received = await listener.ReceiveMessageAsync();
        await Assert.That(received.Address).IsEqualTo("/test/");
        await Assert.That(received.Arguments[0]).IsEqualTo("⚡");
    }
    
    /// <summary>
    /// Single message receive with utf8 content
    /// </summary>
    [Test, Timeout(5000)]
    public async Task ListenerUtf8_2(CancellationToken ct)
    {
        var endpoint = TestUtils.GetNextEndpoint();
        using var listener = new OscListener(endpoint);
        using var sender = new OscSender(endpoint);
    
        var msg = new OscMessage("/test/", "There is a thunderstorm ⚡ brewing in the ☁");
        await sender.SendAsync(msg);
        var received = await listener.ReceiveMessageAsync();
        await Assert.That(received.Address).IsEqualTo("/test/");
        await Assert.That(received.Arguments[0]).IsEqualTo("There is a thunderstorm ⚡ brewing in the ☁");
    }
}