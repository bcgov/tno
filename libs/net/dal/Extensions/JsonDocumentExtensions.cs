using System.Text.Json;
using System.Text.Json.Nodes;
using TNO.Core.Extensions;

namespace TNO.DAL.Extensions;

/// <summary>
/// JsonDocumentExtensions static class, provides extension methods for JsonDocument objects.
/// </summary>
public static class JsonDocumentExtensions
{
    /// <summary>
    /// Modify the Elasticsearch 'query' and add a 'must_not' filter to exclude the specified 'contentIds'.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="contentIds"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static JsonDocument AddExcludeContent(this JsonDocument query, IEnumerable<long> contentIds)
    {
        var json = JsonNode.Parse(query.ToJson())?.AsObject();
        if (json == null) return query;

        var jMustNotTerms = JsonNode.Parse($"{{ \"terms\": {{ \"id\": [{String.Join(',', contentIds)}] }}}}")?.AsObject() ?? throw new InvalidOperationException("Failed to parse JSON");
        if (json.TryGetPropertyValue("query", out JsonNode? jQuery))
        {
            if (jQuery?.AsObject().TryGetPropertyValue("bool", out JsonNode? jQueryBool) == true)
            {
                if (jQueryBool?.AsObject().TryGetPropertyValue("must_not", out JsonNode? jQueryBoolMustNot) == true)
                {
                    jQueryBoolMustNot?.AsArray().Add(jMustNotTerms);
                }
                else
                {
                    jQueryBool?.AsObject().Add("must_not", JsonNode.Parse($"[ {jMustNotTerms.ToJsonString()} ]"));
                }
            }
            else
            {
                jQuery?.AsObject().Add("bool", JsonNode.Parse($"{{ \"must_not\": [ {jMustNotTerms.ToJsonString()} ]}}"));
            }
        }
        else
        {
            json.Add("query", JsonNode.Parse($"{{ \"bool\": {{ \"must_not\": [ {jMustNotTerms.ToJsonString()} ] }}}}"));
        }
        return JsonDocument.Parse(json.ToJsonString());
    }

    /// <summary>
    /// Modify the Elasticsearch 'query' and add a 'must' filter to exclude content posted before the last report instance published date.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="previousInstancePublishedOn"></param>
    /// <returns></returns>
    public static JsonDocument IncludeOnlyLatestPosted(this JsonDocument query, DateTime? previousInstancePublishedOn)
    {
        var json = JsonNode.Parse(query.ToJson())?.AsObject();
        if (json == null || previousInstancePublishedOn == null) return query;

        var jIncludeOnlyLatestPosted = JsonNode.Parse($"{{ \"range\": {{ \"postedOn\": {{ \"gte\": \"{previousInstancePublishedOn.Value.ToLocalTime():yyyy-MM-dd}\", \"time_zone\": \"US/Pacific\" }} }} }}");
        if (json.TryGetPropertyValue("query", out JsonNode? jQuery))
        {
            if (jQuery?.AsObject().TryGetPropertyValue("bool", out JsonNode? jQueryBool) == true)
            {
                if (jQueryBool?.AsObject().TryGetPropertyValue("must", out JsonNode? jQueryBoolMust) == true)
                {
                    jQueryBoolMust?.AsArray().Add(jIncludeOnlyLatestPosted!);
                }
                else
                {
                    jQueryBool?.AsObject().Add("must", JsonNode.Parse($"[ {jIncludeOnlyLatestPosted!.ToJsonString()} ]"));
                }
            }
            else
            {
                jQuery?.AsObject().Add("bool", JsonNode.Parse($"{{ \"must\": [ {jIncludeOnlyLatestPosted!.ToJsonString()} ]}}"));
            }
        }
        else
        {
            json.Add("query", JsonNode.Parse($"{{ \"bool\": {{ \"must\": [ {jIncludeOnlyLatestPosted!.ToJsonString()} ] }}}}"));
        }

        return JsonDocument.Parse(json.ToJsonString());
    }
}
