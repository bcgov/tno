using System.Text.RegularExpressions;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.DAL.Scoring;

/// <summary>
/// TopicScoreInput record, the content values topic score rules are evaluated against.
/// </summary>
/// <param name="SourceId">The content source.</param>
/// <param name="SeriesId">The content series.</param>
/// <param name="Section">The print section.</param>
/// <param name="Page">The print page, a prefix plus a number ("A1").</param>
/// <param name="HasImage">Whether the content has an attached image file or an image in its body.</param>
/// <param name="PublishedOn">The publication date and time (UTC).</param>
/// <param name="CharacterCount">The length of the body's plain text.</param>
public record TopicScoreInput(
    int? SourceId,
    int? SeriesId,
    string? Section,
    string? Page,
    bool HasImage,
    DateTime? PublishedOn,
    int CharacterCount)
{
    /// <summary>
    /// Build the scoring input for the specified content values.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="seriesId"></param>
    /// <param name="section"></param>
    /// <param name="page"></param>
    /// <param name="body"></param>
    /// <param name="publishedOn"></param>
    /// <param name="hasImageFile">Whether an image file is attached to the content.</param>
    /// <returns></returns>
    public static TopicScoreInput From(int? sourceId, int? seriesId, string? section, string? page, string? body, DateTime? publishedOn, bool hasImageFile)
    {
        return new TopicScoreInput(
            sourceId,
            seriesId,
            section,
            page,
            hasImageFile || body.ContainsHtmlImage(),
            publishedOn,
            body.HtmlToPlainText().Length);
    }
}

/// <summary>
/// TopicScoreCondition enum, the conditions a topic score rule can set, in evaluation order.
/// </summary>
public enum TopicScoreCondition
{
    /// <summary>The content series.</summary>
    Series,
    /// <summary>The print section.</summary>
    Section,
    /// <summary>The print page range.</summary>
    Page,
    /// <summary>Whether the content has an image.</summary>
    Image,
    /// <summary>The publish time of day.</summary>
    Time,
    /// <summary>The plain-text character count.</summary>
    Characters,
}

/// <summary>
/// TopicScoreRuleEvaluation record, whether one rule matched and, when it did not, the first
/// condition that failed.
/// </summary>
/// <param name="RuleId">The rule.</param>
/// <param name="SortOrder">The rule's position within its source.</param>
/// <param name="Score">The rule's score.</param>
/// <param name="IsMatch">Whether every condition the rule sets matched.</param>
/// <param name="FailedCondition">The first condition that failed.</param>
/// <param name="Reason">A human-readable explanation of the failure.</param>
public record TopicScoreRuleEvaluation(int RuleId, int SortOrder, int Score, bool IsMatch, TopicScoreCondition? FailedCondition, string? Reason);

/// <summary>
/// TopicScoreResult record, the calculated score and how it was reached.
/// </summary>
/// <param name="Score">The calculated score.</param>
/// <param name="RuleId">The matching rule, or null when the source default (or 0) applied.</param>
/// <param name="IsSourceDefault">No rule matched and the source default score applied.</param>
/// <param name="Evaluations">Every rule evaluated, in order, up to and including the match.</param>
public record TopicScoreResult(int Score, int? RuleId, bool IsSourceDefault, IReadOnlyList<TopicScoreRuleEvaluation> Evaluations);

/// <summary>
/// TopicScoreCalculator static class, evaluates topic score rules. Rules are evaluated in sort
/// order within the content's source and the first match wins; with no match the source default
/// applies, and with no default the score is 0. A rule matches when every condition it sets
/// matches; an unset condition matches anything.
/// </summary>
public static partial class TopicScoreCalculator
{
    [GeneratedRegex(@"^\s*(?<prefix>[a-zA-Z]*)\s*(?<number>\d+)")]
    private static partial Regex PageRegex();

    [GeneratedRegex(@"^[a-zA-Z]*\d+$")]
    private static partial Regex RulePageRegex();

    /// <summary>
    /// The maximum length of a rule page (prefix plus number).
    /// </summary>
    public const int MaxPageLength = 5;

