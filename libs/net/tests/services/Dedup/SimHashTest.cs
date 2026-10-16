using FluentAssertions;
using TNO.Services.Dedup;

namespace TNO.Test.Services.Dedup;

public class SimHashTest
{
    /// <summary>
    /// Fingerprints are stored in content references, so the hash must never change between processes or releases.
    /// The expected values were computed independently.
    /// </summary>
    [Theory]
    [InlineData("", 0xefd01f60ba992926UL)]
    [InlineData("massey tunnel bridge", 0x2979e06756e49602UL)]
    [InlineData("b c conservatives remove", 0x555cd6e51d297ad8UL)]
    public void Hash_IsStable(string value, ulong expected)
    {
        SimHash.Hash(value).Should().Be(expected);
    }

    [Fact]
    public void Compute_IdenticalTextHasIdenticalFingerprint()
    {
        var tokens = TextNormalizer.TextTokens(Story.Original);
        SimHash.Compute(tokens).Should().Be(SimHash.Compute(TextNormalizer.TextTokens(Story.Original)));
        SimHash.Similarity(SimHash.Compute(tokens), SimHash.Compute(tokens)).Should().Be(1);
    }

    [Fact]
    public void Compute_MarkupDoesNotChangeFingerprint()
    {
        var plain = SimHash.Compute(TextNormalizer.TextTokens(Story.Original));
        var html = SimHash.Compute(TextNormalizer.TextTokens($"<p>{Story.Original.Replace(". ", ".</p><p>")}</p>"));
        html.Should().Be(plain);
    }

    [Fact]
    public void Similarity_SmallEditIsAboveDefaultThreshold()
    {
        var original = SimHash.Compute(TextNormalizer.TextTokens(Story.Original));
        var edited = SimHash.Compute(TextNormalizer.TextTokens(Story.Edited));
        SimHash.Similarity(original, edited).Should().BeGreaterThanOrEqualTo(new DedupOptions().BodyThreshold);
    }

    [Fact]
    public void Similarity_UnrelatedTextIsBelowDefaultThreshold()
    {
        var original = SimHash.Compute(TextNormalizer.TextTokens(Story.Original));
        var unrelated = SimHash.Compute(TextNormalizer.TextTokens(Story.Unrelated));
        SimHash.Similarity(original, unrelated).Should().BeLessThan(new DedupOptions().BodyThreshold);
    }

    [Fact]
    public void Shingles_ShortTextIsOneFeature()
    {
        SimHash.Shingles(new[] { "two", "words" }).Should().Equal("two words");
        SimHash.Shingles(new[] { "a", "b", "c", "d" }).Should().Equal("a b c", "b c d");
        SimHash.Shingles(Array.Empty<string>()).Should().BeEmpty();
    }
}
