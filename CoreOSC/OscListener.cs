using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LucHeart.CoreOSC;

public class OscListener : IDisposable, IOscListener
{
    internal readonly UdpClient UdpClient;

    public OscListener(IPEndPoint listenerEndPoint)
    {
        UdpClient = new UdpClient(listenerEndPoint);
    }

    public async Task<OscMessage> ReceiveMessageAsync()
    {
        var receiveResult = await UdpClient.ReceiveAsync();
        return OscMessage.ParseMessage(receiveResult.Buffer);
    }

    public async Task<(OscMessage Message, IPEndPoint EndPoint)> ReceiveMessageExAsync()
    {
        var receiveResult = await UdpClient.ReceiveAsync();
        return (OscMessage.ParseMessage(receiveResult.Buffer), receiveResult.RemoteEndPoint);
    }

    public async Task<OscBundle> ReceiveBundleAsync()
    {
        var receiveResult = await UdpClient.ReceiveAsync();
        return OscBundle.ParseBundle(receiveResult.Buffer);
    }

    public async Task<(OscBundle Bundle, IPEndPoint EndPoint)> ReceiveBundleExAsync()
    {
        var receiveResult = await UdpClient.ReceiveAsync();
        return (OscBundle.ParseBundle(receiveResult.Buffer), receiveResult.RemoteEndPoint);
    }

    public void Dispose()
    {
        UdpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}