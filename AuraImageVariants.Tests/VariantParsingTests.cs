namespace AuraImageVariants.Tests;

[TestClass]
public class VariantParsingTests
{
    [TestMethod]
    public void TestParseNothing()
    {
        var variants = Functions.Parse(string.Empty);
        variants.Should().BeEmpty();
    }

    [TestMethod]
    public void TestParseSingle_ValidFull()
    {
        var variants = Functions.Parse("name=small,w=96,h=64");
        variants.Should().ContainSingle();
        var variant = variants[0];
        variant.Should().NotBeNull();
        variant.Name.Should().Be("small");
        variant.Width.Should().Be(96);
        variant.Height.Should().Be(64);
    }

    [TestMethod]
    public void TestParseSingle_ValidNoHeight()
    {
        var variants = Functions.Parse("name=small,w=96");
        variants.Should().ContainSingle();
        var variant = variants[0];
        variant.Should().NotBeNull();
        variant.Name.Should().Be("small");
        variant.Width.Should().Be(96);
        variant.Height.Should().BeNull();
    }

    [TestMethod]
    public void TestParseSingle_ValidNoWidth()
    {
        var variants = Functions.Parse("name=small,h=64");
        variants.Should().ContainSingle();
        var variant = variants[0];
        variant.Should().NotBeNull();
        variant.Name.Should().Be("small");
        variant.Width.Should().BeNull();
        variant.Height.Should().Be(64);
    }

    [TestMethod]
    public void TestParseSingle_ValidNoName()
    {
        VariantInfo defaultVariant = new();

        var variants = Functions.Parse("w=96,h=64");
        variants.Should().ContainSingle();
        var variant = variants[0];
        variant.Should().NotBeNull();
        variant.Name.Should().Be(defaultVariant.Name);
        variant.Width.Should().Be(96);
        variant.Height.Should().Be(64);
    }

    [TestMethod]
    public void TestParseSingle_ValidMultipleTerminatorsAndSeparators()
    {
        var variants = Functions.Parse("name=small,,,,w=96,,,,,,,,,h=64;;;");
        variants.Should().ContainSingle();
        var variant = variants[0];
        variant.Should().NotBeNull();
        variant.Name.Should().Be("small");
        variant.Width.Should().Be(96);
        variant.Height.Should().Be(64);
    }

    [TestMethod]
    public void TestParseSingle_ValidMultiplePropertyAssignmentSymbols()
    {
        VariantInfo defaultVariant = new();

        var variants = Functions.Parse("name==small,w=96,h=64;");
        variants.Should().ContainSingle();
        var variant = variants[0];
        variant.Should().NotBeNull();
        variant.Name.Should().Be(defaultVariant.Name);
        variant.Width.Should().Be(96);
        variant.Height.Should().Be(64);
    }

    [TestMethod]
    public void TestParseSingle_ValidMultipleProperties()
    {
        // It will always pick the most recent property to assign to.
        var variants = Functions.Parse("name=small,name=medium,w=96,h=64,h=40");
        variants.Should().ContainSingle();
        var variant = variants[0];
        variant.Should().NotBeNull();
        variant.Name.Should().Be("medium");
        variant.Width.Should().Be(96);
        variant.Height.Should().Be(40);
    }

    [TestMethod]
    public void TestParseSingle_ValidMultipleEquivalentNames()
    {
        // It will always use the first variant if there is a name property match.
        var variants = Functions.Parse("name=small,w=96,h=64;name=small,w=10,h=40");
        variants.Should().ContainSingle();
        var variant = variants[0];
        variant.Should().NotBeNull();
        variant.Name.Should().Be("small");
        variant.Width.Should().Be(96);
        variant.Height.Should().Be(64);
    }

    [TestMethod]
    public void TestParseMultiple_WithSpaces()
    {
        var variants = Functions.Parse("nam e=smal  l, w=96 ,   h= 64 ; name=large, w=5 00,  h=  500 ");
        variants.Should().HaveCount(2);

        var variantA = variants[0];
        variantA.Should().NotBeNull();
        variantA.Name.Should().Be("small");
        variantA.Width.Should().Be(96);
        variantA.Height.Should().Be(64);

        var variantB = variants[1];
        variantB.Should().NotBeNull();
        variantB.Name.Should().Be("large");
        variantB.Width.Should().Be(500);
        variantB.Height.Should().Be(500);
    }
}