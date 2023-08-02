using System.Text;

namespace LucHeart.CoreOSC;

public readonly struct Symbol : IOscSerializable
{
    public readonly string Value;

    public Symbol(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
    public byte[] ToBytes()
    {
        var bytes = Encoding.UTF8.GetBytes(Value);

        var msg = new byte[(bytes.Length / 4 + 1) * 4];
        bytes.CopyTo(msg, 0);

        return msg;
    }

    public override bool Equals(object obj) => obj switch
    {
        Symbol symbol => Value == symbol.Value,
        string stringObj => Value == stringObj,
        _ => false
    };

    public static bool operator ==(Symbol a, Symbol b) => a.Equals(b);

    public static bool operator !=(Symbol a, Symbol b) => !a.Equals(b);

    public override int GetHashCode() => Value.GetHashCode();
    
}