    /// <summary>
    /// Whether content is scored: its source or its series uses topics, and it is not Image content.
    /// This is the same rule the editor content form uses to show topics.
    /// </summary>
    /// <param name="contentType"></param>
    /// <param name="sourceUseInTopics"></param>
    /// <param name="seriesUseInTopics"></param>
    /// <returns></returns>
    public static bool IsEligible(ContentType contentType, bool sourceUseInTopics, bool seriesUseInTopics)
    {
        return contentType != ContentType.Image && (sourceUseInTopics || seriesUseInTopics);
    }

    /// <summary>
    /// Calculate the score for the specified input.
    /// </summary>
    /// <param name="rules">Rules for the content's source; other sources' rules are ignored.</param>
    /// <param name="sourceDefaultScore">The source's default score.</param>
    /// <param name="input"></param>
    /// <param name="timeZone">The time zone rule times are expressed in.</param>
    /// <param name="evaluateAll">Evaluate every rule, not only those before the match (for the rule tester).</param>
    /// <returns></returns>
    public static TopicScoreResult Calculate(IEnumerable<TopicScoreRule> rules, int? sourceDefaultScore, TopicScoreInput input, TimeZoneInfo timeZone, bool evaluateAll = false)
    {
        var evaluations = new List<TopicScoreRuleEvaluation>();
        TopicScoreRuleEvaluation? match = null;
        foreach (var rule in rules.Where(r => r.SourceId == input.SourceId).OrderBy(r => r.SortOrder).ThenBy(r => r.Id))
        {
            var evaluation = Evaluate(rule, input, timeZone);
            if (match != null)
            {
                // Rules after the match never apply; the tester still explains them.
                evaluations.Add(evaluation with { IsMatch = false, FailedCondition = null, Reason = evaluation.IsMatch ? "An earlier rule matched first." : evaluation.Reason });
                continue;
            }
            evaluations.Add(evaluation);
            if (evaluation.IsMatch)
            {
                match = evaluation;
                if (!evaluateAll) break;
            }
        }

        if (match != null) return new TopicScoreResult(match.Score, match.RuleId, false, evaluations);
        return new TopicScoreResult(sourceDefaultScore ?? 0, null, sourceDefaultScore.HasValue, evaluations);
    }

    /// <summary>
    /// Evaluate one rule against the input.
    /// </summary>
    /// <param name="rule"></param>
    /// <param name="input"></param>
    /// <param name="timeZone"></param>
    /// <returns></returns>
    public static TopicScoreRuleEvaluation Evaluate(TopicScoreRule rule, TopicScoreInput input, TimeZoneInfo timeZone)
    {
        TopicScoreRuleEvaluation Fail(TopicScoreCondition condition, string reason) => new(rule.Id, rule.SortOrder, rule.Score, false, condition, reason);

        if (rule.SeriesId.HasValue && input.SeriesId != rule.SeriesId)
            return Fail(TopicScoreCondition.Series, input.SeriesId.HasValue ? "The series does not match." : "The content has no series.");

        if (!String.IsNullOrWhiteSpace(rule.Section))
        {
            if (String.IsNullOrWhiteSpace(input.Section))
                return Fail(TopicScoreCondition.Section, "The content has no section.");
            if (!String.Equals(rule.Section.Trim(), input.Section.Trim(), StringComparison.OrdinalIgnoreCase))
                return Fail(TopicScoreCondition.Section, "The section does not match.");
        }

        if (!String.IsNullOrWhiteSpace(rule.PageMin) || !String.IsNullOrWhiteSpace(rule.PageMax))
        {
            var page = ParsePage(input.Page);
            if (page == null)
                return Fail(TopicScoreCondition.Page, "The content has no page number.");
            var min = ParsePage(rule.PageMin);
            var max = ParsePage(rule.PageMax);
            var prefix = (min ?? max)!.Value.Prefix;
            if (!String.Equals(prefix, page.Value.Prefix, StringComparison.OrdinalIgnoreCase))
                return Fail(TopicScoreCondition.Page, "The page prefix does not match.");
            if ((min.HasValue && page.Value.Number < min.Value.Number) || (max.HasValue && page.Value.Number > max.Value.Number))
                return Fail(TopicScoreCondition.Page, "The page is outside the range.");
        }

        if (rule.HasImage.HasValue && rule.HasImage.Value != input.HasImage)
            return Fail(TopicScoreCondition.Image, rule.HasImage.Value ? "The content has no image." : "The content has an image.");

        if (rule.TimeMin.HasValue || rule.TimeMax.HasValue)
        {
            if (!input.PublishedOn.HasValue)
                return Fail(TopicScoreCondition.Time, "The content has no publish time.");
            var time = ToTimeOfDay(input.PublishedOn.Value, timeZone);
            if (!IsTimeInRange(time, rule.TimeMin, rule.TimeMax))
                return Fail(TopicScoreCondition.Time, "The publish time is outside the range.");
        }

        if ((rule.CharacterMin.HasValue && input.CharacterCount < rule.CharacterMin) || (rule.CharacterMax.HasValue && input.CharacterCount > rule.CharacterMax))
            return Fail(TopicScoreCondition.Characters, "The character count is outside the range.");

        return new TopicScoreRuleEvaluation(rule.Id, rule.SortOrder, rule.Score, true, null, null);
    }

