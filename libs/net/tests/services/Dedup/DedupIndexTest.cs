using FluentAssertions;
using TNO.Services.Dedup;

namespace TNO.Test.Services.Dedup;

public class DedupIndexTest
{
    private static readonly DateTime PublishedOn = new(2026, 10, 15, 21, 0, 0, DateTimeKind.Utc);
    private static readonly DedupOptions Options = new() { Enabled = true };

    private static DedupEntry Entry(string source, string uid, string headline, string? body = null, int priority = 0, DateTime? publishedOn = null)
    {
        return DedupEntry.Create(source, uid, headline, body, publishedOn ?? PublishedOn, priority, Options);
    }

    [Fact]
    public void TryFindMatch_ExactHeadlineFromAnotherSource()
    {
        var index = new DedupIndex();
        index.Add(Entry("VMS", "1", "B.C. Conservatives remove Vancouver candidate after alleged Hitler posts", priority: 0));

        var found = index.TryFindMatch(Entry("KCN", "2", "B.C. Conservatives remove Vancouver candidate after alleged Hitler posts", priority: 1), Options, out var match);

        found.Should().BeTrue();
        match!.Primary.Source.Should().Be("VMS");
        match.Reason.Should().Be(DedupMatchReason.ExactHeadline);
        match.Score.Should().Be(1);
    }

    [Fact]
    public void TryFindMatch_HeadlineWithSmallEdit()
    {
        var index = new DedupIndex();
        index.Add(Entry("VMS", "1", "Conservatives pledge to scrap new Massey Tunnel, build bridge instead"));

        var found = index.TryFindMatch(Entry("KCN", "2", "UPDATE: Conservatives pledge to scrap Massey Tunnel, build a bridge instead", priority: 1), Options, out var match);

        found.Should().BeTrue();
        match!.Reason.Should().Be(DedupMatchReason.FuzzyHeadline);
        match.Score.Should().BeGreaterThanOrEqualTo(Options.HeadlineThreshold);
    }

    [Fact]
    public void TryFindMatch_DifferentHeadlineSameStory()
    {
        var index = new DedupIndex();
        index.Add(Entry("VMS", "1", "Conservatives pledge to scrap new Massey Tunnel", Story.Original));

        var found = index.TryFindMatch(Entry("KCN", "2", "Rustad promises a bridge to replace aging crossing", Story.Edited, priority: 1), Options, out var match);

        found.Should().BeTrue();
        match!.Reason.Should().Be(DedupMatchReason.Body);
    }

    [Fact]
    public void TryFindMatch_DifferentStoriesDoNotMatch()
    {
        var index = new DedupIndex();
        index.Add(Entry("VMS", "1", "Conservatives pledge to scrap new Massey Tunnel", Story.Original));

        index.TryFindMatch(Entry("KCN", "2", "Valley Road residence being searched by Kelowna RCMP", Story.Unrelated, priority: 1), Options, out var match)
            .Should().BeFalse();
        match.Should().BeNull();
    }

    [Fact]
    public void TryFindMatch_SimilarSeriesHeadlinesDoNotMatch()
    {
        var index = new DedupIndex();
        index.Add(Entry("CVC", "1", "Voter guide for the City of Duncan"));

        index.TryFindMatch(Entry("CVC", "2", "Voter guide for the Town of Lake Cowichan"), Options, out _).Should().BeFalse();
    }

    [Fact]
    public void TryFindMatch_ShortHeadlineNeedsMatchingText()
    {
        var index = new DedupIndex();
        index.Add(Entry("CVC", "1", "Letter: Racing to extinction"));

        index.TryFindMatch(Entry("NAN", "2", "Letter: Racing to extinction", priority: 1), Options, out _).Should().BeFalse();

        index.Add(Entry("CVC", "3", "Letters to the editor", Story.Original));
        index.TryFindMatch(Entry("NAN", "4", "Letters to the editor", Story.Original, priority: 1), Options, out var match).Should().BeTrue();
        match!.Reason.Should().Be(DedupMatchReason.Body);
    }

