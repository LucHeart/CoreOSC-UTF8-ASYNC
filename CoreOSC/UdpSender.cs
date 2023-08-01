using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LucHeart.CoreOSC;

public class UdpSender : IDisposable
{

    private readonly IPEndPoint _remoteIpEndPoint;
    private readonly Socket _sock;

    public UdpSender(IPEndPoint remoteIpEndPoint)
    {
        _remoteIpEndPoint = remoteIpEndPoint;
        _sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    public Task SendAsync(byte[] message) => _sock.SendToAsync(message, SocketFlags.None, _remoteIpEndPoint);

    public Task SendAsync(OscPacket packet) => SendAsync(packet.GetBytes());
    
    public void Dispose()
    {
        _sock.Dispose();
        GC.SuppressFinalize(this);
    }
}