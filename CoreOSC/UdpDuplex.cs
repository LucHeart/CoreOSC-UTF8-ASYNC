using System.Net;
using System.Threading.Tasks;

namespace LucHeart.CoreOSC;

public class UdpDuplex : UdpListener
{
    private readonly IPEndPoint _remoteEndPoint;

    public UdpDuplex(IPEndPoint listenerEndpoint, IPEndPoint remoteEndpoint) : base(listenerEndpoint)
    {
        _remoteEndPoint = remoteEndpoint;
    }
    
    public Task SendAsync(byte[] message) => UdpClient.SendAsync(message, message.Length, _remoteEndPoint);
    public Task SendAsync(OscPacket packet) => SendAsync(packet.GetBytes());
    
}