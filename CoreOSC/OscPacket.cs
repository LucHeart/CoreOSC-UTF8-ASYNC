using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneOf;

namespace LucHeart.CoreOSC;

public abstract class OscPacket
{
    public static OneOf<OscMessage, OscBundle> GetPacket(byte[] oscData)
    {
        if (oscData[0] == '#')
            return ParseBundle(oscData);
        return ParseMessage(oscData);
    }

    public abstract byte[] GetBytes();

    #region Parse OSC packages

    /// <summary>
    /// Takes in an OSC bundle package in byte form and parses it into a more usable OscBundle object
    /// </summary>
    /// <param name="msg"></param>
    /// <returns>Message containing various arguments and an address</returns>
    private static OscMessage ParseMessage(Span<byte> msg)
    {
        ReadOnlySpan<byte> msgReadOnlySpan = msg;

        var arguments = new List<object?>();
        var mainArray = arguments; // used as a reference when we are parsing arrays to get the main array back

        // Get address
        var address = GetAddress(msgReadOnlySpan, out var index);

        if (index % 4 != 0)
            throw new Exception(
                "Misaligned OSC Packet data. Address string is not padded correctly and does not align to 4 byte interval");

        // Get type tags
        var types = GetTypes(msgReadOnlySpan, index);
        index += types.Length;

        while (index % 4 != 0)
            index++;

        var commaParsed = false;

        foreach (var type in types)
        {
            // skip leading comma
            if (type == DividerChar && !commaParsed)
            {
                commaParsed = true;
                continue;
            }

            switch (type)
            {
                case '\0':
                    break;

                case 'i':
                    var intVal = GetInt(msgReadOnlySpan, index);
                    arguments.Add(intVal);
                    index += 4;
                    break;

                case 'f':
                    var floatVal = GetFloat(msg, index);
                    arguments.Add(floatVal);
                    index += 4;
                    break;

                case 's':
                    var stringVal = GetString(msgReadOnlySpan, index);
                    arguments.Add(stringVal);
                    index += Encoding.UTF8.GetBytes(stringVal).Length;
                    break;

                case 'b':
                    var blob = GetBlob(msgReadOnlySpan, index);
                    arguments.Add(blob.ToArray());
                    index += 4 + blob.Length;
                    break;

                case 'h':
                    var hval = GetLong(msgReadOnlySpan, index);
                    arguments.Add(hval);
                    index += 8;
                    break;

                case 't':
                    var sval = GetULong(msgReadOnlySpan, index);
                    arguments.Add(new TimeTag(sval));
                    index += 8;
                    break;

                case 'd':
                    var dval = GetDouble(msg, index);
                    arguments.Add(dval);
                    index += 8;
                    break;

                case 'S':
                    var symbolVal = GetString(msgReadOnlySpan, index);
                    arguments.Add(new Symbol(symbolVal));
                    index += symbolVal.Length;
                    break;

                case 'c':
                    var cval = GetChar(msg, index);
                    arguments.Add(cval);
                    index += 4;
                    break;

                case 'r':
                    var rgbaval = GetRgba(msg, index);
                    arguments.Add(rgbaval);
                    index += 4;
                    break;

                case 'm':
                    var midival = GetMidi(msg, index);
                    arguments.Add(midival);
                    index += 4;
                    break;

                case 'T':
                    arguments.Add(true);
                    break;

                case 'F':
                    arguments.Add(false);
                    break;

                case 'N':
                    arguments.Add(null);
                    break;

                case 'I':
                    arguments.Add(double.PositiveInfinity);
                    break;

                case '[':
                    if (arguments != mainArray)
                        throw new Exception("CoreOSC does not support nested arrays");
                    arguments = new List<object?>(); // make arguments point to a new object array
                    break;

                case ']':
                    mainArray.Add(arguments); // add the array to the main array
                    arguments = mainArray; // make arguments point back to the main array
                    break;

                default:
                    throw new Exception("OSC type tag '" + type + "' is unknown.");
            }

            while (index % 4 != 0)
                index++;
        }

        return new OscMessage(address, arguments.ToArray());
    }

