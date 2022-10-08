namespace AuraImageVariants.Tests;

[TestClass]
public class ResizingTests
{
    [TestMethod]
    public void TestResizeSameSize_Complete()
    {
        Size original = new(256, 128);
        var resized = Functions.GetResizedDimensions(original, 256, 128);
        
        resized.Width.Should().Be(original.Width);
        resized.Height.Should().Be(original.Height);
    }

    [TestMethod]
    public void TestResizeSameSize_IncompleteWidth()
    {
        Size original = new(256, 128);
        var resized = Functions.GetResizedDimensions(original, null, 128);

        resized.Width.Should().Be(original.Width);
        resized.Height.Should().Be(original.Height);
    }

    [TestMethod]
    public void TestResizeSameSize_IncompleteHeight()
    {
        Size original = new(256, 128);
        var resized = Functions.GetResizedDimensions(original, 256, null);

        resized.Width.Should().Be(original.Width);
        resized.Height.Should().Be(original.Height);
    }

    [TestMethod]
    public void TestResizeLarger_Complete()
    {
        Size original = new(256, 128);
        var resized = Functions.GetResizedDimensions(original, 500, 482);

        resized.Width.Should().Be(original.Width);
        resized.Height.Should().Be(original.Height);
    }

    [TestMethod]
    public void TestResizeLarger_IncompleteWidth()
    {
        Size original = new(256, 128);
        var resized = Functions.GetResizedDimensions(original, null, 482);

        resized.Width.Should().Be(original.Width);
        resized.Height.Should().Be(original.Height);
    }

    [TestMethod]
    public void TestResizeLarger_IncompleteHeight()
    {
        Size original = new(256, 128);
        var resized = Functions.GetResizedDimensions(original, 500, null);

        resized.Width.Should().Be(original.Width);
        resized.Height.Should().Be(original.Height);
    }

    [TestMethod]
    public void TestResize_Square()
    {
        Size original = new(256, 128);
        var resized = Functions.GetResizedDimensions(original, 128, 128);

        resized.Width.Should().Be(128);
        resized.Height.Should().Be(128);
    }

    [TestMethod]
    public void TestResize_PreserveWidthAspect()
    {
        Size original = new(256, 128);
        var resized = Functions.GetResizedDimensions(original, 100, null);

        resized.Width.Should().Be(100);
        resized.Height.Should().Be(50);
    }

    [TestMethod]
    public void TestResize_PreserveHeightAspect()
    {
        Size original = new(256, 128);
        var resized = Functions.GetResizedDimensions(original, null, 100);

        resized.Width.Should().Be(200);
        resized.Height.Should().Be(100);
    }

    [TestMethod]
    public void TestResize_ResizeToZero()
    {
        Size original = new(256, 128);
        var resized = Functions.GetResizedDimensions(original, 0, null);

        resized.Width.Should().Be(original.Width);
        resized.Height.Should().Be(original.Height);
    }
}