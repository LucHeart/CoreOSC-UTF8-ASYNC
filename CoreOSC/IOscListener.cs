using System.Threading.Tasks;

namespace LucHeart.CoreOSC;

public interface IOscListener
{
    public Task<OscMessage> ReceiveMessageAsync();
    public Task<OscBundle> ReceiveBundleAsync();
}