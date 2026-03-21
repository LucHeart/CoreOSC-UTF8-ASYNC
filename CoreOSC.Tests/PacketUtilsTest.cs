namespace LucHeart.CoreOSC.Tests;

/// <summary>
/// Direct unit tests for OscPacketUtils Set/Get methods.
/// </summary>
public class PacketUtilsTest
{
    #region Int

    [Test]
    [Arguments(0)]
    [Arguments(1)]
    [Arguments(-1)]
    [Arguments(int.MaxValue)]
    [Arguments(int.MinValue)]
    [Arguments(42)]
    public async Task SetGetInt(int value)
    {
        var bytes = OscPacketUtils.SetInt(value);
        var result = OscPacketUtils.GetInt(bytes, 0);

        await Assert.That(result).IsEqualTo(value);
    }

    #endregion

    #region Float

    [Test]
    [Arguments(0f)]
    [Arguments(1.0f)]
    [Arguments(-1.0f)]
    [Arguments(3.14f)]
    [Arguments(float.MaxValue)]
    [Arguments(float.MinValue)]
    [Arguments(float.Epsilon)]
    public async Task SetGetFloat(float value)
    {
        var bytes = OscPacketUtils.SetFloat(value);
        Span<byte> span = bytes;
        var result = OscPacketUtils.GetFloat(span, 0);

        await Assert.That(result).IsEqualTo(value);
    }

    #endregion

    #region Double

    [Test]
    [Arguments(0.0)]
    [Arguments(1.0)]
    [Arguments(-1.0)]
    [Arguments(3.141592653589793)]
    [Arguments(double.MaxValue)]
    [Arguments(double.MinValue)]
    [Arguments(double.Epsilon)]
    public async Task SetGetDouble(double value)
    {
        var bytes = OscPacketUtils.SetDouble(value);
        Span<byte> span = bytes;
        var result = OscPacketUtils.GetDouble(span, 0);

        await Assert.That(result).IsEqualTo(value);
    }

    #endregion

    #region Long

    [Test]
    [Arguments(0L)]
    [Arguments(1L)]
    [Arguments(-1L)]
    [Arguments(long.MaxValue)]
    [Arguments(long.MinValue)]
    public async Task SetGetLong(long value)
    {
        var bytes = OscPacketUtils.SetLong(value);
        var result = OscPacketUtils.GetLong(bytes, 0);

        await Assert.That(result).IsEqualTo(value);
    }

    #endregion

    #region ULong

    [Test]
    [Arguments(0UL)]
    [Arguments(1UL)]
    [Arguments(ulong.MaxValue)]
    public async Task SetGetULong(ulong value)
    {
        var bytes = OscPacketUtils.SetULong(value);
        var result = OscPacketUtils.GetULong(bytes, 0);

        await Assert.That(result).IsEqualTo(value);
    }

    #endregion

    #region String

