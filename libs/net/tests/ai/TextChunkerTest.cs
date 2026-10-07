using FluentAssertions;
using TNO.AI.Text;
using TNO.AI.Tokens;

namespace TNO.Test.AI;

/// <summary>
/// Tests for splitting text into budget-sized chunks.
/// </summary>
public class TextChunkerTest
{
    private readonly ITokenEstimator _estimator = new HeuristicTokenEstimator();

    private static string Words(string text) => String.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    [Fact]
    public void ShortTextIsOneChunk()
    {
        var chunks = TextChunker.Split("A short paragraph.", 100, _estimator);

        chunks.Should().ContainSingle().Which.Text.Should().Be("A short paragraph.");
    }

    [Fact]
    public void SplitsOnParagraphsWithoutLosingText()
    {
        var paragraphs = Enumerable.Range(1, 20).Select(i => $"Paragraph {i} " + String.Join(" ", Enumerable.Repeat("text", 30)) + ".").ToArray();
        var text = String.Join("\n\n", paragraphs);

        var chunks = TextChunker.Split(text, 100, _estimator);

        chunks.Should().HaveCountGreaterThan(1);
        chunks.Should().OnlyContain(c => _estimator.Count(c.Text) <= 100);
        Words(String.Join(" ", chunks.Select(c => c.Text))).Should().Be(Words(text));
        chunks.Should().OnlyContain(c => text.Substring(c.Start, c.Length) == c.Text);
    }

    [Fact]
    public void SplitsTextWithoutSentenceBreaks()
    {
        var text = String.Join(" ", Enumerable.Range(1, 5000).Select(i => $"w{i}"));

        var chunks = TextChunker.Split(text, 200, _estimator);

        chunks.Should().OnlyContain(c => _estimator.Count(c.Text) <= 200);
        Words(String.Join(" ", chunks.Select(c => c.Text))).Should().Be(text);
    }

    [Fact]
    public void SplitsASingleUnbrokenRun()
    {
        var text = new string('x', 5000);

        var chunks = TextChunker.Split(text, 100, _estimator);

        chunks.Should().OnlyContain(c => _estimator.Count(c.Text) <= 100);
        String.Concat(chunks.Select(c => c.Text)).Should().Be(text);
    }

    [Fact]
    public void OverlapRepeatsPreviousContext()
    {
        var sentences = Enumerable.Range(1, 40).Select(i => $"Sentence number {i} is here.").ToArray();
        var text = String.Join(" ", sentences);

        var chunks = TextChunker.Split(text, 60, _estimator, 15);

        chunks.Should().HaveCountGreaterThan(1);
        chunks.Skip(1).Should().OnlyContain(c => c.OverlapLength > 0);
        chunks.Should().OnlyContain(c => _estimator.Count(c.Text) <= 60);
        // Without the overlap, the chunks cover the text exactly once.
        Words(String.Join(" ", chunks.Select(c => c.Text[c.OverlapLength..]))).Should().Be(Words(text));
    }

    [Theory]
    [InlineData(TokenEstimationStrategy.O200kBase)]
    [InlineData(TokenEstimationStrategy.Cl100kBase)]
    [InlineData(TokenEstimationStrategy.Heuristic)]
    [InlineData(null)]
    public void EstimatorsCountTokens(string? strategy)
    {
        var estimator = TokenEstimator.Create(strategy);

        estimator.Count("The premier announced a new transit plan today.").Should().BeInRange(5, 20);
        estimator.Count("").Should().Be(0);
    }
}
