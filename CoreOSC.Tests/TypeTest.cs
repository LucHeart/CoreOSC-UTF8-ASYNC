namespace LucHeart.CoreOSC.Tests;

/// <summary>
/// Tests for value type structs: Symbol, RGBA, Midi, TimeTag.
/// </summary>
public class TypeTest
{
    #region Symbol

    [Test]
    public async Task SymbolToString()
    {
        var sym = new Symbol("hello");
        await Assert.That(sym.ToString()).IsEqualTo("hello");
    }

    [Test]
    public async Task SymbolEquality()
    {
        var a = new Symbol("test");
        var b = new Symbol("test");
        var c = new Symbol("other");

        await Assert.That(a == b).IsTrue();
        await Assert.That(a != c).IsTrue();
        await Assert.That(a.Equals(b)).IsTrue();
        await Assert.That(a.Equals(c)).IsFalse();
    }

    [Test]
    public async Task SymbolEqualsString()
    {
        var sym = new Symbol("test");
        await Assert.That(sym.Equals("test")).IsTrue();
        await Assert.That(sym.Equals("other")).IsFalse();
    }

    [Test]
    public async Task SymbolGetHashCode()
    {
        var a = new Symbol("test");
        var b = new Symbol("test");

        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }

    [Test]
    public async Task SymbolToBytesRoundtrip()
    {
        var sym = new Symbol("test_symbol");
        var bytes = sym.ToBytes();

        await Assert.That(bytes.Length % 4).IsEqualTo(0);

        var result = OscPacketUtils.GetString(bytes, 0, out _);
        await Assert.That(result).IsEqualTo("test_symbol");
    }

    [Test]
    public async Task SymbolToBytesPadding()
    {
        // Various lengths to check padding
        var lengths = new[] { "", "a", "ab", "abc", "abcd", "abcde" };
        foreach (var val in lengths)
        {
            var sym = new Symbol(val);
            var bytes = sym.ToBytes();
            await Assert.That(bytes.Length % 4).IsEqualTo(0);
        }
    }

    #endregion

    #region RGBA

    [Test]
    public async Task RgbaEquality()
    {
        var a = new RGBA(10, 20, 30, 40);
        var b = new RGBA(10, 20, 30, 40);
        var c = new RGBA(10, 20, 30, 41);

        await Assert.That(a == b).IsTrue();
        await Assert.That(a != c).IsTrue();
        await Assert.That(a.Equals(b)).IsTrue();
        await Assert.That(a.Equals(c)).IsFalse();
    }

    [Test]
    public async Task RgbaEqualsByteArray()
    {
        var rgba = new RGBA(10, 20, 30, 40);
        await Assert.That(rgba.Equals(new byte[] { 10, 20, 30, 40 })).IsTrue();
        await Assert.That(rgba.Equals(new byte[] { 10, 20, 30, 41 })).IsFalse();
    }

    [Test]
    public async Task RgbaGetHashCode()
    {
        var a = new RGBA(10, 20, 30, 40);
        var b = new RGBA(10, 20, 30, 40);

        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }

    [Test]
    public async Task RgbaToBytes()
    {
        var rgba = new RGBA(1, 2, 3, 4);
        var bytes = rgba.ToBytes();

        await Assert.That(bytes).IsEquivalentTo(new byte[] { 1, 2, 3, 4 });
    }

    [Test]
    public async Task RgbaMinMaxValues()
    {
        var min = new RGBA(0, 0, 0, 0);
        var max = new RGBA(255, 255, 255, 255);

        await Assert.That(min.ToBytes()).IsEquivalentTo(new byte[] { 0, 0, 0, 0 });
        await Assert.That(max.ToBytes()).IsEquivalentTo(new byte[] { 255, 255, 255, 255 });
    }

    #endregion

    #region Midi

    [Test]
    public async Task MidiEquality()
    {
        var a = new Midi(1, 144, 60, 127);
        var b = new Midi(1, 144, 60, 127);
        var c = new Midi(1, 144, 60, 0);

        await Assert.That(a == b).IsTrue();
        await Assert.That(a != c).IsTrue();
        await Assert.That(a.Equals(b)).IsTrue();
        await Assert.That(a.Equals(c)).IsFalse();
    }