    [Test]
    [Arguments("hello")]
    [Arguments("")]
    [Arguments("a")]
    [Arguments("ab")]
    [Arguments("abc")]
    [Arguments("abcd")]
    [Arguments("abcde")]
    [Arguments("test string with spaces")]
    public async Task SetGetString(string value)
    {
        var bytes = OscPacketUtils.SetString(value);
        var result = OscPacketUtils.GetString(bytes, 0, out _);

        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task SetGetStringUtf8()
    {
        var value = "héllo ⚡";
        var bytes = OscPacketUtils.SetString(value);
        var result = OscPacketUtils.GetString(bytes, 0, out _);

        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task StringBytesPaddedTo4()
    {
        // All string byte arrays must be padded to 4-byte boundaries
        var values = new[] { "", "a", "ab", "abc", "abcd", "abcde", "abcdef", "abcdefg", "abcdefgh" };
        foreach (var val in values)
        {
            var bytes = OscPacketUtils.SetString(val);
            await Assert.That(bytes.Length % 4).IsEqualTo(0);
        }
    }

    [Test]
    public async Task GetStringAdvancesIndex()
    {
        var bytes = OscPacketUtils.SetString("test");
        OscPacketUtils.GetString(bytes, 0, out var newIndex);

        await Assert.That(newIndex).IsEqualTo(bytes.Length);
        await Assert.That(newIndex % 4).IsEqualTo(0);
    }

    #endregion

    #region Blob

    [Test]
    public async Task SetGetBlob()
    {
        var value = new byte[] { 1, 2, 3, 4, 5 };
        var bytes = OscPacketUtils.SetBlob(value);
        var result = OscPacketUtils.GetBlob(bytes, 0);

        await Assert.That(result.ToArray()).IsEquivalentTo(value);
    }

    [Test]
    public async Task SetGetBlobEmpty()
    {
        var value = Array.Empty<byte>();
        var bytes = OscPacketUtils.SetBlob(value);
        var result = OscPacketUtils.GetBlob(bytes, 0);

        await Assert.That(result.Length).IsEqualTo(0);
    }

    [Test]
    public async Task BlobBytesPaddedTo4()
    {
        for (var size = 0; size <= 20; size++)
        {
            var blob = new byte[size];
            var bytes = OscPacketUtils.SetBlob(blob);
            await Assert.That(bytes.Length % 4).IsEqualTo(0);
        }
    }

    #endregion

    #region Char

    [Test]
    [Arguments('a')]
    [Arguments('Z')]
    [Arguments('0')]
    [Arguments(' ')]
    public async Task SetGetChar(char value)
    {
        var bytes = OscPacketUtils.SetChar(value);
        var result = OscPacketUtils.GetChar(bytes, 0);

        await Assert.That(result).IsEqualTo(value);
    }

    #endregion

    #region RGBA

    [Test]
    public async Task GetRgba()
    {
        var bytes = new byte[] { 10, 20, 30, 40 };
        var result = OscPacketUtils.GetRgba(bytes, 0);

        await Assert.That(result.R).IsEqualTo((byte)10);
        await Assert.That(result.G).IsEqualTo((byte)20);
        await Assert.That(result.B).IsEqualTo((byte)30);
        await Assert.That(result.A).IsEqualTo((byte)40);
    }

    [Test]
    public async Task GetRgbaAtOffset()
    {
        var bytes = new byte[] { 0, 0, 0, 0, 10, 20, 30, 40 };
        var result = OscPacketUtils.GetRgba(bytes, 4);

        await Assert.That(result.R).IsEqualTo((byte)10);
        await Assert.That(result.G).IsEqualTo((byte)20);
        await Assert.That(result.B).IsEqualTo((byte)30);
        await Assert.That(result.A).IsEqualTo((byte)40);
    }

    #endregion

    #region Midi

    [Test]
    public async Task GetMidi()
    {
        var bytes = new byte[] { 1, 144, 60, 127 };
        var result = OscPacketUtils.GetMidi(bytes, 0);

        await Assert.That(result.Port).IsEqualTo((byte)1);
        await Assert.That(result.Status).IsEqualTo((byte)144);
        await Assert.That(result.Data1).IsEqualTo((byte)60);
        await Assert.That(result.Data2).IsEqualTo((byte)127);
    }

    [Test]
    public async Task GetMidiAtOffset()
    {
        var bytes = new byte[] { 0, 0, 0, 0, 1, 144, 60, 127 };
        var result = OscPacketUtils.GetMidi(bytes, 4);

        await Assert.That(result.Port).IsEqualTo((byte)1);
        await Assert.That(result.Status).IsEqualTo((byte)144);
        await Assert.That(result.Data1).IsEqualTo((byte)60);
        await Assert.That(result.Data2).IsEqualTo((byte)127);
    }

    #endregion

    #region GetAddress

    [Test]
    [Arguments("/test")]
    [Arguments("/a")]
    [Arguments("/test/nested")]
    public async Task GetAddressReturnsCleanString(string address)
    {
        var msg = new OscMessage(address, 1);
        var bytes = msg.GetBytes();
        var result = OscPacketUtils.GetAddress(bytes, out var index);

        await Assert.That(result).IsEqualTo(address);
        await Assert.That(index % 4).IsEqualTo(0);
    }

    [Test]
    public async Task GetAddressThrowsOnNoComma()
    {
        var bytes = new byte[] { (byte)'/', (byte)'t', 0, 0 };
        Assert.Throws<Exception>(() => OscPacketUtils.GetAddress(bytes, out _));
    }

    #endregion

    #region AlignedStringLength

    [Test]
    [Arguments("", 4)]
    [Arguments("a", 4)]
    [Arguments("ab", 4)]
    [Arguments("abc", 4)]
    [Arguments("abcd", 8)]
    [Arguments("abcde", 8)]
    [Arguments("abcdef", 8)]
    [Arguments("abcdefg", 8)]
    [Arguments("abcdefgh", 12)]
    public async Task AlignedStringLength(string value, int expectedLen)
    {
        var result = Utils.AlignedStringLength(value);
        await Assert.That(result).IsEqualTo(expectedLen);
        await Assert.That(result % 4).IsEqualTo(0);
    }

    #endregion
}
