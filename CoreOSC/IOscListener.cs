using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LucHeart.CoreOSC;

public interface IOscListener
{
    public Task<OscMessage> ReceiveMessageAsync();

    public Task<(OscMessage Message, IPEndPoint EndPoint)> ReceiveMessageExAsync();

    public Task<OscBundle> ReceiveBundleAsync();

    public Task<(OscBundle Bundle, IPEndPoint EndPoint)> ReceiveBundleExAsync();

#if !NETSTANDARD
    public Task<OscMessage> ReceiveMessageAsync(CancellationToken ct);

    public Task<(OscMessage Message, IPEndPoint EndPoint)> ReceiveMessageExAsync(CancellationToken ct);

    public Task<OscBundle> ReceiveBundleAsync(CancellationToken ct);

    public Task<(OscBundle Bundle, IPEndPoint EndPoint)> ReceiveBundleExAsync(CancellationToken ct);
#endif
}
