using FluentAssertions;
using TNO.Services.Dedup;

namespace TNO.Test.Services.Dedup;

public class TextNormalizerTest
{
    [Fact]
    public void NormalizeHeadline_RemovesMarkupEntitiesPunctuationAndCase()
    {
        TextNormalizer.NormalizeHeadline("<b>Attempted theft of an ATM doesn&#8217;t pay out, for thieves!</b>")
            .Should().Be("attempted theft of an atm doesnt pay out for thieves");
    }

    [Fact]
    public void NormalizeHeadline_CurlyAndStraightApostrophesMatch()
    {
        TextNormalizer.NormalizeHeadline("Ecoforest looks to ‘burn the mortgage’")
            .Should().Be(TextNormalizer.NormalizeHeadline("Ecoforest looks to 'burn the mortgage'"));
    }

    [Fact]
    public void NormalizeHeadline_DecodesDoubleEncodedEntities()
    {
        TextNormalizer.NormalizeHeadline("Loss of control led to crash &amp;#8211; TSB")
            .Should().Be("loss of control led to crash tsb");
    }

    [Fact]
    public void NormalizeHeadline_RemovesAccents()
    {
        TextNormalizer.NormalizeHeadline("Café opens in Québec").Should().Be("cafe opens in quebec");
    }

    [Theory]
    [InlineData("UPDATE: Valley Road residence being searched")]
    [InlineData("Updated : Valley Road residence being searched")]
    [InlineData("VIDEO: Valley Road residence being searched")]
    [InlineData("Valley Road residence being searched")]
    public void NormalizeHeadline_RemovesLeadingLabel(string headline)
    {
        TextNormalizer.NormalizeHeadline(headline).Should().Be("valley road residence being searched");
    }

    [Fact]
    public void HeadlineTokens_DropsStopWordsAndSingleCharacters()
    {
        var tokens = TextNormalizer.HeadlineTokens(TextNormalizer.NormalizeHeadline("B.C. Conservatives remove a candidate from the race"));
        tokens.Should().BeEquivalentTo(new[] { "conservatives", "remove", "candidate", "race" });
    }

    [Fact]
    public void Jaccard_ReturnsSharedWordsOverAllWords()
    {
        var first = new HashSet<string> { "massey", "tunnel", "bridge", "new" };
        var second = new HashSet<string> { "massey", "tunnel", "bridge" };
        TextNormalizer.Jaccard(first, second).Should().Be(0.75);
        TextNormalizer.Jaccard(first, new HashSet<string>()).Should().Be(0);
    }

    [Fact]
    public void TextTokens_ConvertsHtmlToWords()
    {
        TextNormalizer.TextTokens("<p>The <strong>mayor</strong> said &quot;no&quot;.</p><p>Council agreed.</p>")
            .Should().Equal("the", "mayor", "said", "no", "council", "agreed");
    }
}
