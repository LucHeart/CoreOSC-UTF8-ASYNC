namespace LucHeart.CoreOSC.Tests;

/// <summary>
/// Unit tests for OscBundle serialization, parsing, and IsBundle detection.
/// </summary>
public class BundleTest
{
    #region IsBundle

    [Test]
    public async Task IsBundleTrue()
    {
        var bundle = new OscBundle(1, new OscMessage("/test", 1));
        var bytes = bundle.GetBytes();

        await Assert.That(OscBundle.IsBundle(bytes)).IsTrue();
    }

    [Test]
    public async Task IsBundleFalseForMessage()
    {
        var msg = new OscMessage("/test", 1);
        var bytes = msg.GetBytes();

        await Assert.That(OscBundle.IsBundle(bytes)).IsFalse();
    }

    [Test]
    public async Task IsBundleFalseForShortData()
    {
        await Assert.That(OscBundle.IsBundle(new byte[] { 1, 2, 3 })).IsFalse();
    }

    [Test]
    public async Task IsBundleFalseForEmpty()
    {
        await Assert.That(OscBundle.IsBundle(ReadOnlySpan<byte>.Empty)).IsFalse();
    }

    #endregion

    #region Roundtrip

    [Test]
    public async Task BundleSingleMessage()
    {
        var msg = new OscMessage("/test", 42, "hello");
        var bundle = new OscBundle(1, msg);

        var bytes = bundle.GetBytes();
        var parsed = OscBundle.ParseBundle(bytes);

        await Assert.That(parsed.Messages.Count).IsEqualTo(1);
        await Assert.That(parsed.Messages[0].Address).IsEqualTo("/test");
        await Assert.That(parsed.Messages[0].Arguments[0]).IsEqualTo(42);
        await Assert.That(parsed.Messages[0].Arguments[1]).IsEqualTo("hello");
    }

    [Test]
    public async Task BundleMultipleMessages()
    {
        var msg1 = new OscMessage("/a", 1);
        var msg2 = new OscMessage("/b", 2.0f);
        var msg3 = new OscMessage("/c", "three");
        var bundle = new OscBundle(1, msg1, msg2, msg3);

        var bytes = bundle.GetBytes();
        var parsed = OscBundle.ParseBundle(bytes);

        await Assert.That(parsed.Messages.Count).IsEqualTo(3);
        await Assert.That(parsed.Messages[0].Address).IsEqualTo("/a");
        await Assert.That(parsed.Messages[0].Arguments[0]).IsEqualTo(1);
        await Assert.That(parsed.Messages[1].Address).IsEqualTo("/b");
        await Assert.That(parsed.Messages[1].Arguments[0]).IsEqualTo(2.0f);
        await Assert.That(parsed.Messages[2].Address).IsEqualTo("/c");
        await Assert.That(parsed.Messages[2].Arguments[0]).IsEqualTo("three");
    }

    [Test]
    public async Task BundleTimeTagPreserved()
    {
        var dt = DateTime.Parse("2025-06-15T12:00:00Z");
        var tag = Utils.DateTimeToTimeTag(dt);
        var bundle = new OscBundle(tag, new OscMessage("/test", 1));

        var bytes = bundle.GetBytes();
        var parsed = OscBundle.ParseBundle(bytes);

        await Assert.That(parsed.TimeTag).IsEqualTo(tag);
    }

    [Test]
    public async Task BundleComplexMessages()
    {
        var msg1 = new OscMessage("/complex",
            42, 3.14f, "test", new byte[] { 1, 2, 3 },
            99999L, true, false, null);
        var msg2 = new OscMessage("/simple", 1);
        var bundle = new OscBundle(1, msg1, msg2);

        var bytes = bundle.GetBytes();
        var parsed = OscBundle.ParseBundle(bytes);

        await Assert.That(parsed.Messages.Count).IsEqualTo(2);
        await Assert.That(parsed.Messages[0].Arguments.Length).IsEqualTo(8);
        await Assert.That(parsed.Messages[0].Arguments[0]).IsEqualTo(42);
        await Assert.That(parsed.Messages[0].Arguments[1]).IsEqualTo(3.14f);
        await Assert.That(parsed.Messages[0].Arguments[2]).IsEqualTo("test");
        await Assert.That((byte[])parsed.Messages[0].Arguments[3]!).IsEquivalentTo(new byte[] { 1, 2, 3 });
        await Assert.That(parsed.Messages[0].Arguments[4]).IsEqualTo(99999L);
        await Assert.That((bool)parsed.Messages[0].Arguments[5]!).IsTrue();
        await Assert.That((bool)parsed.Messages[0].Arguments[6]!).IsFalse();
        await Assert.That(parsed.Messages[0].Arguments[7]).IsNull();
    }

    [Test]
    public async Task BundleGetBytesDeterministic()
    {
        var bundle = new OscBundle(1,
            new OscMessage("/a", 1, "hello"),
            new OscMessage("/b", 2.0f));

        var bytes1 = bundle.GetBytes();
        var bytes2 = bundle.GetBytes();

        await Assert.That(bytes1).IsEquivalentTo(bytes2);
    }

    #endregion

    [Test]
    public async Task BundleManyMessages()
    {
        var messages = new OscMessage[20];
        for (var i = 0; i < 20; i++)
            messages[i] = new OscMessage($"/msg/{i}", i, $"val{i}");

        var bundle = new OscBundle(1, messages);
        var bytes = bundle.GetBytes();
        var parsed = OscBundle.ParseBundle(bytes);

        await Assert.That(parsed.Messages.Count).IsEqualTo(20);
        for (var i = 0; i < 20; i++)
        {
            await Assert.That(parsed.Messages[i].Address).IsEqualTo($"/msg/{i}");
            await Assert.That(parsed.Messages[i].Arguments[0]).IsEqualTo(i);
            await Assert.That(parsed.Messages[i].Arguments[1]).IsEqualTo($"val{i}");
        }
    }

    [Test]
    public async Task BundleTimestampPropertyRoundtrip()
    {
        var dt = DateTime.Parse("2025-03-15T08:30:00Z");
        var bundle = new OscBundle(0, new OscMessage("/test", 1));
        bundle.Timestamp = dt;

        var bytes = bundle.GetBytes();
        var parsed = OscBundle.ParseBundle(bytes);
        var parsedDt = Utils.TimeTagToDateTime(parsed.TimeTag);

        await Assert.That(parsedDt.Date).IsEqualTo(dt.Date);
        await Assert.That(parsedDt.Hour).IsEqualTo(dt.Hour);
        await Assert.That(parsedDt.Minute).IsEqualTo(dt.Minute);
        await Assert.That(parsedDt.Second).IsEqualTo(dt.Second);
    }

    [Test]
    public async Task BundleGetBytesAlwaysMultipleOf4()
    {
        var bundle = new OscBundle(1,
            new OscMessage("/a", 1),
            new OscMessage("/ab", "x"),
            new OscMessage("/abc", 1, 2.0f, "hi"));

        var bytes = bundle.GetBytes();
        await Assert.That(bytes.Length % 4).IsEqualTo(0);
    }

    #region Error cases

    [Test]
    public async Task ParseBundleThrowsOnNonBundle()
    {
        var msg = new OscMessage("/test", 1);
        var bytes = msg.GetBytes();

        Assert.Throws<Exception>(() => OscBundle.ParseBundle(bytes));
    }

    #endregion
}
