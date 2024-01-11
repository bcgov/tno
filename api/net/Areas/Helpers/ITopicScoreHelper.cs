using TNO.Entities;
using TNO.API.Areas.Services.Models.Content;

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
        void SetContentScore(ref Content content);

        /// <summary>
        /// Calculate the Topic score for a piece of content and update the Topics
        /// </summary>
        /// <param name="contentModel"></param>
        void SetContentScore(ref ContentModel contentModel);

        /// <summary>
        /// Calculates the score for a piece of content
        /// </summary>
        /// <param name="publishedOnTime"></param>
        /// <param name="sourceId"></param>
        /// <param name="bodyLength"></param>
        /// <param name="section"></param>
        /// <param name="pageWithPrefix"></param>
        /// <param name="seriesId"></param>
        /// <returns>a score OR null if no rule could be found to apply</returns>
        int? GetScore(TimeSpan? publishedOnTime = null, int? sourceId = null, int bodyLength = 0, string section = "", string pageWithPrefix = "", int? seriesId = null);
        
        /// <summary>
        /// Calculates the score for a piece of content.
        /// This overloaded method is targeted at situations where multiple content items are being iterated over and avoids the overhead of an implicit call to retrieve all rules from the db with every call.
        /// </summary>
        /// <param name="allRules"></param>
        /// <param name="publishedOnTime"></param>
        /// <param name="sourceId"></param>
        /// <param name="bodyLength"></param>
        /// <param name="section"></param>
        /// <param name="pageWithPrefix"></param>
        /// <param name="seriesId"></param>
        /// <returns>a score OR null if no rule could be found to apply</returns>
        int? GetScore(IEnumerable<TopicScoreRule> allRules, TimeSpan? publishedOnTime = null, int? sourceId = null, int bodyLength = 0, string section = "", string pageWithPrefix = "", int? seriesId = null);
    }
}
