using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public byte[] GetBytes()
    {
        var parts = new List<byte[]>();

        var currentList = Arguments;
        var argumentsIndex = 0;

        var typeStringBuilder = new StringBuilder(",");
        var i = 0;
        while (i < currentList.Length)
        {
            var arg = currentList[i];
            switch (arg)
            {
                case int intValue:
                    typeStringBuilder.Append('i');
                    parts.Add(OscPacketUtils.SetInt(intValue));
                    break;

                case float floatValue:
                    if (float.IsPositiveInfinity(floatValue))
                    {
                        typeStringBuilder.Append('I');
                        break;
                    }
                    typeStringBuilder.Append('f');
                    parts.Add(OscPacketUtils.SetFloat(floatValue));
                    break;

                case string stringValue:
                    typeStringBuilder.Append('s');
                    parts.Add(OscPacketUtils.SetString(stringValue));
                    break;

                case byte[] byteArrayValue:
                    typeStringBuilder.Append('b');
                    parts.Add(OscPacketUtils.SetBlob(byteArrayValue));
                    break;

                case long longValue:
                    typeStringBuilder.Append('h');
                    parts.Add(OscPacketUtils.SetLong(longValue));
                    break;

                case ulong ulongValue:
                    typeStringBuilder.Append('t');
                    parts.Add(OscPacketUtils.SetULong(ulongValue));
                    break;

                case TimeTag timeTagValue:
                    typeStringBuilder.Append('t');
                    parts.Add(OscPacketUtils.SetULong(timeTagValue.Tag));
                    break;

                case double doubleValue:
                    if (double.IsPositiveInfinity(doubleValue))
                    {
                        typeStringBuilder.Append('I');
                        break;
                    }

                    typeStringBuilder.Append('d');
                    parts.Add(OscPacketUtils.SetDouble(doubleValue));
                    break;

                case Symbol symbolValue:
                    typeStringBuilder.Append('S');
                    parts.Add(symbolValue.ToBytes());
                    break;

                case char charValue:
                    typeStringBuilder.Append('c');
                    parts.Add(OscPacketUtils.SetChar(charValue));
                    break;

                case RGBA rgbaValue:
                    typeStringBuilder.Append('r');
                    parts.Add(rgbaValue.ToBytes());
                    break;

                case Midi midiValue:
                    typeStringBuilder.Append('m');
                    parts.Add(midiValue.ToBytes());
                    break;

                case bool boolValue:
                    typeStringBuilder.Append(boolValue ? "T" : "F");
                    break;

                case null:
                    typeStringBuilder.Append('N');
                    break;

                // This part handles arrays. It points currentList to the array and resets i
                // The array is processed like normal and when it is finished we replace
                // currentList back with Arguments and continue from where we left off
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
            
            // End of array, go back to main Argument list
            typeStringBuilder.Append(']');
            currentList = Arguments;
            i = argumentsIndex + 1;
        }

        var addressLen = string.IsNullOrEmpty(Address) ? 0 : Utils.AlignedStringLength(Address);
        var typeString = typeStringBuilder.ToString();
        var typeLen = Utils.AlignedStringLength(typeString);

        var total = addressLen + typeLen + parts.Sum(x => x.Length);
        
        var output = new byte[total];
        i = 0;

        Encoding.ASCII.GetBytes(Address).CopyTo(output, i);
        i += addressLen;
        
        Encoding.ASCII.GetBytes(typeString).CopyTo(output, i);
        i += typeLen;

        foreach (var part in parts)
        {
            part.CopyTo(output, i);
            i += part.Length;
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