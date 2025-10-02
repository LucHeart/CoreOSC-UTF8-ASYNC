namespace LucHeart.CoreOSC;

public readonly struct Midi : IOscSerializable
{
    public readonly byte Port;
    public readonly byte Status;
    public readonly byte Data1;
    public readonly byte Data2;

    public Midi(byte port, byte status, byte data1, byte data2)
    {
        Port = port;
        Status = status;
        Data1 = data1;
        Data2 = data2;
    }

    public override bool Equals(object? obj) => obj switch
    {
        Midi midi => Port == midi.Port && Status == midi.Status && Data1 == midi.Data1 && Data2 == midi.Data2,
        byte[] byteArray => Port == byteArray[0] && Status == byteArray[1] && Data1 == byteArray[2] &&
                            Data2 == byteArray[3],
        _ => false
    };

    public static bool operator ==(Midi a, Midi b) => a.Equals(b);
    public static bool operator !=(Midi a, Midi b) => !a.Equals(b);
    public override int GetHashCode() => (Port << 24) + (Status << 16) + (Data1 << 8) + (Data2);


    /// <inheritdoc />
    public byte[] ToBytes()
    {
        var output = new byte[4];
        output[0] = Port;
        output[1] = Status;
        output[2] = Data1;
        output[3] = Data2;
        return output;
    }
}