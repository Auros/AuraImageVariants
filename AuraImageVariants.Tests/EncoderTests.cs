namespace AuraImageVariants.Tests;

[TestClass]
public class EncoderTests
{
    [TestMethod]
    public void TestGetPng()
    {
        var encoder = Functions.GetEncoder("png");
        encoder.Should().NotBeNull();
    }

    [TestMethod]
    public void TestGetPng_WrongCase()
    {
        var encoder = Functions.GetEncoder("Png");
        encoder.Should().BeNull();
    }

    [TestMethod]
    public void TestGetJpg()
    {
        var encoder = Functions.GetEncoder("jpg");
        encoder.Should().NotBeNull();
    }

    [TestMethod]
    public void TestGetJpeg()
    {
        var encoder = Functions.GetEncoder("jpeg");
        encoder.Should().NotBeNull();
    }

    [TestMethod]
    public void TestGetGif()
    {
        var encoder = Functions.GetEncoder("gif");
        encoder.Should().NotBeNull();
    }

    [TestMethod]
    public void TestGetWebp()
    {
        var encoder = Functions.GetEncoder("webp");
        encoder.Should().NotBeNull();
    }

    [TestMethod]
    public void TestGetUnknown()
    {
        var encoder = Functions.GetEncoder("encoderthatdoesntexist");
        encoder.Should().BeNull();
    }

}
