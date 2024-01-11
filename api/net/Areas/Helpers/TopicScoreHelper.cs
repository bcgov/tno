using System.Text.RegularExpressions;
using TNO.API.Areas.Services.Models.Content;
using TNO.DAL.Services;
using TNO.Entities;

namespace TNO.API.Helpers
{
    /// <summary>
    /// Helper class for calculating Topic scores and updating Content Topic Scores
    /// </summary>
    public class TopicScoreHelper : ITopicScoreHelper
    {
        #region Variables
        private readonly ITopicScoreRuleService _topicScoreRuleService;
        private readonly ITopicService _topicService;
        ILogger<TopicScoreHelper> _logger;
        private static Regex _pageNumberAndPrefixRegex = new Regex(@"(?<page_prefix>[a-zA-Z]+)?(?<page_number>\d+)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of a TopicScoreHelper object, initializes with specified parameters.
        /// </summary>
        /// <param name="topicScoreRuleService"></param>
        /// <param name="topicService"></param>
        /// <param name="logger"></param>
        public TopicScoreHelper(ITopicScoreRuleService topicScoreRuleService, ITopicService topicService, ILogger<TopicScoreHelper> logger)
        {
            _topicScoreRuleService = topicScoreRuleService;
            _topicService = topicService;
            _logger = logger;
        }
        #endregion

        #region Methods

        /// <inheritdoc/>
        public void SetContentScore(Content content)
        {
            var topicScore = GetScore(content.PublishedOn?.TimeOfDay, content.SourceId, content.Body.Length, content.Section, content.Page, content.SeriesId);
            if (topicScore != null)
            {
                if (!content.TopicsManyToMany.Any())
                {
                    // retrieve the magic placeholder Topic and use that
                    const int defaultTopicId = 1;
                    Topic? defaultTopic = _topicService.FindById(defaultTopicId);
                    if (defaultTopic != null)
                        content.TopicsManyToMany.Add(new ContentTopic(content, defaultTopic, 0));
                    else
                        _logger.LogWarning($"Couldn't retrieve default Topic with ID: [{defaultTopicId}] for Event of the day. Score will not be set on content.");
                }
                else
                {
                    content.TopicsManyToMany.First().Score = topicScore.Value;
                }
            }
        }

        /// <inheritdoc/>
        public void SetContentScore(ContentModel model)
        {
            var topicScore = GetScore(_topicScoreRuleService.FindAll(), model.PublishedOn?.TimeOfDay, model.SourceId, model.Body.Length, model.Section, model.Page, model.SeriesId);
            if (topicScore != null) {
                if(!model.Topics.Any()) {
                    // retrieve the magic placeholder Topic and use that
                    const int defaultTopicId = 1;
                    Entities.Topic? defaultTopic = _topicService.FindById(defaultTopicId);
                    if (defaultTopic != null)
                        model.Topics = new[] { new ContentTopicModel { ContentId = 0, Id = defaultTopic.Id, Name = defaultTopic.Name, TopicType = defaultTopic.TopicType, Score = topicScore.Value } };
                    else
                        _logger.LogWarning($"Couldn't retrieve default Topic with ID: [{defaultTopicId}] for Event of the day. Score will not be set on content.");
                } else {
                    model.Topics.First().Score = topicScore.Value;
                }
            }
        }

        /// <inheritdoc/>
        public int? GetScore(TimeSpan? publishedOnTime = null, int? sourceId = null, int bodyLength = 0, string section = "", string pageWithPrefix = "", int? seriesId = null)
        {
            return GetScore(_topicScoreRuleService.FindAll(), publishedOnTime, sourceId, bodyLength, section, pageWithPrefix, seriesId);
        }

        /// <inheritdoc/>
        public int? GetScore(IEnumerable<TopicScoreRule> allRules, TimeSpan? publishedOnTime = null, int? sourceId = null, int bodyLength = 0, string section = "", string pageWithPrefix = "", int? seriesId = null)
        {
            int? score = null;

            IEnumerable<TopicScoreRule> rulesForSource = allRules.Where((r) => r.SourceId == sourceId);

            // no applicable rules
            if (!rulesForSource.Any()) return score;

            foreach (TopicScoreRule rule in rulesForSource)
            {
                // PRINT CONTENT - rule section doesnt match the section on the content
                if (rule.Section != null && !string.IsNullOrEmpty(section) && section != rule.Section) continue;

                // AUDIO-VIDEO CONTENT - rule series doesnt match the series on the content
                if (rule.SeriesId != null && seriesId != null && seriesId != rule.SeriesId) continue;

                (string rulePageMinPrefix, int? rulePageMinNumber) = GetPrintSourceRulePageNumberAndPrefix(rule.PageMin);
                (string rulePageMaxPrefix, int? rulePageMaxNumber) = GetPrintSourceRulePageNumberAndPrefix(rule.PageMax);
                (string actualPagePrefix, int? actualPageNumber) = GetPrintSourceRulePageNumberAndPrefix(pageWithPrefix);

                if (!IsPrintPageInRange(rulePageMinPrefix, rulePageMinNumber, rulePageMaxPrefix, rulePageMaxNumber, actualPagePrefix, actualPageNumber)) continue;

                // if (r.hasImage != null && hasImage) continue; // TODO: We need a way to identify a story has an image.

                if (!IsPrintContentALengthMatch(bodyLength, rule)) continue;

                // AUDIO-VIDEO CONTENT ONLY? - rule series doesnt match the series on the content
                if (!IsAudioVideoContentATimeMatch(publishedOnTime, rule.TimeMin, rule.TimeMax)) continue;

                // Take the first matching rule.
                score = rule.Score;

                break;
            }

            return score;
        }

        /// <summary>
        /// extract the page prefix (if it exists) and the page number
        /// </summary>
        /// <param name="pageRef"></param>
        /// <returns></returns>
        private Tuple<string, int?> GetPrintSourceRulePageNumberAndPrefix(string? pageRef)
        {
            string pagePrefix = string.Empty;
            int? pageNumber = null;
            if (!string.IsNullOrEmpty(pageRef))
            {
                Match match = _pageNumberAndPrefixRegex.Match(pageRef);
                if (match.Success)
                {
                    pagePrefix = match.Groups["page_prefix"].Value;
                    string unparsedPageNumber = match.Groups["page_number"].Value;
                    if (!string.IsNullOrEmpty(unparsedPageNumber))
                    {
                        try
                        {
                            pageNumber = int.Parse(unparsedPageNumber);
                        }
                        catch (Exception)
                        {
                            // a very weird page number perhaps...?
                        }
                    }
                }
            }
            return new Tuple<string, int?>(pagePrefix, pageNumber);
        }

        /// <summary>
        /// Check the page number and prefix against the rule values
        /// </summary>
        /// <param name="rulePageMinPrefix"></param>
        /// <param name="rulePageMinNumber"></param>
        /// <param name="rulePageMaxPrefix"></param>
        /// <param name="rulePageMaxNumber"></param>
        /// <param name="actualPagePrefix"></param>
        /// <param name="actualPageNumber"></param>
        /// <returns></returns>
        private bool IsPrintPageInRange(string rulePageMinPrefix, int? rulePageMinNumber, string rulePageMaxPrefix, int? rulePageMaxNumber, string actualPagePrefix, int? actualPageNumber)
        {
            if ((rulePageMinNumber != null && rulePageMaxNumber != null &&
                (actualPageNumber == 0 || actualPageNumber < rulePageMinNumber || actualPageNumber > rulePageMaxNumber)) ||
              (rulePageMinPrefix != null && actualPagePrefix != rulePageMinPrefix && rulePageMaxPrefix != null && actualPagePrefix != rulePageMaxPrefix))
            {
                return false;
            }
            else if ((rulePageMinNumber != null && (actualPageNumber == 0 || actualPageNumber < rulePageMinNumber)) ||
                (rulePageMinPrefix != null && actualPagePrefix != rulePageMinPrefix && rulePageMaxPrefix != null && actualPagePrefix != rulePageMaxPrefix))
            {
                return false;
            }
            else if ((rulePageMaxNumber != null && (actualPageNumber == 0 || actualPageNumber > rulePageMaxNumber)) ||
              (rulePageMinPrefix != null && actualPagePrefix != rulePageMinPrefix && rulePageMaxPrefix != null && actualPagePrefix != rulePageMaxPrefix))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// If the rule has character min/max values, check bodyLength to see if it's in the range
        /// </summary>
        /// <param name="bodyLength"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private bool IsPrintContentALengthMatch(int bodyLength, TopicScoreRule r)
        {
            if (r.CharacterMin != null && r.CharacterMax != null &&
                (bodyLength < r.CharacterMin || bodyLength > r.CharacterMax))
            {
                return false;
            }
            else if (r.CharacterMin != null && bodyLength < r.CharacterMin) return false;
            else if (r.CharacterMax != null && bodyLength > r.CharacterMax) return false;

            return true;
        }

        /// <summary>
        /// If the rule has timeMin or timeMax values, check the publishedOnTime to see if it's within the range
        /// </summary>
        /// <param name="publishedOnTime"></param>
        /// <param name="timeMin"></param>
        /// <param name="timeMax"></param>
        /// <returns></returns>
        private bool IsAudioVideoContentATimeMatch(TimeSpan? publishedOnTime, TimeSpan? timeMin, TimeSpan? timeMax)
        {
            if (timeMin != null && timeMax != null &&
              (publishedOnTime < timeMin || publishedOnTime > timeMax)
            )
            {
                return false;
            }
            else if (timeMin != null && publishedOnTime < timeMin) return false;
            else if (timeMax != null && publishedOnTime > timeMax) return false;

            return true;
        }

        #endregion
    }
}
