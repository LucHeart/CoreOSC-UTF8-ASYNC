using System.Net;
using System.Threading.Tasks;

namespace LucHeart.CoreOSC;

public class OscDuplex : OscListener, IOscSender
{
    private readonly IPEndPoint _remoteEndPoint;

    public OscDuplex(IPEndPoint listenerEndpoint, IPEndPoint remoteEndpoint) : base(listenerEndpoint)
    {
        _remoteEndPoint = remoteEndpoint;
    }

    public Task SendAsync(byte[] message) => UdpClient.SendAsync(message, message.Length, _remoteEndPoint);

    public Task SendAsync(IOscPacket packet) => SendAsync(packet.GetBytes());

    public Task SendAsync(IPEndPoint endPoint, byte[] message) => UdpClient.SendAsync(message, message.Length, endPoint);

    public Task SendAsync(IPEndPoint endPoint, IOscPacket packet) => SendAsync(endPoint, packet.GetBytes());
}