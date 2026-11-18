using System.Net.Http;
using System.Text.Json.Nodes;

namespace TNO.AI;

/// <summary>
/// LlmFunction record, a function the model may call (native function calling).
/// </summary>
/// <param name="Name">The function name the model calls.</param>
/// <param name="Description">What the function does and when to call it.</param>
/// <param name="Parameters">The JSON schema of the function's arguments.</param>
/// <param name="Strict">Whether the provider must constrain the arguments to the schema. A strict
/// schema lists every property as required and sets additionalProperties to false, so it cannot
/// describe open-ended objects such as an Elasticsearch query.</param>
public record LlmFunction(string Name, string Description, JsonObject Parameters, bool Strict = false);

/// <summary>
/// LlmItem record, one item of a function-calling conversation.
/// </summary>
public abstract record LlmItem;

/// <summary>
/// LlmTextItem record, a 'system', 'user' or 'assistant' message.
/// </summary>
/// <param name="Role"></param>
/// <param name="Content"></param>
public record LlmTextItem(string Role, string Content) : LlmItem;

/// <summary>
/// LlmFunctionCall record, a function the model called.
/// </summary>
/// <param name="CallId">The provider's id for the call; its output must quote it.</param>
/// <param name="Name">The function called.</param>
/// <param name="Arguments">The arguments as the model wrote them (a JSON string).</param>
public record LlmFunctionCall(string CallId, string Name, string Arguments) : LlmItem;

/// <summary>
/// LlmFunctionOutput record, the result of a function call, returned to the model.
/// </summary>
/// <param name="CallId">The id of the call this answers.</param>
/// <param name="Output"></param>
public record LlmFunctionOutput(string CallId, string Output) : LlmItem;

/// <summary>
/// LlmProviderItem record, a provider item that must be sent back unchanged on the next request,
/// i.e. a Responses API reasoning item with its encrypted content.
/// </summary>
/// <param name="Json">The item as the provider returned it.</param>
public record LlmProviderItem(string Json) : LlmItem;

/// <summary>
/// LlmFunctionResult record, a response to a function-calling request.
/// </summary>
/// <param name="Text">The text the model wrote, if any.</param>
/// <param name="Calls">The functions the model called, in order.</param>
/// <param name="Items">Everything the model returned, in order, to append to the conversation.</param>
/// <param name="PromptTokens"></param>
/// <param name="CompletionTokens"></param>
/// <param name="Attempts">The attempt that succeeded.</param>
public record LlmFunctionResult(string Text, IReadOnlyList<LlmFunctionCall> Calls, IReadOnlyList<LlmItem> Items, int? PromptTokens, int? CompletionTokens, int Attempts)
{
    /// <summary>
    /// get/init - Why the model stopped ('stop', 'tool_calls', 'length', 'max_output_tokens', 'content_filter').
    /// </summary>
    public string? FinishReason { get; init; }

    /// <summary>
    /// get - Whether the response was cut off at the output token limit.
    /// </summary>
    public bool IsTruncated => FinishReason == "length" || FinishReason == "max_output_tokens";

    /// <summary>
    /// get - Whether the provider's content filter blocked the response.
    /// </summary>
    public bool IsContentFiltered => FinishReason == "content_filter";

    /// <summary>
    /// get/init - The model's refusal, when it declined to answer.
    /// </summary>
    public string? Refusal { get; init; }
}

/// <summary>
/// LlmFunctionCallingException class, the deployment rejected the function definitions or the
/// request to call them, i.e. because the model does not support function calling.
/// </summary>
public class LlmFunctionCallingException : HttpRequestException
{
    /// <summary>
    /// Creates a new instance of an LlmFunctionCallingException.
    /// </summary>
    /// <param name="message"></param>
    public LlmFunctionCallingException(string message) : base(message, null, System.Net.HttpStatusCode.BadRequest) { }
}
