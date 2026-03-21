namespace LucHeart.CoreOSC.Tests;

/// <summary>
/// Tests that serialize a message to bytes and parse it back, verifying all argument types
/// survive the roundtrip with correct values.
/// </summary>
public class RoundtripTest
{
    #region Individual type roundtrips

    [Test]
    public async Task RoundtripInt()
    {
        var msg = new OscMessage("/int", 42);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Address).IsEqualTo("/int");
        await Assert.That(parsed.Arguments.Length).IsEqualTo(1);
        await Assert.That(parsed.Arguments[0]).IsEqualTo(42);
    }

    [Test]
    [Arguments(0)]
    [Arguments(int.MinValue)]
    [Arguments(int.MaxValue)]
    [Arguments(-1)]
    [Arguments(1)]
    public async Task RoundtripIntEdgeCases(int value)
    {
        var msg = new OscMessage("/int", value);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments[0]).IsEqualTo(value);
    }

    [Test]
    public async Task RoundtripFloat()
    {
        var msg = new OscMessage("/float", 3.14f);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Address).IsEqualTo("/float");
        await Assert.That(parsed.Arguments[0]).IsEqualTo(3.14f);
    }

    [Test]
    [Arguments(0f)]
    [Arguments(-0f)]
    [Arguments(float.MinValue)]
    [Arguments(float.MaxValue)]
    [Arguments(float.Epsilon)]
    [Arguments(-1.23456f)]
    public async Task RoundtripFloatEdgeCases(float value)
    {
        var msg = new OscMessage("/float", value);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments[0]).IsEqualTo(value);
    }

    [Test]
    public async Task RoundtripFloatPositiveInfinity()
    {
        var msg = new OscMessage("/inf", float.PositiveInfinity);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        // PositiveInfinity is encoded as type 'I' and decoded as double.PositiveInfinity
        await Assert.That(parsed.Arguments[0]).IsEqualTo(double.PositiveInfinity);
    }

    [Test]
    public async Task RoundtripDouble()
    {
        var msg = new OscMessage("/double", 3.141592653589793);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments[0]).IsEqualTo(3.141592653589793);
    }

    [Test]
    [Arguments(0.0)]
    [Arguments(double.MinValue)]
    [Arguments(double.MaxValue)]
    [Arguments(double.Epsilon)]
    [Arguments(-9999999.123456789)]
    public async Task RoundtripDoubleEdgeCases(double value)
    {
        var msg = new OscMessage("/double", value);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments[0]).IsEqualTo(value);
    }

    [Test]
    public async Task RoundtripDoublePositiveInfinity()
    {
        var msg = new OscMessage("/inf", double.PositiveInfinity);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments[0]).IsEqualTo(double.PositiveInfinity);
    }

    [Test]
    [Arguments("hello")]
    [Arguments("")]
    [Arguments("a")]
    [Arguments("ab")]
    [Arguments("abc")]
    [Arguments("abcd")]
    [Arguments("abcde")]
    public async Task RoundtripString(string value)
    {
        var msg = new OscMessage("/str", value);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments[0]).IsEqualTo(value);
    }

    [Test]
    public async Task RoundtripStringUtf8()
    {
        var value = "héllo wörld ⚡🎵";
        var msg = new OscMessage("/str", value);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments[0]).IsEqualTo(value);
    }

    [Test]
    public async Task RoundtripStringLong()
    {
        var value = new string('x', 1000);
        var msg = new OscMessage("/str", value);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments[0]).IsEqualTo(value);
    }

    [Test]
    public async Task RoundtripBlob()
    {
        var blob = new byte[] { 1, 2, 3, 4, 5 };
        var msg = new OscMessage("/blob", blob);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That((byte[])parsed.Arguments[0]!).IsEquivalentTo(blob);
    }

    [Test]
    public async Task RoundtripBlobEmpty()
    {
        var blob = Array.Empty<byte>();
        var msg = new OscMessage("/blob", blob);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That((byte[])parsed.Arguments[0]!).IsEquivalentTo(blob);
    }

    [Test]
    public async Task RoundtripBlobLarge()
    {
        var blob = new byte[256];
        for (var i = 0; i < blob.Length; i++) blob[i] = (byte)(i % 256);
        var msg = new OscMessage("/blob", blob);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That((byte[])parsed.Arguments[0]!).IsEquivalentTo(blob);
    }

    [Test]
    [Arguments(0L)]
    [Arguments(long.MinValue)]
    [Arguments(long.MaxValue)]
    [Arguments(123456789012345L)]
    [Arguments(-123456789012345L)]
    public async Task RoundtripLong(long value)
    {
        var msg = new OscMessage("/long", value);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments[0]).IsEqualTo(value);
    }

    [Test]
    public async Task RoundtripTimeTag()
    {
        var tag = new TimeTag(DateTime.Parse("2025-06-15T10:30:00Z"));
        var msg = new OscMessage("/tt", tag);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        var parsedTag = (TimeTag)parsed.Arguments[0]!;
        await Assert.That(parsedTag.Tag).IsEqualTo(tag.Tag);
    }

    [Test]
    public async Task RoundtripSymbol()
    {
        var symbol = new Symbol("my_symbol");
        var msg = new OscMessage("/sym", symbol);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        var parsedSymbol = (Symbol)parsed.Arguments[0]!;
        await Assert.That(parsedSymbol.Value).IsEqualTo("my_symbol");
    }

    [Test]
    public async Task RoundtripSymbolUtf8()
    {
        var symbol = new Symbol("symbôl_⚡");
        var msg = new OscMessage("/sym", symbol);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        var parsedSymbol = (Symbol)parsed.Arguments[0]!;
        await Assert.That(parsedSymbol.Value).IsEqualTo("symbôl_⚡");
    }

    [Test]
    [Arguments('a')]
    [Arguments('Z')]
    [Arguments('0')]
    [Arguments(' ')]
    public async Task RoundtripChar(char value)
    {
        var msg = new OscMessage("/char", value);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments[0]).IsEqualTo(value);
    }

    [Test]
    public async Task RoundtripRgba()
    {
        var rgba = new RGBA(255, 128, 0, 200);
        var msg = new OscMessage("/rgba", rgba);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        var parsedRgba = (RGBA)parsed.Arguments[0]!;
        await Assert.That(parsedRgba.R).IsEqualTo((byte)255);
        await Assert.That(parsedRgba.G).IsEqualTo((byte)128);
        await Assert.That(parsedRgba.B).IsEqualTo((byte)0);
        await Assert.That(parsedRgba.A).IsEqualTo((byte)200);
    }

    [Test]
    public async Task RoundtripRgbaMinMax()
    {
        var rgba = new RGBA(0, 0, 0, 0);
        var msg = new OscMessage("/rgba", rgba);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());
        var parsedRgba = (RGBA)parsed.Arguments[0]!;
        await Assert.That(parsedRgba).IsEqualTo(rgba);

        var rgba2 = new RGBA(255, 255, 255, 255);
        var msg2 = new OscMessage("/rgba", rgba2);
        var parsed2 = OscMessage.ParseMessage(msg2.GetBytes());
        var parsedRgba2 = (RGBA)parsed2.Arguments[0]!;
        await Assert.That(parsedRgba2).IsEqualTo(rgba2);
    }

    [Test]
    public async Task RoundtripMidi()
    {
        var midi = new Midi(1, 144, 60, 127);
        var msg = new OscMessage("/midi", midi);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        var parsedMidi = (Midi)parsed.Arguments[0]!;
        await Assert.That(parsedMidi.Port).IsEqualTo((byte)1);
        await Assert.That(parsedMidi.Status).IsEqualTo((byte)144);
        await Assert.That(parsedMidi.Data1).IsEqualTo((byte)60);
        await Assert.That(parsedMidi.Data2).IsEqualTo((byte)127);
    }

    [Test]
    public async Task RoundtripTrue()
    {
        var msg = new OscMessage("/bool", true);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That((bool)parsed.Arguments[0]!).IsTrue();
    }

    [Test]
    public async Task RoundtripFalse()
    {
        var msg = new OscMessage("/bool", false);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That((bool)parsed.Arguments[0]!).IsFalse();
    }

    [Test]
    public async Task RoundtripNull()
    {
        var msg = new OscMessage("/null", new object?[] { null });
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments[0]).IsNull();
    }

    #endregion

    #region Multi-argument roundtrips

    [Test]
    public async Task RoundtripAllTypes()
    {
        var blob = new byte[] { 10, 20, 30 };
        var tag = new TimeTag(DateTime.Parse("2025-01-01T00:00:00Z"));
        var msg = new OscMessage("/all",
            42,
            3.14f,
            "hello",
            blob,
            999999999999L,
            tag,
            2.718281828,
            new Symbol("sym"),
            'X',
            new RGBA(1, 2, 3, 4),
            new Midi(5, 6, 7, 8),
            true,
            false,
            null,
            double.PositiveInfinity
        );

        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Address).IsEqualTo("/all");
        await Assert.That(parsed.Arguments.Length).IsEqualTo(15);

        await Assert.That(parsed.Arguments[0]).IsEqualTo(42);
        await Assert.That(parsed.Arguments[1]).IsEqualTo(3.14f);
        await Assert.That(parsed.Arguments[2]).IsEqualTo("hello");
        await Assert.That((byte[])parsed.Arguments[3]!).IsEquivalentTo(blob);
        await Assert.That(parsed.Arguments[4]).IsEqualTo(999999999999L);
        await Assert.That(((TimeTag)parsed.Arguments[5]!).Tag).IsEqualTo(tag.Tag);
        await Assert.That(parsed.Arguments[6]).IsEqualTo(2.718281828);
        await Assert.That(((Symbol)parsed.Arguments[7]!).Value).IsEqualTo("sym");
        await Assert.That(parsed.Arguments[8]).IsEqualTo('X');
        await Assert.That(parsed.Arguments[9]).IsEqualTo(new RGBA(1, 2, 3, 4));
        await Assert.That(parsed.Arguments[10]).IsEqualTo(new Midi(5, 6, 7, 8));
        await Assert.That((bool)parsed.Arguments[11]!).IsTrue();
        await Assert.That((bool)parsed.Arguments[12]!).IsFalse();
        await Assert.That(parsed.Arguments[13]).IsNull();
        await Assert.That(parsed.Arguments[14]).IsEqualTo(double.PositiveInfinity);
    }

    [Test]
    public async Task RoundtripNoArguments()
    {
        var msg = new OscMessage("/empty");
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Address).IsEqualTo("/empty");
        await Assert.That(parsed.Arguments.Length).IsEqualTo(0);
    }

    [Test]
    public async Task RoundtripMultipleStrings()
    {
        var msg = new OscMessage("/strs", "aaa", "bb", "c", "dddd", "eeeee");
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments.Length).IsEqualTo(5);
        await Assert.That(parsed.Arguments[0]).IsEqualTo("aaa");
        await Assert.That(parsed.Arguments[1]).IsEqualTo("bb");
        await Assert.That(parsed.Arguments[2]).IsEqualTo("c");
        await Assert.That(parsed.Arguments[3]).IsEqualTo("dddd");
        await Assert.That(parsed.Arguments[4]).IsEqualTo("eeeee");
    }

    [Test]
    public async Task RoundtripMultipleInts()
    {
        var msg = new OscMessage("/ints", 0, 1, -1, 100, -100, int.MaxValue, int.MinValue);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments.Length).IsEqualTo(7);
        await Assert.That(parsed.Arguments[0]).IsEqualTo(0);
        await Assert.That(parsed.Arguments[1]).IsEqualTo(1);
        await Assert.That(parsed.Arguments[2]).IsEqualTo(-1);
        await Assert.That(parsed.Arguments[3]).IsEqualTo(100);
        await Assert.That(parsed.Arguments[4]).IsEqualTo(-100);
        await Assert.That(parsed.Arguments[5]).IsEqualTo(int.MaxValue);
        await Assert.That(parsed.Arguments[6]).IsEqualTo(int.MinValue);
    }

    [Test]
    public async Task RoundtripMixedIntFloatString()
    {
        var msg = new OscMessage("/mixed", 1, 2.0f, "three", 4, 5.0f, "six");
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments.Length).IsEqualTo(6);
        await Assert.That(parsed.Arguments[0]).IsEqualTo(1);
        await Assert.That(parsed.Arguments[1]).IsEqualTo(2.0f);
        await Assert.That(parsed.Arguments[2]).IsEqualTo("three");
        await Assert.That(parsed.Arguments[3]).IsEqualTo(4);
        await Assert.That(parsed.Arguments[4]).IsEqualTo(5.0f);
        await Assert.That(parsed.Arguments[5]).IsEqualTo("six");
    }

    #endregion

    #region Array roundtrips

    [Test]
    public async Task RoundtripArraySingleElement()
    {
        var list = new List<object> { 99 };
        var msg = new OscMessage("/arr", 1, list, 2);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments.Length).IsEqualTo(3);
        await Assert.That(parsed.Arguments[0]).IsEqualTo(1);
        await Assert.That(parsed.Arguments[1]).IsTypeOf<List<object?>>();
        var arr = (List<object?>)parsed.Arguments[1]!;
        await Assert.That(arr.Count).IsEqualTo(1);
        await Assert.That(arr[0]).IsEqualTo(99);
        await Assert.That(parsed.Arguments[2]).IsEqualTo(2);
    }

    [Test]
    public async Task RoundtripArrayMixedTypes()
    {
        var list = new List<object> { 42, 3.14f, "hello", true };
        var msg = new OscMessage("/arr", list);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments.Length).IsEqualTo(1);
        var arr = (List<object?>)parsed.Arguments[0]!;
        await Assert.That(arr.Count).IsEqualTo(4);
        await Assert.That(arr[0]).IsEqualTo(42);
        await Assert.That(arr[1]).IsEqualTo(3.14f);
        await Assert.That(arr[2]).IsEqualTo("hello");
        await Assert.That((bool)arr[3]!).IsTrue();
    }

    [Test]
    public async Task RoundtripArrayAtStart()
    {
        var list = new List<object> { 10, 20 };
        var msg = new OscMessage("/arr", list, "after");
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments.Length).IsEqualTo(2);
        var arr = (List<object?>)parsed.Arguments[0]!;
        await Assert.That(arr[0]).IsEqualTo(10);
        await Assert.That(arr[1]).IsEqualTo(20);
        await Assert.That(parsed.Arguments[1]).IsEqualTo("after");
    }

    [Test]
    public async Task RoundtripArrayAtEnd()
    {
        var list = new List<object> { 10, 20 };
        var msg = new OscMessage("/arr", "before", list);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments.Length).IsEqualTo(2);
        await Assert.That(parsed.Arguments[0]).IsEqualTo("before");
        var arr = (List<object?>)parsed.Arguments[1]!;
        await Assert.That(arr[0]).IsEqualTo(10);
        await Assert.That(arr[1]).IsEqualTo(20);
    }

    #endregion

    #region Address roundtrips

    [Test]
    [Arguments("/a")]
    [Arguments("/test/nested/deep")]
    [Arguments("/1/2/3")]
    [Arguments("/with spaces")]
    public async Task RoundtripAddressVariants(string address)
    {
        var msg = new OscMessage(address, 1);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Address).IsEqualTo(address);
    }

    #endregion

    #region ULong roundtrip (distinct from TimeTag)

    [Test]
    [Arguments(0UL)]
    [Arguments(1UL)]
    [Arguments(ulong.MaxValue)]
    [Arguments(9999999999UL)]
    public async Task RoundtripULong(ulong value)
    {
        // ulong is encoded as TimeTag type 't', so it comes back as TimeTag
        var msg = new OscMessage("/ulong", value);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        var parsedTag = (TimeTag)parsed.Arguments[0]!;
        await Assert.That(parsedTag.Tag).IsEqualTo(value);
    }

    #endregion

    #region Blob padding and sequencing

    [Test]
    [Arguments(1)]
    [Arguments(2)]
    [Arguments(3)]
    [Arguments(4)]
    [Arguments(5)]
    [Arguments(6)]
    [Arguments(7)]
    [Arguments(8)]
    public async Task RoundtripBlobPaddingSizes(int size)
    {
        var blob = new byte[size];
        for (var i = 0; i < size; i++) blob[i] = (byte)(i + 1);

        var msg = new OscMessage("/blob", blob);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That((byte[])parsed.Arguments[0]!).IsEquivalentTo(blob);
    }

    [Test]
    public async Task RoundtripMultipleBlobs()
    {
        var blob1 = new byte[] { 1, 2, 3 };
        var blob2 = new byte[] { 4, 5, 6, 7, 8 };
        var blob3 = new byte[] { 9 };

        var msg = new OscMessage("/blobs", blob1, blob2, blob3);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments.Length).IsEqualTo(3);
        await Assert.That((byte[])parsed.Arguments[0]!).IsEquivalentTo(blob1);
        await Assert.That((byte[])parsed.Arguments[1]!).IsEquivalentTo(blob2);
        await Assert.That((byte[])parsed.Arguments[2]!).IsEquivalentTo(blob3);
    }

    [Test]
    public async Task RoundtripBlobFollowedByString()
    {
        var blob = new byte[] { 1, 2, 3, 4, 5 };
        var msg = new OscMessage("/mixed", blob, "after blob");
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments.Length).IsEqualTo(2);
        await Assert.That((byte[])parsed.Arguments[0]!).IsEquivalentTo(blob);
        await Assert.That(parsed.Arguments[1]).IsEqualTo("after blob");
    }

    [Test]
    public async Task RoundtripStringFollowedByBlob()
    {
        var blob = new byte[] { 10, 20 };
        var msg = new OscMessage("/mixed", "before blob", blob);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments.Length).IsEqualTo(2);
        await Assert.That(parsed.Arguments[0]).IsEqualTo("before blob");
        await Assert.That((byte[])parsed.Arguments[1]!).IsEquivalentTo(blob);
    }

    #endregion

    #region TimeTag.Immediate

    [Test]
    public async Task RoundtripTimeTagImmediate()
    {
        var msg = new OscMessage("/tt", TimeTag.Immediate);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        var parsedTag = (TimeTag)parsed.Arguments[0]!;
        await Assert.That(parsedTag.Tag).IsEqualTo(1UL);
    }

    #endregion

    #region Many arguments stress

    [Test]
    public async Task RoundtripManyArguments()
    {
        var args = new object[50];
        for (var i = 0; i < 50; i++) args[i] = i;

        var msg = new OscMessage("/stress", args);
        var parsed = OscMessage.ParseMessage(msg.GetBytes());

        await Assert.That(parsed.Arguments.Length).IsEqualTo(50);
        for (var i = 0; i < 50; i++)
            await Assert.That(parsed.Arguments[i]).IsEqualTo(i);
    }

    #endregion

    #region Error cases

    [Test]
    public async Task GetBytesThrowsOnUnsupportedType()
    {
        var msg = new OscMessage("/bad", new DateTime(2025, 1, 1));
        Assert.Throws<Exception>(() => msg.GetBytes());
    }

    [Test]
    public async Task GetBytesThrowsOnNestedArrays()
    {
        var inner = new List<object> { 1 };
        var outer = new List<object> { inner };
        var msg = new OscMessage("/bad", outer);

        Assert.Throws<Exception>(() => msg.GetBytes());
    }

    #endregion

    #region GetBytes determinism

    [Test]
    public async Task GetBytesDeterministic()
    {
        var msg = new OscMessage("/test", 42, 3.14f, "hello");
        var bytes1 = msg.GetBytes();
        var bytes2 = msg.GetBytes();

        await Assert.That(bytes1).IsEquivalentTo(bytes2);
    }

    #endregion

    #region GetBytes output alignment

    [Test]
    public async Task GetBytesAlwaysMultipleOf4()
    {
        var messages = new[]
        {
            new OscMessage("/a", 1),
            new OscMessage("/ab", 1),
            new OscMessage("/abc", 1),
            new OscMessage("/abcd", 1),
            new OscMessage("/test", "x"),
            new OscMessage("/test", "xy"),
            new OscMessage("/test", "xyz"),
            new OscMessage("/test", "xyzw"),
            new OscMessage("/test", 1, 2.0f, "hello", new byte[] { 1 }),
        };

        foreach (var msg in messages)
        {
            var bytes = msg.GetBytes();
            await Assert.That(bytes.Length % 4).IsEqualTo(0);
        }
    }

    #endregion
}
