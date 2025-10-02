using System.Net;
using System.Threading.Tasks;

namespace LucHeart.CoreOSC;

/// <summary>
/// Provides asynchronous operations for sending OSC packets to remote endpoints.
/// </summary>
public interface IOscSender
{
    /// <summary>
    /// Sends a raw OSC message payload to the configured default endpoint.
    /// </summary>
    /// <param name="message">The binary OSC message payload to send.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public Task SendAsync(byte[] message);

    /// <summary>
    /// Serializes and sends an OSC packet to the configured default endpoint.
    /// </summary>
    /// <param name="packet">The OSC packet to serialize and send.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public Task SendAsync(IOscPacket packet);

    /// <summary>
    /// Sends a raw OSC message payload to a specific endpoint.
    /// </summary>
    /// <param name="endPoint">The destination endpoint that should receive the message.</param>
    /// <param name="message">The binary OSC message payload to send.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public Task SendAsync(IPEndPoint endPoint, byte[] message);

    /// <summary>
    /// Serializes and sends an OSC packet to a specific endpoint.
    /// </summary>
    /// <param name="endPoint">The destination endpoint that should receive the packet.</param>
    /// <param name="packet">The OSC packet to serialize and send.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public Task SendAsync(IPEndPoint endPoint, IOscPacket packet);
}