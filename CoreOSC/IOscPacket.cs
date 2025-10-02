namespace LucHeart.CoreOSC;

/// <summary>
/// Represents an Open Sound Control packet that can be serialized for transmission.
/// </summary>
public interface IOscPacket
{
    /// <summary>
    /// Converts the packet into its OSC binary representation.
    /// </summary>
    /// <returns>The serialized OSC byte sequence.</returns>
    public byte[] GetBytes();
}