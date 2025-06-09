﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LucHeart.CoreOSC;

public class OscListener : IDisposable, IOscListener
{
    internal readonly UdpClient UdpClient;

    public OscListener(IPEndPoint listenerEndPoint)
    {
        UdpClient = new UdpClient(listenerEndPoint);

        // Set the SIO_UDP_CONNRESET ioctl to true for this UDP socket. If this UDP socket
        // ever sends a UDP packet to a remote destination that exists but there is
        // no socket to receive the packet, an ICMP port unreachable message is returned
        // to the sender. By default, when this is received the next operation on the
        // UDP socket that send the packet will receive a SocketException. The native
        // (Winsock) error that is received is WSAECONNRESET (10054). Since we don't want
        // to wrap each UDP socket operation in a try/except, we'll disable this error
        // for the socket with this ioctl call.
        try
        {
            uint IOC_IN = 0x80000000;
            uint IOC_VENDOR = 0x18000000;
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

            byte[] optionInValue = { Convert.ToByte(false) };
            byte[] optionOutValue = new byte[4];
            UdpClient.Client.IOControl((int)SIO_UDP_CONNRESET, optionInValue, optionOutValue);
        }
        catch
        {
            // Unable to set SIO_UDP_CONNRESET, maybe not supported
        }
    }

    public async Task<OscMessage> ReceiveMessageAsync()
    {
        var receiveResult = await UdpClient.ReceiveAsync();
        return OscMessage.ParseMessage(receiveResult.Buffer);
    }

    public async Task<(OscMessage Message, IPEndPoint EndPoint)> ReceiveMessageExAsync()
    {
        var receiveResult = await UdpClient.ReceiveAsync();
        return (OscMessage.ParseMessage(receiveResult.Buffer), receiveResult.RemoteEndPoint);
    }

    public async Task<OscBundle> ReceiveBundleAsync()
    {
        var receiveResult = await UdpClient.ReceiveAsync();
        return OscBundle.ParseBundle(receiveResult.Buffer);
    }

    public async Task<(OscBundle Bundle, IPEndPoint EndPoint)> ReceiveBundleExAsync()
    {
        var receiveResult = await UdpClient.ReceiveAsync();
        return (OscBundle.ParseBundle(receiveResult.Buffer), receiveResult.RemoteEndPoint);
    }

    public void Dispose()
    {
        UdpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}