using SASD.LearningManager.Application.Resources;

namespace SASD.LearningManager.Application.Tests;

public sealed class UrlNormalizerTests
{
    [Fact]
    public void Normalize_LowercasesSchemeAndHostAndRemovesFragment()
    {
        var normalizer = new UrlNormalizer();

        var result = normalizer.Normalize("HTTPS://Example.COM:443/course?id=42#chapter");

        Assert.Equal("https://example.com/course?id=42", result);
    }

    [Fact]
    public void Normalize_RetainsQueryParameters()
    {
        var result = new UrlNormalizer().Normalize("https://example.test/course?utm_source=x&id=42");
        Assert.Equal("https://example.test/course?utm_source=x&id=42", result);
    }
}
