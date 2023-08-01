using System;
using System.Threading;
using NUnit.Framework;

namespace LucHeart.CoreOSC.Tests;

[TestFixture]
internal class ListenerTest
{
    /// <summary>
    /// Opens a listener on a specified port, then closes it and attempts to open another on the same port
    /// Opening the second listener will fail unless the first one has been properly closed.
    /// </summary>
    [TestCase]
    public void CloseListener()
    {
        var l1 = new UDPListener(Constants.TestPort);
        var isnull = l1.Receive();
        l1.Close();

        var l2 = new UDPListener(Constants.TestPort);
        isnull = l2.Receive();
        l2.Close();
    }

    /// <summary>
    /// Tries to open two listeners on the same port, results in an exception
    /// </summary>
    [TestCase]
    public void CloseListenerException()
    {
        UDPListener l1 = null;
        bool ex = false;
        try
        {
            l1 = new UDPListener(Constants.TestPort);
            var isnull = l1.Receive();
            var l2 = new UDPListener(Constants.TestPort);
        }
        catch (Exception)
        {
            ex = true;
        }

        Assert.IsTrue(ex);
        l1.Close();
    }

    /// <summary>
    /// Single message receive
    /// </summary>
    [TestCase]
    public void ListenerSingleMSG()
    {
        var listener = new UDPListener(Constants.TestPort);

        var sender = new CoreOSC.UDPSender(Constants.TestAddress, Constants.TestPort);

        var msg = new CoreOSC.OscMessage("/test/", 23.42f);

        sender.Send(msg);

        while (true)
        {
            var pack = listener.Receive();
            if (pack == null)
                Thread.Sleep(1);
            else
                break;
        }

        listener.Dispose();
    }

    /// <summary>
    /// Bombard the listener with messages, check if they are all received
    /// </summary>
    [TestCase]
    public void ListenerLoadTest()
    {
        var listener = new UDPListener(Constants.TestPort);

        var sender = new CoreOSC.UDPSender(Constants.TestAddress, Constants.TestPort);

        var msg = new CoreOSC.OscMessage("/test/", 23.42f);

        for (int i = 0; i < 1000; i++)
            sender.Send(msg);

        for (int i = 0; i < 1000; i++)
        {
            var receivedMessage = listener.Receive();
            Assert.NotNull(receivedMessage);
        }

        listener.Dispose();
    }
}