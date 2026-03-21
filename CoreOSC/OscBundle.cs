using System;
using System.Collections.Generic;
using System.Text;

namespace LucHeart.CoreOSC;

public sealed class OscBundle : IOscPacket
{
    private TimeTag _timeTag;

    public ulong TimeTag
    {
        get => _timeTag.Tag;
        set => _timeTag.Tag = value;
    }

    public DateTime Timestamp
    {
        get => _timeTag.Timestamp;
        set => _timeTag.Timestamp = value;
    }

    public readonly List<OscMessage> Messages;

    public OscBundle(ulong timetag, params OscMessage[] args)
    {
        _timeTag = new TimeTag(timetag);
        Messages = new List<OscMessage>();
        Messages.AddRange(args);
    }

    private const string BundleName = "#bundle";
    private static readonly int BundleTagLen = Utils.AlignedStringLength(BundleName);
    
    /// <inheritdoc />
    public byte[] GetBytes()
    {
        var messageCount = Messages.Count;
        var outMessages = new byte[messageCount][];
        var totalMsgSize = 0;
        for (var m = 0; m < messageCount; m++)
        {
            outMessages[m] = Messages[m].GetBytes();
            totalMsgSize += outMessages[m].Length + 4; // +4 for size prefix
        }

        var len = BundleTagLen + 8 + totalMsgSize;
        var output = new byte[len];
        var span = output.AsSpan();

        Encoding.ASCII.GetBytes(BundleName.AsSpan(), span);
        var i = BundleTagLen;

        OscPacketUtils.WriteULong(span[i..], _timeTag.Tag);
        i += 8;

        foreach (var msg in outMessages)
        {
            OscPacketUtils.WriteInt(span[i..], msg.Length);
            i += 4;

            msg.CopyTo(span[i..]);
            i += msg.Length;
        }

        return output;
    }
    
    /// <summary>
    /// Takes in an OSC bundle package in byte form and parses it into a more usable OscBundle object
    /// </summary>
    /// <param name="msg"></param>
    /// <returns>Bundle containing elements and a timetag</returns>
    public static OscBundle ParseBundle(Span<byte> msg)
    {
        ReadOnlySpan<byte> msgReadOnly = msg;
        var messages = new List<OscMessage>();

        var index = 0;

        if (!IsBundle(msgReadOnly))
            throw new Exception("Not a bundle");
        index += 8;

        var timeTag = OscPacketUtils.GetULong(msgReadOnly, index);
        index += 8;

        while (index < msgReadOnly.Length)
        {
            var size = OscPacketUtils.GetInt(msgReadOnly, index);
            index += 4;

            var messageBytes = msg.Slice(index, size);
            var message = OscMessage.ParseMessage(messageBytes);
            messages.Add(message);

            index += size;
            while (index % 4 != 0)
                index++;
        }

        var output = new OscBundle(timeTag, messages.ToArray());
        return output;
    }
    
    private static ReadOnlySpan<byte> BundleTagBytes => "#bundle\0"u8;

    public static bool IsBundle(ReadOnlySpan<byte> msg)
    {
        if (msg.Length < 8)
            return false;

        return msg[..8].SequenceEqual(BundleTagBytes);
    }
}