    [Test]
    public async Task MidiEqualsByteArray()
    {
        var midi = new Midi(1, 144, 60, 127);
        await Assert.That(midi.Equals(new byte[] { 1, 144, 60, 127 })).IsTrue();
        await Assert.That(midi.Equals(new byte[] { 1, 144, 60, 0 })).IsFalse();
    }

    [Test]
    public async Task MidiGetHashCode()
    {
        var a = new Midi(1, 144, 60, 127);
        var b = new Midi(1, 144, 60, 127);

        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }

    [Test]
    public async Task MidiToBytes()
    {
        var midi = new Midi(1, 2, 3, 4);
        var bytes = midi.ToBytes();

        await Assert.That(bytes).IsEquivalentTo(new byte[] { 1, 2, 3, 4 });
    }

    #endregion

    #region TimeTag

    [Test]
    public async Task TimeTagFromUlong()
    {
        var tag = new TimeTag(12345UL);
        await Assert.That(tag.Tag).IsEqualTo(12345UL);
    }

    [Test]
    public async Task TimeTagFromDateTime()
    {
        var dt = DateTime.Parse("2025-06-15T12:00:00Z");
        var tag = new TimeTag(dt);
        var roundtripped = Utils.TimeTagToDateTime(tag.Tag);

        await Assert.That(roundtripped.Date).IsEqualTo(dt.Date);
        await Assert.That(roundtripped.Hour).IsEqualTo(dt.Hour);
        await Assert.That(roundtripped.Minute).IsEqualTo(dt.Minute);
        await Assert.That(roundtripped.Second).IsEqualTo(dt.Second);
    }

    [Test]
    public async Task TimeTagTimestampProperty()
    {
        var dt = DateTime.Parse("2025-01-01T00:00:00Z");
        var tag = new TimeTag(dt);

        await Assert.That(tag.Timestamp.Date).IsEqualTo(dt.Date);
    }

    [Test]
    public async Task TimeTagSecondsRoundtrip()
    {
        var seconds = 123.456;
        var tag = Utils.SecondsToTimeTag(seconds);
        var result = Utils.TimeTagToSeconds(tag);

        await Assert.That(Math.Abs(result - seconds)).IsLessThan(0.001);
    }

    [Test]
    public async Task TimeTagNowValue()
    {
        // Tag value 1 means "now"
        var dt = Utils.TimeTagToDateTime(1);
        await Assert.That(Math.Abs((dt - DateTime.Now).TotalSeconds)).IsLessThan(1.0);
    }

    [Test]
    public async Task TimeTagZero()
    {
        var dt = new DateTime(1900, 1, 1);
        var tag = Utils.DateTimeToTimeTag(dt);
        await Assert.That(tag).IsEqualTo(0UL);
    }

    [Test]
    public async Task TimeTagFraction()
    {
        ulong tag = 0;
        tag |= (ulong)1 << 32; // 1 second
        tag |= 0xFFFFFFFF / 2; // ~500ms

        var fraction = Utils.TimeTagToFraction(tag);
        await Assert.That(Math.Abs(fraction - 0.5)).IsLessThan(0.001);
    }

    [Test]
    public async Task TimeTagImmediateIs1()
    {
        await Assert.That(TimeTag.Immediate.Tag).IsEqualTo(1UL);
    }

    [Test]
    public async Task TimeTagEqualityUlong()
    {
        var tag = new TimeTag(42UL);
        await Assert.That(tag.Equals(42UL)).IsTrue();
        await Assert.That(tag.Equals(43UL)).IsFalse();
    }

    [Test]
    public async Task TimeTagEqualityTimeTag()
    {
        var a = new TimeTag(42UL);
        var b = new TimeTag(42UL);

        await Assert.That(a == b).IsTrue();
        await Assert.That(a != b).IsFalse();
    }

    [Test]
    public async Task TimeTagSecondsProperty()
    {
        var tag = new TimeTag(0UL);
        tag.Seconds = 100.0;

        await Assert.That(Math.Abs(tag.Seconds - 100.0)).IsLessThan(0.001);
    }

    [Test]
    public async Task TimeTagFractionProperty()
    {
        var tag = new TimeTag(0UL);
        tag.Fraction = 0.5;

        await Assert.That(Math.Abs(tag.Fraction - 0.5)).IsLessThan(0.001);
    }

    #endregion
}
