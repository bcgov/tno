using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TNO.Core.Extensions;
using TNO.DAL.Scoring;
using TNO.Entities;

namespace TNO.DAL;

/// <summary>
/// TNOContext topic score recalculation. Calculated (non-overridden) topic scores follow their
/// inputs: whenever a save changes a content's topics, source, series, section, page, attached
/// images, body, or publish time, its calculated scores are recalculated in the same save.
/// </summary>
public partial class TNOContext
{
    #region Variables
    /// <summary>
    /// The content properties topic score rules read.
    /// </summary>
    private static readonly string[] _scoringProperties = new[]
    {
        nameof(Content.SourceId),
        nameof(Content.SeriesId),
        nameof(Content.Section),
        nameof(Content.Page),
        nameof(Content.Body),
        nameof(Content.PublishedOn),
        nameof(Content.ContentType),
    };
    #endregion

    #region Methods
    /// <summary>
    /// Whether a file is an image, for the "has image" rule condition.
    /// </summary>
    /// <param name="contentType"></param>
    /// <returns></returns>
    public static bool IsImageFile(string? contentType)
    {
        return contentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true;
    }

    /// <summary>
    /// Recalculate the calculated topic scores of every content whose scoring inputs this save
    /// changes. Overridden scores are never touched.
    /// </summary>
    private void RecalculateTopicScores()
    {
        var entries = ChangeTracker.Entries().ToArray();

        // Content being added is scored through its navigation (its key is not known yet).
        var addedContent = entries
            .Where(e => e.State == EntityState.Added && e.Entity is Content)
            .Select(e => (Content)e.Entity)
            .ToList();

        var contentIds = new HashSet<long>();
        foreach (var entry in entries)
        {
            switch (entry.Entity)
            {
                case Content content when entry.State == EntityState.Modified
                    && _scoringProperties.Any(p => entry.Property(p).IsModified):
                    contentIds.Add(content.Id);
                    break;
                case ContentTopic topic when entry.State == EntityState.Added
                    || (entry.State == EntityState.Modified && entry.Property(nameof(ContentTopic.IsScoreOverridden)).IsModified):
                    if (topic.ContentId != 0) contentIds.Add(topic.ContentId);
                    break;
                case FileReference file when entry.State == EntityState.Added || entry.State == EntityState.Deleted
                    || (entry.State == EntityState.Modified && entry.Property(nameof(FileReference.ContentType)).IsModified):
                    if (file.ContentId != 0) contentIds.Add(file.ContentId);
                    break;
            }
        }
        if (addedContent.Count == 0 && contentIds.Count == 0) return;

        var timeZone = DateTimeExtensions.ResolveTimeZone(_topicScoreOptions.TimeZone);
        var sources = new Dictionary<int, (bool UseInTopics, int? DefaultScore, TopicScoreRule[] Rules)>();
        (bool UseInTopics, int? DefaultScore, TopicScoreRule[] Rules) GetSource(int sourceId)
        {
            if (!sources.TryGetValue(sourceId, out var source))
            {
                var values = this.Sources.AsNoTracking().Where(s => s.Id == sourceId)
                    .Select(s => new { s.UseInTopics, s.TopicDefaultScore }).FirstOrDefault();
                var rules = this.TopicScoreRules.AsNoTracking().Where(r => r.SourceId == sourceId).ToArray();
                source = (values?.UseInTopics ?? false, values?.TopicDefaultScore, rules);
                sources[sourceId] = source;
            }
            return source;
        }
        bool SeriesUsesTopics(int? seriesId) => seriesId.HasValue
            && this.Series.AsNoTracking().Any(s => s.Id == seriesId && s.UseInTopics);

        foreach (var content in addedContent)
        {
            var hasImageFile = content.FileReferences.Any(f => IsImageFile(f.ContentType));
            ScoreTopics(content, content.TopicsManyToMany.Where(t => Entry(t).State != EntityState.Deleted), hasImageFile, GetSource, SeriesUsesTopics, timeZone);
        }

        contentIds.ExceptWith(addedContent.Select(c => c.Id).Where(id => id != 0));
        foreach (var contentId in contentIds)
        {
            var content = entries.Select(e => e.Entity).OfType<Content>().FirstOrDefault(c => c.Id == contentId)
                ?? this.Contents.AsNoTracking().FirstOrDefault(c => c.Id == contentId);
            if (content == null) continue;

            // Saved topics and files, adjusted by what this save adds and deletes.
            var topics = this.ContentTopics.Where(t => t.ContentId == contentId).ToList()
                .Concat(ChangeTracker.Entries<ContentTopic>().Where(e => e.State == EntityState.Added && e.Entity.ContentId == contentId).Select(e => e.Entity))
                .Where(t => Entry(t).State != EntityState.Deleted)
                .Distinct()
                .ToArray();
            if (topics.Length == 0) continue;

            var deletedFileIds = ChangeTracker.Entries<FileReference>().Where(e => e.State == EntityState.Deleted && e.Entity.ContentId == contentId).Select(e => e.Entity.Id).ToHashSet();
            var hasImageFile = this.FileReferences.AsNoTracking().Any(f => f.ContentId == contentId && f.ContentType.StartsWith("image/") && !deletedFileIds.Contains(f.Id))
                || ChangeTracker.Entries<FileReference>().Any(e => (e.State == EntityState.Added || e.State == EntityState.Modified) && e.Entity.ContentId == contentId && IsImageFile(e.Entity.ContentType));

            ScoreTopics(content, topics, hasImageFile, GetSource, SeriesUsesTopics, timeZone);
        }
    }

    /// <summary>
    /// Apply the calculated score to each non-overridden topic of eligible content.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="topics"></param>
    /// <param name="hasImageFile"></param>
    /// <param name="getSource"></param>
    /// <param name="seriesUsesTopics"></param>
    /// <param name="timeZone"></param>
    private static void ScoreTopics(
        Content content,
        IEnumerable<ContentTopic> topics,
        bool hasImageFile,
        Func<int, (bool UseInTopics, int? DefaultScore, TopicScoreRule[] Rules)> getSource,
        Func<int?, bool> seriesUsesTopics,
        TimeZoneInfo timeZone)
    {
        var calculated = topics.Where(t => !t.IsScoreOverridden).ToArray();
        if (calculated.Length == 0 || !content.SourceId.HasValue) return;

        var source = getSource(content.SourceId.Value);
        if (!TopicScoreCalculator.IsEligible(content.ContentType, source.UseInTopics, seriesUsesTopics(content.SeriesId))) return;

        var input = TopicScoreInput.From(content.SourceId, content.SeriesId, content.Section, content.Page, content.Body, content.PublishedOn, hasImageFile);
        var result = TopicScoreCalculator.Calculate(source.Rules, source.DefaultScore, input, timeZone);
        foreach (var topic in calculated)
        {
            if (topic.Score != result.Score) topic.Score = result.Score;
            if (topic.ScoreRuleId != result.RuleId) topic.ScoreRuleId = result.RuleId;
        }
    }
    #endregion
}
