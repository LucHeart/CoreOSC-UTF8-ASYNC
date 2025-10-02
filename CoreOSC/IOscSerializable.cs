namespace LucHeart.CoreOSC;

/// <summary>
/// Represents a value that can be serialized into the OSC binary wire format.
/// </summary>
public interface IOscSerializable
{
    /// <summary>
    /// Converts the current value into its OSC binary representation.
    /// </summary>
    /// <returns>The serialized OSC byte sequence.</returns>
    public byte[] ToBytes();
}