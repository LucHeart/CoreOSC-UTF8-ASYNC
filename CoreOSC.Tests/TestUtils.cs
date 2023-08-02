using System.Net;
using System.Threading;

namespace LucHeart.CoreOSC.Tests;

public static class TestUtils
{
    private static int _port = 30000;
    
    public static IPEndPoint GetNextEndpoint() => new(IPAddress.Loopback, Interlocked.Increment(ref _port));
}