    /// <summary>
    /// Parse a page as a prefix plus a number ("A12" is prefix "A", number 12).
    /// </summary>
    /// <param name="page"></param>
    /// <returns>Null when the value has no page number.</returns>
    public static (string Prefix, int Number)? ParsePage(string? page)
    {
        if (String.IsNullOrWhiteSpace(page)) return null;
        var match = PageRegex().Match(page);
        if (!match.Success || !int.TryParse(match.Groups["number"].Value, out var number)) return null;
        return (match.Groups["prefix"].Value, number);
    }

    /// <summary>
    /// Whether a time of day is in the inclusive range. A range whose minimum is after its maximum
    /// wraps midnight.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static bool IsTimeInRange(TimeSpan time, TimeSpan? min, TimeSpan? max)
    {
        if (min.HasValue && max.HasValue && min > max)
            return time >= min || time <= max;
        return (!min.HasValue || time >= min) && (!max.HasValue || time <= max);
    }

    /// <summary>
    /// Convert a stored UTC date to the time of day in the specified time zone. An unspecified
    /// kind is treated as UTC, which is how content dates are stored.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="timeZone"></param>
    /// <returns></returns>
    public static TimeSpan ToTimeOfDay(DateTime date, TimeZoneInfo timeZone)
    {
        var utc = date.Kind switch
        {
            DateTimeKind.Utc => date,
            DateTimeKind.Local => date.ToUniversalTime(),
            _ => DateTime.SpecifyKind(date, DateTimeKind.Utc),
        };
        return TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone).TimeOfDay;
    }

    /// <summary>
    /// Validate a rule before it is saved.
    /// </summary>
    /// <param name="rule"></param>
    /// <returns>The validation errors; empty when the rule is valid.</returns>
    public static IReadOnlyList<string> Validate(TopicScoreRule rule)
    {
        var errors = new List<string>();
        if (rule.Score < 0) errors.Add("Score must be a whole number of 0 or more.");
        foreach (var (name, value) in new[] { ("Page minimum", rule.PageMin), ("Page maximum", rule.PageMax) })
        {
            if (String.IsNullOrWhiteSpace(value)) continue;
            if (value.Trim().Length > MaxPageLength) errors.Add($"{name} must be at most {MaxPageLength} characters.");
            else if (!RulePageRegex().IsMatch(value.Trim()))
                errors.Add($"{name} must be a letter prefix followed by a number, e.g. A1.");
        }
        var min = ParsePage(rule.PageMin);
        var max = ParsePage(rule.PageMax);
        if (min.HasValue && max.HasValue)
        {
            if (!String.Equals(min.Value.Prefix, max.Value.Prefix, StringComparison.OrdinalIgnoreCase))
                errors.Add("Page minimum and maximum must share a prefix.");
            else if (min.Value.Number > max.Value.Number)
                errors.Add("Page minimum must not be greater than page maximum.");
        }
        if (rule.CharacterMin.HasValue && rule.CharacterMin < 0) errors.Add("Character minimum must be 0 or more.");
        if (rule.CharacterMax.HasValue && rule.CharacterMax < 0) errors.Add("Character maximum must be 0 or more.");
        if (rule.CharacterMin.HasValue && rule.CharacterMax.HasValue && rule.CharacterMin > rule.CharacterMax)
            errors.Add("Character minimum must not be greater than character maximum.");
        return errors;
    }
}
