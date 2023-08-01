using System;
using System.Threading;
using Xunit;

namespace LucHeart.CoreOSC.Tests;

// [TestFixture]
// internal class CallbackTest
// {
//     [Fact]
//     public void TestCallback()
//     {
//         bool cbCalled = false;
//         // The cabllback function
//         HandleOscPacket cb = delegate (OscPacket packet)
//         {
//             var msg = (OscMessage)packet;
//             Assert.AreEqual(2, msg.Arguments.Length);
//             Assert.AreEqual(23, msg.Arguments[0]);
//             Assert.AreEqual("hello world", msg.Arguments[1]);
//             cbCalled = true;
//         };
//
//         var l1 = new UdpListener(Constants.TestPort, cb);
//
//         var sender = new CoreOSC.UDPSender(Constants.TestAddress, Constants.TestPort);
//         var msg1 = new CoreOSC.OscMessage("/test/address", 23, "hello world");
//         sender.Send(msg1);
//
//         // Wait until callback processes its message
//         var start = DateTime.Now;
//         while (cbCalled == false && start.AddSeconds(2) > DateTime.Now)
//             Thread.Sleep(1);
//
//         Assert.IsTrue(cbCalled);
//
//         l1.Close();
//     }
//
//     [TestCase]
//     public void TestByteCallback()
//     {
//         bool cbCalled = false;
//         // The cabllback function
//         HandleBytePacket cb = delegate (byte[] packet)
//         {
//             var msg = (OscMessage)OscPacket.GetPacket(packet);
//             Assert.AreEqual(2, msg.Arguments.Length);
//             Assert.AreEqual(23, msg.Arguments[0]);
//             Assert.AreEqual("hello world", msg.Arguments[1]);
//             cbCalled = true;
//         };
//
//         var l1 = new UdpListener(Constants.TestPort, cb);
//
//         var sender = new CoreOSC.UDPSender(Constants.TestAddress, Constants.TestPort);
//         var msg1 = new CoreOSC.OscMessage("/test/address", 23, "hello world");
//         sender.Send(msg1);
//
//         // Wait until callback processes its message
//         var start = DateTime.Now;
//         while (cbCalled == false && start.AddSeconds(2) > DateTime.Now)
//             Thread.Sleep(1);
//
//         Assert.IsTrue(cbCalled);
//
//         l1.Close();
//     }
// }