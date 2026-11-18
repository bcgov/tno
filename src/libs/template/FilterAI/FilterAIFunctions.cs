using System.Text.Json.Nodes;
using TNO.AI;

namespace TNO.TemplateEngine.FilterAI;

/// <summary>
/// FilterAIFunctions class, the functions a model with native function calling may call: the tools
/// (lookup, search, story) and the function that finishes the conversation (submit_filter when
/// generating, answer when chatting). Schemas that describe open-ended objects (an Elasticsearch
/// query, the filter settings) cannot be strict; the generator validates those arguments itself.
/// </summary>
public static class FilterAIFunctions
{
    #region Variables
    /// <summary>The function that finishes a generation.</summary>
    public const string SubmitFilter = "submit_filter";

    /// <summary>The function that finishes a chat answer.</summary>
    public const string Answer = "answer";

    /// <summary>Find the id or code of a source, media type, series, contributor, tag or action.</summary>
    public static readonly LlmFunction Lookup = new(
        "lookup",
        "Find reference values by part of their name or code, and return them with their id or code. Use it for sources, series and contributors, which are not listed, and whenever a value is not under Reference values. Never guess an id or code.",
        new JsonObject()
        {
            ["type"] = "object",
            ["properties"] = new JsonObject()
            {
                ["kind"] = new JsonObject()
                {
                    ["type"] = "string",
                    ["enum"] = new JsonArray(FilterAIReferenceKind.All.Select(k => (JsonNode)k).ToArray()),
                    ["description"] = "The kind of value to find.",
                },
                ["text"] = new JsonObject()
                {
                    ["type"] = "string",
                    ["description"] = "Part of a name or code, or an empty string to list values.",
                },
            },
            ["required"] = new JsonArray("kind", "text"),
            ["additionalProperties"] = false,
        },
        Strict: true);

    /// <summary>Run a query against the content index.</summary>
    public static readonly LlmFunction Search = new(
        "search",
        "Run a query against the content index. Returns the total, the newest matching stories (headline, source, media type, series, published on, and the collapse field's value) and, with 'aggregate', the most common values of that field among the matches. With 'collapse' it returns one story per value of that field, as the filter's collapseField would. Scripts are not allowed.",
        new JsonObject()
        {
            ["type"] = "object",
            ["properties"] = new JsonObject()
            {
                ["query"] = new JsonObject()
                {
                    ["type"] = "object",
                    ["description"] = "An Elasticsearch query clause, i.e. { \"bool\": { \"must\": [ ... ], \"filter\": [ ... ] } }.",
                },
                ["size"] = new JsonObject() { ["type"] = "integer", ["minimum"] = 0, ["maximum"] = 10, ["description"] = "How many stories to return." },
                ["aggregate"] = new JsonObject() { ["type"] = "string", ["description"] = "A field whose most common values to return." },
                ["aggregateSize"] = new JsonObject() { ["type"] = "integer", ["minimum"] = 1, ["maximum"] = 50, ["description"] = "How many values of 'aggregate' to return." },
                ["collapse"] = new JsonObject() { ["type"] = "string", ["description"] = "Return one story per value of this field." },
            },
            ["required"] = new JsonArray("query"),
        });

    /// <summary>Read one story.</summary>
    public static readonly LlmFunction Story = new(
        "story",
        "Return a story's fields (headline, source, media type, dates, tags, actions, tone, the start of its text) and, when the filter's query is known, which parts of the query matched it.",
        new JsonObject()
        {
            ["type"] = "object",
            ["properties"] = new JsonObject()
            {
                ["id"] = new JsonObject() { ["type"] = "integer", ["description"] = "The story's numeric id." },
            },
            ["required"] = new JsonArray("id"),
            ["additionalProperties"] = false,
        },
        Strict: true);

    /// <summary>The tools, in the order they are offered.</summary>
    public static readonly LlmFunction[] Tools = [Lookup, Search, Story];

    /// <summary>Finish a generation with the filter.</summary>
    public static readonly LlmFunction SubmitFilterFunction = new(
        SubmitFilter,
        "Submit the finished filter. Call it on its own, after the results of every other call have come back; it is the only way to finish.",
        new JsonObject()
        {
            ["type"] = "object",
            ["properties"] = new JsonObject()
            {
                ["settings"] = new JsonObject()
                {
                    ["type"] = "object",
                    ["description"] = "The filter settings described under 'Filter settings'. Use {} only when no standard settings are needed.",
                },
                ["extendedQuery"] = new JsonObject()
                {
                    ["type"] = new JsonArray("object", "null"),
                    ["description"] = "An Elasticsearch query clause for conditions the settings cannot express, or null.",
                },
                ["explanation"] = new JsonObject()
                {
                    ["type"] = "string",
                    ["description"] = "A short plain-text explanation of how the filter meets the prompt, naming anything you could not express and how many stories the last check matched.",
                },
            },
            ["required"] = new JsonArray("settings", "extendedQuery", "explanation"),
        });

    /// <summary>Finish a chat with the answer.</summary>
    public static readonly LlmFunction AnswerFunction = new(
        Answer,
        "Give your answer to the user's question. Call it on its own, after the results of every other call have come back; it is the only way to finish.",
        new JsonObject()
        {
            ["type"] = "object",
            ["properties"] = new JsonObject()
            {
                ["reply"] = new JsonObject() { ["type"] = "string", ["description"] = "Your answer, as plain text or markdown." },
                ["prompt"] = new JsonObject()
                {
                    ["type"] = new JsonArray("string", "null"),
                    ["description"] = "The complete replacement prompt as HTML using only <p>, <ul>, <ol>, <li>, <strong>, <em> and <br>, or null when you do not propose one.",
                },
            },
            ["required"] = new JsonArray("reply", "prompt"),
            ["additionalProperties"] = false,
        },
        Strict: true);
    #endregion
}
