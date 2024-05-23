using TNO.API.Areas.Services.Models.Content;
using TNO.Entities;

namespace TNO.API.Helpers
{
    /// <summary>
    ///
    /// </summary>
    public interface ITopicScoreHelper
    {
        /// <summary>
        /// Calculate the Topic score for a piece of content and update the Topics
        /// </summary>
        /// <param name="content"></param>
        void SetContentScore(Content content);

        /// <summary>
        /// Calculate the Topic score for a piece of content and update the Topics
        /// </summary>
        /// <param name="contentModel"></param>
        void SetContentScore(ContentModel contentModel);

        /// <summary>
        /// Calculates the score for a piece of content
        /// </summary>
        /// <param name="publishedOn"></param>
        /// <param name="sourceId"></param>
        /// <param name="bodyLength"></param>
        /// <param name="section"></param>
        /// <param name="pageWithPrefix"></param>
        /// <param name="seriesId"></param>
        /// <returns>a score OR null if no rule could be found to apply</returns>
        int? GetScore(DateTime? publishedOn = null, int? sourceId = null, int bodyLength = 0, string section = "", string pageWithPrefix = "", int? seriesId = null);

        /// <summary>
        /// Calculates the score for a piece of content.
        /// This overloaded method is targeted at situations where multiple content items are being iterated over and avoids the overhead of an implicit call to retrieve all rules from the db with every call.
        /// </summary>
        /// <param name="allRules"></param>
        /// <param name="publishedOn"></param>
        /// <param name="sourceId"></param>
        /// <param name="bodyLength"></param>
        /// <param name="section"></param>
        /// <param name="pageWithPrefix"></param>
        /// <param name="seriesId"></param>
        /// <returns>a score OR null if no rule could be found to apply</returns>
        int? GetScore(IEnumerable<TopicScoreRule> allRules, DateTime? publishedOn = null, int? sourceId = null, int bodyLength = 0, string section = "", string pageWithPrefix = "", int? seriesId = null);
    }
}
