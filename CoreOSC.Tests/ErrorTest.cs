using System.Text;

namespace LucHeart.CoreOSC.Tests;

/// <summary>
/// Tests for error handling and malformed input across parsers.
/// </summary>
public class ErrorTest
{
    #region ParseMessage errors

    [Test]
    public async Task ParseMessageThrowsOnMisalignedAddress()
    {
        // Build a message where the comma is at a non-4-aligned position
        // "/ab," = 4 bytes, comma at index 3 which is not divisible by 4
        var bytes = new byte[] { (byte)'/', (byte)'a', (byte)'b', OscPacketUtils.DividerChar, 0, 0, 0, 0 };
        Assert.Throws<Exception>(() => OscMessage.ParseMessage(bytes));
    }

    [Test]
    public async Task ParseMessageThrowsOnUnknownTypeTag()
    {
        // Use a real message and tamper with the type tag
        var msg = new OscMessage("/test", 42);
        var bytes = msg.GetBytes();

        // Address "/test" = 8 bytes (padded), type ",i\0\0" starts at byte 8
        // byte 9 should be 'i' — change to 'Q'
        bytes[9] = (byte)'Q';

        Assert.Throws<Exception>(() => OscMessage.ParseMessage(bytes));
    }

    #endregion

    #region GetAddress errors

    [Test]
    public async Task GetAddressThrowsWhenNoCommaFound()
    {
        var bytes = new byte[] { (byte)'/', (byte)'t', (byte)'e', (byte)'s' };
        Assert.Throws<Exception>(() => OscPacketUtils.GetAddress(bytes, out _));
    }

    #endregion

    #region GetString / GetTypes errors

    [Test]
    public async Task GetStringThrowsWhenNoNullTerminator()
    {
        // A string block with no null byte — all non-zero
        var bytes = new byte[] { (byte)'h', (byte)'e', (byte)'l', (byte)'l' };
        Assert.Throws<Exception>(() => OscPacketUtils.GetString(bytes, 0, out _));
    }

    [Test]
    public async Task GetTypesThrowsWhenNoNullTerminator()
    {
        // Type block with no null byte — all non-zero and length is 4
        var bytes = new byte[] { (byte)',', (byte)'i', (byte)'f', (byte)'s' };
        Assert.Throws<Exception>(() => OscPacketUtils.GetTypes(bytes, 0));
    }

    #endregion

    #region ParseBundle errors

    [Test]
    public async Task ParseBundleThrowsOnNonBundleData()
    {
        var bytes = new byte[16];
        Encoding.ASCII.GetBytes("notabund").CopyTo(bytes, 0);
        Assert.Throws<Exception>(() => OscBundle.ParseBundle(bytes));
    }

    #endregion

    #region GetBytes with invalid types

    [Test]
    public async Task GetBytesThrowsOnDecimal()
    {
        var msg = new OscMessage("/bad", 1.0m);
        Assert.Throws<Exception>(() => msg.GetBytes());
    }

    [Test]
    public async Task GetBytesThrowsOnObject()
    {
        var msg = new OscMessage("/bad", new object());
        Assert.Throws<Exception>(() => msg.GetBytes());
    }

    [Test]
    public async Task GetBytesThrowsOnShort()
    {
        var msg = new OscMessage("/bad", (short)5);
        Assert.Throws<Exception>(() => msg.GetBytes());
    }

    #endregion
}
