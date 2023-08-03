using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LucHeart.CoreOSC;

public class OscBundle : OscPacket
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
    
    public override byte[] GetBytes()
    {
        var outMessages = Messages.Select(msg => msg.GetBytes()).ToArray();

        var tag = SetULong(_timeTag.Tag);
        var len = BundleTagLen + tag.Length + outMessages.Sum(x => x.Length + 4);
        
        var output = new byte[len];
        Encoding.ASCII.GetBytes(BundleName).CopyTo(output, 0);
        var i = BundleTagLen;
        tag.CopyTo(output, i);
        i += tag.Length;

        foreach (var msg in outMessages)
        {
            var size = SetInt(msg.Length);
            size.CopyTo(output, i);
            i += size.Length;

            msg.CopyTo(output, i);
            i += msg.Length; // msg size is always a multiple of 4
        }

        return output;
    }
}