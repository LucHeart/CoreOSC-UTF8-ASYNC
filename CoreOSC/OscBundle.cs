using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LucHeart.CoreOSC;

public class OscBundle : IOscPacket
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
    
    public byte[] GetBytes()
    {
        var outMessages = Messages.Select(msg => msg.GetBytes()).ToArray();

        var tag = OscPacketUtils.SetULong(_timeTag.Tag);
        var len = BundleTagLen + tag.Length + outMessages.Sum(x => x.Length + 4);
        
        var output = new byte[len];
        Encoding.ASCII.GetBytes(BundleName).CopyTo(output, 0);
        var i = BundleTagLen;
        tag.CopyTo(output, i);
        i += tag.Length;

        foreach (var msg in outMessages)
        {
            var size = OscPacketUtils.SetInt(msg.Length);
            size.CopyTo(output, i);
            i += size.Length;

            msg.CopyTo(output, i);
            i += msg.Length; // msg size is always a multiple of 4
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

        var bundleTag = Encoding.ASCII.GetString(msgReadOnly[..8]);
        index += 8;

        var timeTag = OscPacketUtils.GetULong(msgReadOnly, index);
        index += 8;

        if (bundleTag != "#bundle\0")
            throw new Exception("Not a bundle");

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
    
    public static bool IsBundle(ReadOnlySpan<byte> msg)
    {
        if (msg.Length < 8)
            return false;
        
        var bundleTag = Encoding.ASCII.GetString(msg[..8]);
        return bundleTag == "#bundle\0";
    }
}