    [Fact]
    public void TryFindMatch_OutsideWindowDoesNotMatch()
    {
        var index = new DedupIndex();
        index.Add(Entry("VMS", "1", "Conservatives pledge to scrap new Massey Tunnel", publishedOn: PublishedOn.AddDays(-Options.WindowDays - 1)));

        index.TryFindMatch(Entry("KCN", "2", "Conservatives pledge to scrap new Massey Tunnel", priority: 1), Options, out _).Should().BeFalse();
    }

    [Fact]
    public void TryFindMatch_LowerPriorityStoryNeverSkipsHigherPriorityStory()
    {
        var index = new DedupIndex();
        index.Add(Entry("KCN", "1", "Conservatives pledge to scrap new Massey Tunnel", priority: 3));

        index.TryFindMatch(Entry("VMS", "2", "Conservatives pledge to scrap new Massey Tunnel", priority: 0), Options, out _).Should().BeFalse();
        index.TryFindMatch(Entry("PW", "3", "Conservatives pledge to scrap new Massey Tunnel", priority: 3), Options, out _).Should().BeTrue();
    }

    [Fact]
    public void TryFindMatch_StoryNeverMatchesItself()
    {
        var index = new DedupIndex();
        index.Add(Entry("VMS", "1", "Conservatives pledge to scrap new Massey Tunnel"));

        index.TryFindMatch(Entry("VMS", "1", "Conservatives pledge to scrap new Massey Tunnel"), Options, out _).Should().BeFalse();
    }

    [Fact]
    public void TryFindMatch_PrefersHighestScoreThenHighestPriority()
    {
        var index = new DedupIndex();
        index.Add(Entry("PW", "1", "Conservatives pledge to scrap Massey Tunnel, build bridge instead", priority: 2));
        index.Add(Entry("KCN", "2", "Conservatives pledge to scrap new Massey Tunnel, build bridge instead", priority: 1));
        index.Add(Entry("VMS", "3", "Conservatives pledge to scrap new Massey Tunnel, build bridge instead", priority: 0));

        index.TryFindMatch(Entry("NAN", "4", "Conservatives pledge to scrap new Massey Tunnel, build bridge instead", priority: 5), Options, out var match).Should().BeTrue();

        match!.Primary.Source.Should().Be("VMS");
        match.Reason.Should().Be(DedupMatchReason.ExactHeadline);
    }

    [Fact]
    public void Prune_RemovesOldStories()
    {
        var index = new DedupIndex();
        index.Add(Entry("VMS", "1", "Conservatives pledge to scrap new Massey Tunnel", publishedOn: PublishedOn.AddDays(-10)));
        index.Add(Entry("VMS", "2", "Boat on fire near Peachland launch"));

        index.Prune(PublishedOn.AddDays(-7)).Should().Be(1);

        index.Count.Should().Be(1);
        index.TryFindMatch(Entry("KCN", "3", "Conservatives pledge to scrap new Massey Tunnel", priority: 1, publishedOn: PublishedOn.AddDays(-10)), Options, out _).Should().BeFalse();
    }

    [Fact]
    public void Load_ReplacesStoriesAndRecordsVersion()
    {
        var index = new DedupIndex();
        index.Add(Entry("VMS", "1", "Boat on fire near Peachland launch"));

        index.Load(new[] { Entry("VMS", "2", "Conservatives pledge to scrap new Massey Tunnel") }, 5, PublishedOn);

        index.Count.Should().Be(1);
        index.Version.Should().Be(5);
        index.LoadedOn.Should().Be(PublishedOn);
        index.TryFindMatch(Entry("KCN", "3", "Boat on fire near Peachland launch", priority: 1), Options, out _).Should().BeFalse();
    }

    [Fact]
    public void Add_IgnoresStoryAlreadyInIndex()
    {
        var index = new DedupIndex();
        index.Add(Entry("VMS", "1", "Boat on fire near Peachland launch")).Should().BeTrue();
        index.Add(Entry("VMS", "1", "Boat on fire near Peachland launch")).Should().BeFalse();
        index.Count.Should().Be(1);
    }

    [Fact]
    public void MarkSkipped_IsTrueOnlyTheFirstTime()
    {
        var index = new DedupIndex();
        var entry = Entry("KCN", "2", "Boat on fire near Peachland launch");

        index.MarkSkipped(entry).Should().BeTrue();
        index.MarkSkipped(entry).Should().BeFalse();
    }
}
