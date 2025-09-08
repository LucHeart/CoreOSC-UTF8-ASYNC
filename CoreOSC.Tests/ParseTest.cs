namespace LucHeart.CoreOSC.Tests;

public class ParseTest
{
    [Test]
    public async Task TestDouble()
    {
        var val = 1234567.2324521e36;

        var msg = new OscMessage("/test/1", val);
        var bytes = msg.GetBytes();

        var msg2 = OscMessage.ParseMessage(bytes);
        await Verify(msg2);
    }

    [Test]
    public async Task TestBlob()
    {
        var blob = new byte[] { 23, 65, 255, 12, 6 };

        var msg = new OscMessage("/test/1", blob);
        var bytes = msg.GetBytes();

        var msg2 = OscMessage.ParseMessage(bytes);
        await Verify(msg2);
    }

    [Test]
    public async Task TestTimetag()
    {
        var val = DateTime.MaxValue;
        var tag = new TimeTag(val);

        var msg = new OscMessage("/test/1", tag);
        var bytes = msg.GetBytes();

        var msg2 = OscMessage.ParseMessage(bytes);
        await Verify(msg2);
    }

    [Test]
    public async Task TestLong()
    {
        var num = 123456789012345;
        var msg = new OscMessage("/test/1", num);
        var bytes = msg.GetBytes();

        var msg2 = OscMessage.ParseMessage(bytes);

        await Verify(msg2);
    }

    [Test]
    public async Task TestArray()
    {
        var list = new List<object> { 23, true, "hello world" };
        var msg = new OscMessage("/test/1", 9999, list, 24.24f);
        var bytes = msg.GetBytes();

        var msg2 = OscMessage.ParseMessage(bytes);

        await Verify(msg2);
    }

    [Test]
    public async Task TestNoAddress()
    {
        var msg = new OscMessage("", 9999, 24.24f);
        var bytes = msg.GetBytes();

        var msg2 = OscMessage.ParseMessage(bytes);

        await Verify(msg2);
    }

    [Test]
    public async Task TestStringPadding()
    {
        var msg = new OscMessage(
            "/test/strings",
            "",
            "1",
            "12",
            "123",
            "1234",
            "12345",
            "1234",
            "123",
            "12",
            "1",
            "");
        var bytes = msg.GetBytes();

        var msg2 = OscMessage.ParseMessage(bytes);

        await Verify(msg2);
    }
}