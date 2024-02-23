using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LucHeart.CoreOSC;

public class OscSender : IDisposable, IOscSender
{

    private readonly IPEndPoint _remoteIpEndPoint;
    private readonly Socket _sock;

    public OscSender(IPEndPoint remoteIpEndPoint)
    {
        _remoteIpEndPoint = remoteIpEndPoint;
        _sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    public Task SendAsync(byte[] message) => _sock.SendToAsync(message, SocketFlags.None, _remoteIpEndPoint);

    public Task SendAsync(IOscPacket packet) => SendAsync(packet.GetBytes());

    public Task SendAsync(IPEndPoint endPoint, byte[] message) => _sock.SendToAsync(message, SocketFlags.None, endPoint);

    public Task SendAsync(IPEndPoint endPoint, IOscPacket packet) => SendAsync(endPoint, packet.GetBytes());

    public void Dispose()
    {
        _sock.Dispose();
        GC.SuppressFinalize(this);
    }
}