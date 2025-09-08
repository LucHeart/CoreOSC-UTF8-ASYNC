using System;
using System.Buffers.Binary;
using System.Text;

namespace LucHeart.CoreOSC;

public static class OscPacketUtils
{
    #region Get arguments from byte array

    public const byte DividerChar = 44;

    public static string GetAddress(ReadOnlySpan<byte> msg, out int index)
    {
        index = msg.IndexOf(DividerChar);
        if (index == -1) throw new Exception("Could not get address from data");
        var address = Encoding.ASCII.GetString(msg[..index]);
        return address.Replace("\0", "");
    }

    public static ReadOnlySpan<char> GetTypes(ReadOnlySpan<byte> msg, int index)
    {
        var i = index + 4;

        for (; i <= msg.Length; i += 4)
        {
            if (msg[i - 1] != 0) continue;
            var charSet = msg[index..i];
            var newSpan = new char[Encoding.ASCII.GetMaxCharCount(charSet.Length)].AsSpan();
            Encoding.ASCII.GetChars(charSet, newSpan);
            return newSpan;
        }

        throw new Exception("No null terminator after type string");
    }

    public static int GetInt(ReadOnlySpan<byte> msg, int index) =>
        BinaryPrimitives.ReadInt32BigEndian(msg.Slice(index, 4));

    public static float GetFloat(Span<byte> msg, int index) => BitConverter.ToSingle(msg.ReverseSlice(index, 4));

    public static string GetString(ReadOnlySpan<byte> msg, int index, out int newIndex)
    {
        var i = index + 4;
        for (; i - 1 < msg.Length; i += 4)
        {
            if (msg[i - 1] != 0) continue;
            newIndex = i;
            return Encoding.UTF8.GetString(msg[index..i]).Replace("\0", "");
        }

        throw new Exception("No null terminator after type string");
    }

    public static ReadOnlySpan<byte> GetBlob(ReadOnlySpan<byte> msg, int index)
    {
        var size = GetInt(msg, index);
        return msg.Slice(index + 4, size);
    }

    public static ulong GetULong(ReadOnlySpan<byte> msg, int index) =>
        BinaryPrimitives.ReadUInt64BigEndian(msg.Slice(index, 8));

    public static long GetLong(ReadOnlySpan<byte> msg, int index) =>
        BinaryPrimitives.ReadInt64BigEndian(msg.Slice(index, 8));

    public static double GetDouble(Span<byte> msg, int index) => BitConverter.ToDouble(msg.ReverseSlice(index, 8));
    public static char GetChar(ReadOnlySpan<byte> msg, int index) => (char)msg[index + 3];

    public static RGBA GetRgba(ReadOnlySpan<byte> msg, int index) =>
        new(msg[index], msg[index + 1], msg[index + 2], msg[index + 3]);

    public static Midi GetMidi(ReadOnlySpan<byte> msg, int index) =>
        new(msg[index], msg[index + 1], msg[index + 2], msg[index + 3]);

    #endregion Get arguments from byte array

    #region Create byte arrays for arguments

    public static byte[] SetInt(int value)
    {
        var output = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(output, value);
        return output;
    }

    public static byte[] SetFloat(float value)
    {
        // ReSharper disable once RedundantAssignment
        var output = new byte[4];
#if NETSTANDARD2_1
        var rev = BitConverter.GetBytes(value);
        output[0] = rev[3];
        output[1] = rev[2];
        output[2] = rev[1];
        output[3] = rev[0];
# else
        BinaryPrimitives.WriteSingleBigEndian(output, value);
#endif
        return output;
    }

    public static byte[] SetString(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var msg = new byte[(bytes.Length / 4 + 1) * 4];
        bytes.CopyTo(msg, 0);
        return msg;
    }

    public static byte[] SetBlob(byte[] value)
    {
        var len = value.Length + 4;
        len += 4 - len % 4;

        var msg = new byte[len];
        BinaryPrimitives.WriteInt32BigEndian(msg, value.Length);
        value.CopyTo(msg, 4);
        return msg;
    }

    public static byte[] SetLong(long value)
    {
        var output = new byte[8];
        BinaryPrimitives.WriteInt64BigEndian(output, value);
        return output;
    }

    public static byte[] SetULong(ulong value)
    {
        var output = new byte[8];
        BinaryPrimitives.WriteUInt64BigEndian(output, value);
        return output;
    }

    public static byte[] SetDouble(double value)
    {
        var output = new byte[8];
#if NETSTANDARD2_1
        var rev = BitConverter.GetBytes(value);
        output[0] = rev[7];
        output[1] = rev[6];
        output[2] = rev[5];
        output[3] = rev[4];
        output[4] = rev[3];
        output[5] = rev[2];
        output[6] = rev[1];
        output[7] = rev[0];
#else
        BinaryPrimitives.WriteDoubleBigEndian(output, value);
#endif
        return output;
    }

    public static byte[] SetChar(char value)
    {
        var output = new byte[4];
        output[0] = 0;
        output[1] = 0;
        output[2] = 0;
        output[3] = (byte)value;
        return output;
    }

    #endregion Create byte arrays for arguments
}