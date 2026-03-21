using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LucHeart.CoreOSC;

public sealed class OscMessage : IOscPacket
{
    public readonly string Address;
    public readonly object?[] Arguments;

    public OscMessage(string address, params object?[] args)
    {
        Address = address;
        Arguments = args;
    }

    /// <inheritdoc />
    public byte[] GetBytes()
    {
        // === Pass 1: Build type string and calculate total data size ===
        var currentList = Arguments;
        var argumentsIndex = 0;

        var typeStringBuilder = new StringBuilder(",");
        var dataSize = 0;
        var i = 0;
        while (i < currentList.Length)
        {
            var arg = currentList[i];
            switch (arg)
            {
                case int:
                    typeStringBuilder.Append('i');
                    dataSize += 4;
                    break;

                case float floatValue:
                    if (float.IsPositiveInfinity(floatValue))
                    {
                        typeStringBuilder.Append('I');
                        break;
                    }
                    typeStringBuilder.Append('f');
                    dataSize += 4;
                    break;

                case string stringValue:
                    typeStringBuilder.Append('s');
                    dataSize += OscPacketUtils.GetStringByteSize(stringValue);
                    break;

                case byte[] byteArrayValue:
                    typeStringBuilder.Append('b');
                    dataSize += OscPacketUtils.GetBlobByteSize(byteArrayValue);
                    break;

                case long:
                    typeStringBuilder.Append('h');
                    dataSize += 8;
                    break;

                case ulong:
                    typeStringBuilder.Append('t');
                    dataSize += 8;
                    break;

                case TimeTag:
                    typeStringBuilder.Append('t');
                    dataSize += 8;
                    break;

                case double doubleValue:
                    if (double.IsPositiveInfinity(doubleValue))
                    {
                        typeStringBuilder.Append('I');
                        break;
                    }
                    typeStringBuilder.Append('d');
                    dataSize += 8;
                    break;

                case Symbol symbolValue:
                    typeStringBuilder.Append('S');
                    dataSize += symbolValue.GetByteSize();
                    break;

                case char:
                    typeStringBuilder.Append('c');
                    dataSize += 4;
                    break;

                case RGBA:
                    typeStringBuilder.Append('r');
                    dataSize += 4;
                    break;

                case Midi:
                    typeStringBuilder.Append('m');
                    dataSize += 4;
                    break;

                case bool boolValue:
                    typeStringBuilder.Append(boolValue ? 'T' : 'F');
                    break;

                case null:
                    typeStringBuilder.Append('N');
                    break;

                case ICollection collectionValue:
                    var array = new object[collectionValue.Count];
                    collectionValue.CopyTo(array, 0);

                    if (Arguments != currentList)
                        throw new Exception("Nested Arrays are not supported");
                    typeStringBuilder.Append('[');
                    currentList = array;
                    argumentsIndex = i;
                    i = 0;
                    continue;

                default:
                    throw new Exception("Unable to transmit values of type " + arg.GetType().FullName);
            }

            i++;
            if (currentList == Arguments || i != currentList.Length) continue;
            typeStringBuilder.Append(']');
            currentList = Arguments;
            i = argumentsIndex + 1;
        }

        // === Allocate single output buffer ===
        var addressLen = string.IsNullOrEmpty(Address) ? 0 : Utils.AlignedStringLength(Address);
        var typeString = typeStringBuilder.ToString();
        var typeLen = Utils.AlignedStringLength(typeString);
        var total = addressLen + typeLen + dataSize;

        var output = new byte[total];
        var span = output.AsSpan();
        var offset = 0;

        // Write address
        Encoding.ASCII.GetBytes(Address.AsSpan(), span[offset..]);
        offset += addressLen;

        // Write type string
        Encoding.ASCII.GetBytes(typeString.AsSpan(), span[offset..]);
        offset += typeLen;

        // === Pass 2: Write argument data directly into buffer ===
        currentList = Arguments;
        argumentsIndex = 0;
        i = 0;
        while (i < currentList.Length)
        {
            var arg = currentList[i];
            switch (arg)
            {
                case int intValue:
                    OscPacketUtils.WriteInt(span[offset..], intValue);
                    offset += 4;
                    break;

                case float floatValue:
                    if (!float.IsPositiveInfinity(floatValue))
                    {
                        OscPacketUtils.WriteFloat(span[offset..], floatValue);
                        offset += 4;
                    }
                    break;

                case string stringValue:
                    offset += OscPacketUtils.WriteString(span[offset..], stringValue);
                    break;

                case byte[] byteArrayValue:
                    offset += OscPacketUtils.WriteBlob(span[offset..], byteArrayValue);
                    break;

                case long longValue:
                    OscPacketUtils.WriteLong(span[offset..], longValue);
                    offset += 8;
                    break;

                case ulong ulongValue:
                    OscPacketUtils.WriteULong(span[offset..], ulongValue);
                    offset += 8;
                    break;

                case TimeTag timeTagValue:
                    OscPacketUtils.WriteULong(span[offset..], timeTagValue.Tag);
                    offset += 8;
                    break;

                case double doubleValue:
                    if (!double.IsPositiveInfinity(doubleValue))
                    {
                        OscPacketUtils.WriteDouble(span[offset..], doubleValue);
                        offset += 8;
                    }
                    break;

                case Symbol symbolValue:
                    offset += symbolValue.WriteTo(span[offset..]);
                    break;

                case char charValue:
                    OscPacketUtils.WriteChar(span[offset..], charValue);
                    offset += 4;
                    break;

                case RGBA rgbaValue:
                    rgbaValue.WriteTo(span[offset..]);
                    offset += 4;
                    break;

                case Midi midiValue:
                    midiValue.WriteTo(span[offset..]);
                    offset += 4;
                    break;

                case bool:
                case null:
                    break;

                case ICollection collectionValue:
                    var array = new object[collectionValue.Count];
                    collectionValue.CopyTo(array, 0);
                    if (Arguments != currentList)
                        throw new Exception("Nested Arrays are not supported");
                    currentList = array;
                    argumentsIndex = i;
                    i = 0;
                    continue;

                default:
                    throw new Exception("Unable to transmit values of type " + arg.GetType().FullName);
            }

            i++;
            if (currentList == Arguments || i != currentList.Length) continue;
            currentList = Arguments;
            i = argumentsIndex + 1;
        }

        return output;
    }
    
