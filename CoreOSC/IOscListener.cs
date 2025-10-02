using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LucHeart.CoreOSC;

/// <summary>
/// Provides asynchronous operations for receiving OSC messages and bundles.
/// </summary>
public interface IOscListener
{
    /// <summary>
    /// Receives the next OSC message from the listening endpoint.
    /// </summary>
    /// <param name="ct">A token used to cancel the receive operation.</param>
    /// <returns>A task that resolves to the next <see cref="OscMessage"/>.</returns>
    public Task<OscMessage> ReceiveMessageAsync(CancellationToken ct);

    /// <summary>
    /// Receives the next OSC message together with the originating endpoint information.
    /// </summary>
    /// <param name="ct">A token used to cancel the receive operation.</param>
    /// <returns>A task that resolves to the received <see cref="OscMessage"/> and its sender endpoint.</returns>
    public Task<(OscMessage Message, IPEndPoint EndPoint)> ReceiveMessageExAsync(CancellationToken ct);

    /// <summary>
    /// Receives the next OSC bundle from the listening endpoint.
    /// </summary>
    /// <param name="ct">A token used to cancel the receive operation.</param>
    /// <returns>A task that resolves to the next <see cref="OscBundle"/>.</returns>
    public Task<OscBundle> ReceiveBundleAsync(CancellationToken ct);

    /// <summary>
    /// Receives the next OSC bundle together with the originating endpoint information.
    /// </summary>
    /// <param name="ct">A token used to cancel the receive operation.</param>
    /// <returns>A task that resolves to the received <see cref="OscBundle"/> and its sender endpoint.</returns>
    public Task<(OscBundle Bundle, IPEndPoint EndPoint)> ReceiveBundleExAsync(CancellationToken ct);
}