    /// <summary>
    /// Takes in an OSC bundle package in byte form and parses it into a more usable OscBundle object
    /// </summary>
    /// <param name="msg"></param>
    /// <returns>Bundle containing elements and a timetag</returns>
    private static OscBundle ParseBundle(Span<byte> msg)
    {
        ReadOnlySpan<byte> msgReadOnly = msg;
        var messages = new List<OscMessage>();

        var index = 0;

        var bundleTag = Encoding.ASCII.GetString(msgReadOnly[..8]);
        index += 8;

        var timeTag = GetULong(msgReadOnly, index);
        index += 8;

        if (bundleTag != "#bundle\0")
            throw new Exception("Not a bundle");

        while (index < msgReadOnly.Length)
        {
            var size = GetInt(msgReadOnly, index);
            index += 4;

            var messageBytes = msg.Slice(index, size);
            var message = ParseMessage(messageBytes);
            messages.Add(message);

            index += size;
            while (index % 4 != 0)
                index++;
        }

        var output = new OscBundle(timeTag, messages.ToArray());
        return output;
    }

    #endregion Parse OSC packages

    #region Get arguments from byte array

    private const byte DividerChar = 44;

    private static string GetAddress(ReadOnlySpan<byte> msg, out int index)
    {
        index = msg.IndexOf(DividerChar);
        if (index == -1) throw new Exception("Could not get address from data");
        var address = Encoding.ASCII.GetString(msg[..index]);
        return address.Replace("\0", "");
    }

    private static ReadOnlySpan<char> GetTypes(ReadOnlySpan<byte> msg, int index)
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

    private static int GetInt(ReadOnlySpan<byte> msg, int index) =>
        BinaryPrimitives.ReadInt32BigEndian(msg.Slice(index, 4));

    private static float GetFloat(Span<byte> msg, int index) => BitConverter.ToSingle(msg.ReverseSlice(index, 4));

    private static string GetString(ReadOnlySpan<byte> msg, int index)
    {
        var i = index + 4;
        for (; i - 1 < msg.Length; i += 4)
        {
            if (msg[i - 1] != 0) continue;
            return Encoding.UTF8.GetString(msg[index..i]).Replace("\0", "");
        }

        throw new Exception("No null terminator after type string");
    }

    private static ReadOnlySpan<byte> GetBlob(ReadOnlySpan<byte> msg, int index)
    {
        var size = GetInt(msg, index);
        return msg.Slice(index + 4, size);
    }

    private static ulong GetULong(ReadOnlySpan<byte> msg, int index) =>
        BinaryPrimitives.ReadUInt64BigEndian(msg.Slice(index, 8));

    private static long GetLong(ReadOnlySpan<byte> msg, int index) =>
        BinaryPrimitives.ReadInt64BigEndian(msg.Slice(index, 8));

    private static double GetDouble(Span<byte> msg, int index) => BitConverter.ToDouble(msg.ReverseSlice(index, 8));
    private static char GetChar(ReadOnlySpan<byte> msg, int index) => (char)msg[index + 3];

    private static RGBA GetRgba(ReadOnlySpan<byte> msg, int index) =>
        new(msg[index], msg[index + 1], msg[index + 2], msg[index + 3]);

    private static Midi GetMidi(ReadOnlySpan<byte> msg, int index) =>
        new(msg[index], msg[index + 1], msg[index + 2], msg[index + 3]);

    #endregion Get arguments from byte array

    #region Create byte arrays for arguments

    protected static byte[] SetInt(int value)
    {
        var output = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(output, value);
        return output;
    }

    protected static byte[] SetFloat(float value)
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

    protected static byte[] SetString(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var msg = new byte[(bytes.Length / 4 + 1) * 4];
        bytes.CopyTo(msg, 0);
        return msg;
    }

    protected static byte[] SetBlob(byte[] value)
    {
        var len = value.Length + 4;
        len += 4 - len % 4;

        var msg = new byte[len];
        BinaryPrimitives.WriteInt32BigEndian(msg, value.Length);
        value.CopyTo(msg, 4);
        return msg;
    }

    protected static byte[] SetLong(long value)
    {
        var output = new byte[8];
        BinaryPrimitives.WriteInt64BigEndian(output, value);
        return output;
    }

    protected static byte[] SetULong(ulong value)
    {
        var output = new byte[8];
        BinaryPrimitives.WriteUInt64BigEndian(output, value);
        return output;
    }

    protected static byte[] SetDouble(double value)
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

    protected static byte[] SetChar(char value)
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