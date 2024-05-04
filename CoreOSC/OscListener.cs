using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LucHeart.CoreOSC;

public class OscListener : IDisposable, IOscListener
{
    internal Queue<OscMessage> MessageQueue;
    internal readonly UdpClient UdpClient;
    
    public bool EnableTransparentBundleToMessageConversion = false;

    public OscListener(IPEndPoint listenerEndPoint)
    {
        UdpClient = new UdpClient(listenerEndPoint);
        MessageQueue = new Queue<OscMessage>();
    }

    public async Task<OscMessage> ReceiveMessageAsync()
    {
        if (EnableTransparentBundleToMessageConversion)
        {
            if (MessageQueue.Count > 0)
                return MessageQueue.Dequeue();
            
            var receiveResult = await UdpClient.ReceiveAsync();

            if (!OscBundle.IsBundle(receiveResult.Buffer))
                return OscMessage.ParseMessage(receiveResult.Buffer);
            
            var bundle = OscBundle.ParseBundle(receiveResult.Buffer);
            foreach (var bundleMessage in bundle.Messages)
                MessageQueue.Enqueue(bundleMessage);
            
            return MessageQueue.Dequeue();
        }
        else
        {
            var receiveResult = await UdpClient.ReceiveAsync();
            return OscMessage.ParseMessage(receiveResult.Buffer);
        }
    }

    public async Task<OscBundle> ReceiveBundleAsync()
    {
        var receiveResult = await UdpClient.ReceiveAsync();
        return OscBundle.ParseBundle(receiveResult.Buffer);
    }

    public void Dispose()
    {
        UdpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}