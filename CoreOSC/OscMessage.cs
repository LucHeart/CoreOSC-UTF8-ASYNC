using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LucHeart.CoreOSC;

public class OscMessage : OscPacket
{
    public readonly string Address;
    public readonly object?[] Arguments;

    public OscMessage(string address, params object?[] args)
    {
        this.Address = address;
        Arguments = args;
    }

    public override byte[] GetBytes()
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
                    typeStringBuilder.Append("i");
                    parts.Add(SetInt(intValue));
                    break;

                case float floatValue:
                    if (float.IsPositiveInfinity(floatValue))
                    {
                        typeStringBuilder.Append("I");
                        break;
                    }
                    typeStringBuilder.Append("f");
                    parts.Add(setFloat(floatValue));
                    break;

                case string stringValue:
                    typeStringBuilder.Append("s");
                    parts.Add(setString(stringValue));
                    break;

                case byte[] byteArrayValue:
                    typeStringBuilder.Append("b");
                    parts.Add(setBlob(byteArrayValue));
                    break;

                case long longValue:
                    typeStringBuilder.Append("h");
                    parts.Add(setLong(longValue));
                    break;

                case ulong ulongValue:
                    typeStringBuilder.Append("t");
                    parts.Add(setULong(ulongValue));
                    break;

                case TimeTag timeTagValue:
                    typeStringBuilder.Append("t");
                    parts.Add(setULong(timeTagValue.Tag));
                    break;

                case double doubleValue:
                    if (double.IsPositiveInfinity(doubleValue))
                    {
                        typeStringBuilder.Append("I");
                        break;
                    }

                    typeStringBuilder.Append("d");
                    parts.Add(setDouble(doubleValue));
                    break;

                case Symbol symbolValue:
                    typeStringBuilder.Append("S");
                    parts.Add(symbolValue.ToBytes());
                    break;

                case char charValue:
                    typeStringBuilder.Append("c");
                    parts.Add(setChar(charValue));
                    break;

                case RGBA rgbaValue:
                    typeStringBuilder.Append("r");
                    parts.Add(rgbaValue.ToBytes());
                    break;

                case Midi midiValue:
                    typeStringBuilder.Append("m");
                    parts.Add(midiValue.ToBytes());
                    break;

                case bool boolValue:
                    typeStringBuilder.Append(boolValue ? "T" : "F");
                    break;

                case null:
                    typeStringBuilder.Append("N");
                    break;

                // This part handles arrays. It points currentList to the array and resets i
                // The array is processed like normal and when it is finished we replace
                // currentList back with Arguments and continue from where we left off
                case ICollection collectionValue:
                    var array = new object[collectionValue.Count];
                    collectionValue.CopyTo(array, 0);

                    if (Arguments != currentList)
                        throw new Exception("Nested Arrays are not supported");
                    typeStringBuilder.Append("[");
                    currentList = array;
                    argumentsIndex = i;
                    i = 0;
                    continue;

                default:
                    throw new Exception("Unable to transmit values of type " + arg.GetType().FullName);
            }

            i++;
            if (currentList != Arguments && i == currentList.Length)
            {
                // End of array, go back to main Argument list
                typeStringBuilder.Append("]");
                currentList = Arguments;
                i = argumentsIndex + 1;
            }
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
}