    /// <summary>
    /// Takes in an OSC bundle package in byte form and parses it into a more usable OscBundle object
    /// </summary>
    /// <param name="msg"></param>
    /// <returns>Message containing various arguments and an address</returns>
    public static OscMessage ParseMessage(Span<byte> msg)
    {
        ReadOnlySpan<byte> msgReadOnlySpan = msg;

        var arguments = new List<object?>();
        var mainArray = arguments; // used as a reference when we are parsing arrays to get the main array back

        // Get address
        var address = OscPacketUtils.GetAddress(msgReadOnlySpan, out var index);
        
        if (index % 4 != 0)
            throw new Exception(
                "Misaligned OSC Packet data. Address string is not padded correctly and does not align to 4 byte interval");

        // Get type tags
        var types = OscPacketUtils.GetTypes(msgReadOnlySpan, index);
        index += types.Length;

        while (index % 4 != 0)
            index++;

        var commaParsed = false;

        foreach (var type in types)
        {
            // skip leading comma
            if (type == OscPacketUtils.DividerChar && !commaParsed)
            {
                commaParsed = true;
                continue;
            }

            switch (type)
            {
                case '\0':
                    break;

                case 'i':
                    var intVal = OscPacketUtils.GetInt(msgReadOnlySpan, index);
                    arguments.Add(intVal);
                    index += 4;
                    break;

                case 'f':
                    var floatVal = OscPacketUtils.GetFloat(msg, index);
                    arguments.Add(floatVal);
                    index += 4;
                    break;

                case 's':
                    var stringVal = OscPacketUtils.GetString(msgReadOnlySpan, index, out index);
                    arguments.Add(stringVal);
                    break;

                case 'b':
                    var blob = OscPacketUtils.GetBlob(msgReadOnlySpan, index);
                    arguments.Add(blob.ToArray());
                    index += 4 + blob.Length;
                    break;

                case 'h':
                    var hval = OscPacketUtils.GetLong(msgReadOnlySpan, index);
                    arguments.Add(hval);
                    index += 8;
                    break;

                case 't':
                    var sval = OscPacketUtils.GetULong(msgReadOnlySpan, index);
                    arguments.Add(new TimeTag(sval));
                    index += 8;
                    break;

                case 'd':
                    var dval = OscPacketUtils.GetDouble(msg, index);
                    arguments.Add(dval);
                    index += 8;
                    break;

                case 'S':
                    var symbolVal = OscPacketUtils.GetString(msgReadOnlySpan, index, out index);
                    arguments.Add(new Symbol(symbolVal));
                    break;

                case 'c':
                    var cval = OscPacketUtils.GetChar(msg, index);
                    arguments.Add(cval);
                    index += 4;
                    break;

                case 'r':
                    var rgbaval = OscPacketUtils.GetRgba(msg, index);
                    arguments.Add(rgbaval);
                    index += 4;
                    break;

                case 'm':
                    var midival = OscPacketUtils.GetMidi(msg, index);
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
}