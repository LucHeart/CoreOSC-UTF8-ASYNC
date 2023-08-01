using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using OneOf;

namespace LucHeart.CoreOSC;

public class UdpListener : IDisposable
{
    internal readonly UdpClient UdpClient;

    public UdpListener(IPEndPoint listenerEndPoint)
    {
        UdpClient = new UdpClient(listenerEndPoint);
    }

    public async Task<OneOf<OscMessage, OscBundle>> ReceiveAsync()
    {
        var receiveResult = await UdpClient.ReceiveAsync();
        return OscPacket.GetPacket(receiveResult.Buffer);
    }
    
    public void Dispose()
    {
        UdpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}