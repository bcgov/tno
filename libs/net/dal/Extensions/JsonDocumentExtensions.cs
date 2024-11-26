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
        if (json == null || !contentIds.Any()) return query;

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
    /// Modify the Elasticsearch 'query' and add a 'should' filter to include the specified 'contentIds' and the previous published on date time.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="contentIds"></param>
    /// <param name="previousInstancePublishedOn"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static JsonDocument IncludeOnlyLatestPostedAndContentIds(this JsonDocument query, IEnumerable<long> contentIds, DateTime? previousPublishedOn)
    {
        var json = JsonNode.Parse(query.ToJson())?.AsObject();
        if (json == null) return query;
        JsonNode? jShouldQuery = null;

        if (contentIds != null && contentIds.Any())
        {
            jShouldQuery = JsonNode.Parse($"{{ \"terms\": {{ \"id\": [{String.Join(',', contentIds)}] }}}}")?.AsObject() ?? throw new InvalidOperationException("Failed to parse JSON");
        }
        if (previousPublishedOn != null)
        {
            var jIncludeOnlyLatestPosted = JsonNode.Parse($"{{ \"range\": {{ \"postedOn\": {{ \"gte\": \"{previousPublishedOn.Value:yyyy-MM-ddTHH:mm:ss}\", \"time_zone\": \"US/Pacific\" }} }} }}")?.AsObject();
            if (jIncludeOnlyLatestPosted != null)
            {
                if (jShouldQuery != null)
                {
                    jShouldQuery = JsonNode.Parse($"{{ \"bool\": {{ \"should\": [ {jShouldQuery.ToJsonString()}, {jIncludeOnlyLatestPosted.ToJsonString()} ]}}}}")?.AsObject();
                }
                else
                {
                    jShouldQuery = JsonNode.Parse($"{{ \"bool\": {{ \"should\": [ {jIncludeOnlyLatestPosted.ToJsonString()} ]}}}}")?.AsObject();
                }
            }
        }
        if (jShouldQuery != null)
        {
            if (json.TryGetPropertyValue("query", out JsonNode? jQuery))
            {
                if (jQuery?.AsObject().TryGetPropertyValue("bool", out JsonNode? jQueryBool) == true)
                {
                    if (jQueryBool?.AsObject().TryGetPropertyValue("must", out JsonNode? jQueryBoolMust) == true)
                    {
                        jQueryBoolMust?.AsArray().Add(jShouldQuery);
                    }
                    else
                    {
                        jQueryBool?.AsObject().Add("must", JsonNode.Parse($"[ {jShouldQuery.ToJsonString()} ]"));
                    }
                }
                else
                {
                    jQuery?.AsObject().Add("bool", JsonNode.Parse($"{{ \"must\": [ {jShouldQuery.ToJsonString()} ]}}"));
                }
            }
            else
            {
                
                json.Add("query", JsonNode.Parse($"{{ \"bool\": {{ \"must\": [ {jShouldQuery.ToJsonString()} ] }}}}"));
            }
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

        var jIncludeOnlyLatestPosted = JsonNode.Parse($"{{ \"range\": {{ \"postedOn\": {{ \"gte\": \"{previousInstancePublishedOn.Value:yyyy-MM-ddTHH:mm:ssZ}\", \"time_zone\": \"US/Pacific\" }} }} }}");
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

    /// <summary>
    /// Modify the Elasticsearch 'query' and add a 'must_not' filter to exclude the specified 'sourceIds'.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="sourceIds"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static JsonDocument AddExcludeSources(this JsonDocument query, IEnumerable<int> sourceIds)
    {
        var json = JsonNode.Parse(query.ToJson())?.AsObject();
        if (json == null || !sourceIds.Any()) return query;

        var jMustNotTerms = JsonNode.Parse($"{{ \"terms\": {{ \"sourceId\": [{String.Join(',', sourceIds)}] }}}}")?.AsObject() ?? throw new InvalidOperationException("Failed to parse JSON");
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
    /// Modify the Elasticsearch 'query' and add a 'must_not' filter to exclude the specified 'mediaTypes'.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="mediaTypes"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static JsonDocument AddExcludeMediaTypes(this JsonDocument query, IEnumerable<int> mediaTypes)
    {
        var json = JsonNode.Parse(query.ToJson())?.AsObject();
        if (json == null || !mediaTypes.Any()) return query;

        var jMustNotTerms = JsonNode.Parse($"{{ \"terms\": {{ \"mediaTypeId\": [{String.Join(',', mediaTypes)}] }}}}")?.AsObject() ?? throw new InvalidOperationException("Failed to parse JSON");
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
    /// Modify the Elasticsearch 'query' and add a 'must_not' filter to exclude content with "BC Update" in the body or "BC Calendar" in the headline.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public static JsonDocument ExcludeBCUpdate(this JsonDocument query)
    {
        var json = JsonNode.Parse(query.ToJson())?.AsObject();
        if (json == null) return query;

        // Get the query object
        if (!json.TryGetPropertyValue("query", out var queryNode))
        {
            queryNode = new JsonObject();
            json["query"] = queryNode;
        }

        // Get or create the bool query object
        if (!((JsonObject)queryNode!).TryGetPropertyValue("bool", out var boolNode))
        {
            boolNode = new JsonObject();
            ((JsonObject)queryNode!)["bool"] = boolNode;
        }

        // Get or create the must_not array
        if (!((JsonObject)boolNode!).TryGetPropertyValue("must_not", out var mustNotNode))
        {
            mustNotNode = new JsonArray();
            ((JsonObject)boolNode!)["must_not"] = mustNotNode;
        }

        // Add first query_string to exclude "BC Update"
        ((JsonArray)mustNotNode!).Add(new JsonObject
        {
            ["query_string"] = new JsonObject
            {
                ["query"] = "\"BC Update\"",
                ["fields"] = new JsonArray { "body" }
            }
        });

        // Add second query_string to exclude "BC Calendar"
        ((JsonArray)mustNotNode!).Add(new JsonObject
        {
            ["query_string"] = new JsonObject
            {
                ["query"] = "\"BC Calendar\"",
                ["fields"] = new JsonArray { "headline" }
            }
        });

        return JsonDocument.Parse(json.ToJsonString());
    }
}
