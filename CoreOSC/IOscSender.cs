using System.Threading.Tasks;

namespace LucHeart.CoreOSC;

public interface IOscSender
{
    public Task SendAsync(byte[] message);
    public Task SendAsync(IOscPacket packet);
}