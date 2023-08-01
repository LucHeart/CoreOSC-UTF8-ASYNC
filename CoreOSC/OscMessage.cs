using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LucHeart.CoreOSC;

public class OscMessage : OscPacket
{
    public readonly string Address;
    public readonly object[] Arguments;

    public OscMessage(string address, params object[] args)
    {
        this.Address = address;
        Arguments = args;
    }

    public override byte[] GetBytes()
    {
        var parts = new List<byte[]>();

        var currentList = Arguments;
        int ArgumentsIndex = 0;

        var typeString = ",";
        var i = 0;
        while (i < currentList.Length)
        {
            var arg = currentList[i];
            switch (arg)
            {
                case int intValue:
                    typeString += "i";
                    parts.Add(setInt(intValue));
                    break;

                case float floatValue:
                    if (float.IsPositiveInfinity(floatValue))
                    {
                        typeString += "I";
                        break;
                    }
                    typeString += "f";
                    parts.Add(setFloat(floatValue));
                    break;

                case string stringValue:
                    typeString += "s";
                    parts.Add(setString(stringValue));
                    break;

                case byte[] byteArrayValue:
                    typeString += "b";
                    parts.Add(setBlob(byteArrayValue));
                    break;

                case long longValue:
                    typeString += "h";
                    parts.Add(setLong(longValue));
                    break;

                case ulong ulongValue:
                    typeString += "t";
                    parts.Add(setULong(ulongValue));
                    break;

                case Timetag timeTagValue:
                    typeString += "t";
                    parts.Add(setULong(timeTagValue.Tag));
                    break;

                case double doubleValue:
                    if (double.IsPositiveInfinity(doubleValue))
                    {
                        typeString += "I";
                        break;
                    }

                    typeString += "d";
                    parts.Add(setDouble(doubleValue));
                    break;

                case Symbol symbolValue:
                    typeString += "S";
                    parts.Add(setString(symbolValue.Value));
                    break;

                case char charValue:
                    typeString += "c";
                    parts.Add(setChar(charValue));
                    break;

                case RGBA rgbaValue:
                    typeString += "r";
                    parts.Add(setRGBA(rgbaValue));
                    break;

                case Midi midiValue:
                    typeString += "m";
                    parts.Add(setMidi(midiValue));
                    break;

                case bool boolValue:
                    typeString += boolValue ? "T" : "F";
                    break;

                case null:
                    typeString += "N";
                    break;

                // This part handles arrays. It points currentList to the array and resets i
                // The array is processed like normal and when it is finished we replace
                // currentList back with Arguments and continue from where we left off
                case ICollection collectionValue:
                    var array = new object[collectionValue.Count];
                    collectionValue.CopyTo(array, 0);

                    if (Arguments != currentList)
                        throw new Exception("Nested Arrays are not supported");
                    typeString += "[";
                    currentList = array;
                    ArgumentsIndex = i;
                    i = 0;
                    continue;

                default:
                    throw new Exception("Unable to transmit values of type " + arg.GetType().FullName);
            }

            i++;
            if (currentList != Arguments && i == currentList.Length)
            {
                // End of array, go back to main Argument list
                typeString += "]";
                currentList = Arguments;
                i = ArgumentsIndex + 1;
            }
        }

        int addressLen = (Address.Length == 0 || Address == null) ? 0 : Utils.AlignedStringLength(Address);
        int typeLen = Utils.AlignedStringLength(typeString);

        int total = addressLen + typeLen + parts.Sum(x => x.Length);

        byte[] output = new byte[total];
        i = 0;

        Encoding.ASCII.GetBytes(Address).CopyTo(output, i);
        i += addressLen;

        Encoding.ASCII.GetBytes(typeString).CopyTo(output, i);
        i += typeLen;

        foreach (byte[] part in parts)
        {
            part.CopyTo(output, i);
            i += part.Length;
        }

        return output;
    }
}