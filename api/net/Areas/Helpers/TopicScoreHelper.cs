using System.Text.RegularExpressions;
using TNO.Entities;

namespace TNO.API.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public static class TopicScoreHelper
    {
        private static Regex _pageNumberAndPrefixRegex = new Regex(@"(?<page_prefix>[a-zA-Z]+)?(?<page_number>\d+)?", RegexOptions.IgnoreCase | RegexOptions.Compiled );
        /// <inheritdoc/>
        public static int? GetScore(IEnumerable<TopicScoreRule> allRules, TimeSpan? publishedOnTime = null, int? sourceId = null, int bodyLength = 0, string section = "", string pageWithPrefix = "", int? seriesId = null)
        {
            int? score = null;

            IEnumerable<Entities.TopicScoreRule> rulesForSource = allRules.Where((r) => r.SourceId == sourceId);

            // no applicable rules
            if (!rulesForSource.Any()) return score;

            foreach (Entities.TopicScoreRule rule in rulesForSource)
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

                if (!IsPrintContentATimeMatch(publishedOnTime, rule.TimeMin, rule.TimeMax)) continue;

                // Take the first matching rule.
                score = rule.Score;

                break;
            }

            return score;
        }

        private static Tuple<string, int?> GetPrintSourceRulePageNumberAndPrefix(string? pageRef)
        {
            string pagePrefix = string.Empty;
            int? pageNumber = null;
            if (!string.IsNullOrEmpty(pageRef))
            {
                // extract the page prefix (if it exists) and the page number
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

        private static bool IsPrintPageInRange(string rulePageMinPrefix, int? rulePageMinNumber, string rulePageMaxPrefix, int? rulePageMaxNumber, string actualPagePrefix, int? actualPageNumber)
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
        private static bool IsPrintContentALengthMatch(int bodyLength, TopicScoreRule r)
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
        private static bool IsPrintContentATimeMatch(TimeSpan? publishedOnTime, TimeSpan? timeMin, TimeSpan? timeMax)
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
    }
}
