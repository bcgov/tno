using System.Text.Json;
using System.Text.Json.Nodes;

namespace TNO.AI;

/// <summary>
/// LlmFunctionRequestShape record, the optional parts of a function-calling request.
/// </summary>
/// <param name="MaxOutputTokens"></param>
/// <param name="Temperature"></param>
/// <param name="UseLegacyMaxTokens">Chat completions only: send 'max_tokens' instead of 'max_completion_tokens'.</param>
/// <param name="ParallelToolCalls">Let the model call several functions in one reply.</param>
/// <param name="Stateless">Responses API only: send store: false, so the provider keeps nothing between requests.</param>
/// <param name="IncludeReasoning">Responses API only: ask for encrypted reasoning, which a stateless conversation must send back.</param>
public record LlmFunctionRequestShape(int? MaxOutputTokens, float? Temperature, bool UseLegacyMaxTokens, bool ParallelToolCalls, bool Stateless, bool IncludeReasoning);

/// <summary>
/// LlmFunctionRequests class, builds function-calling requests and reads their responses, for both
/// the Responses API ('input' items) and the chat-completions API ('messages' with 'tool_calls').
/// </summary>
public static class LlmFunctionRequests
{
    /// <summary>
    /// Build the request body.
    /// </summary>
    /// <param name="deployment"></param>
    /// <param name="items"></param>
    /// <param name="functions"></param>
    /// <param name="requireFunctionCall"></param>
    /// <param name="isResponsesApi"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    public static string Build(string deployment, IReadOnlyList<LlmItem> items, IReadOnlyList<LlmFunction> functions, bool requireFunctionCall, bool isResponsesApi, LlmFunctionRequestShape shape)
    {
        var request = new JsonObject() { ["model"] = deployment };
        if (isResponsesApi)
        {
            request["input"] = ResponsesInput(items);
            request["tools"] = new JsonArray(functions.Select(f => (JsonNode)new JsonObject()
            {
                ["type"] = "function",
                ["name"] = f.Name,
                ["description"] = f.Description,
                ["parameters"] = f.Parameters.DeepClone(),
                ["strict"] = f.Strict,
            }).ToArray());
            if (shape.Stateless) request["store"] = false;
            if (shape.IncludeReasoning) request["include"] = new JsonArray("reasoning.encrypted_content");
            if (shape.MaxOutputTokens.HasValue) request["max_output_tokens"] = shape.MaxOutputTokens;
        }
        else
        {
            request["messages"] = ChatMessages(items);
            request["tools"] = new JsonArray(functions.Select(f => (JsonNode)new JsonObject()
            {
                ["type"] = "function",
                ["function"] = new JsonObject()
                {
                    ["name"] = f.Name,
                    ["description"] = f.Description,
                    ["parameters"] = f.Parameters.DeepClone(),
                    ["strict"] = f.Strict,
                },
            }).ToArray());
            if (shape.MaxOutputTokens.HasValue) request[shape.UseLegacyMaxTokens ? "max_tokens" : "max_completion_tokens"] = shape.MaxOutputTokens;
        }
        request["tool_choice"] = requireFunctionCall ? "required" : "auto";
        if (shape.ParallelToolCalls) request["parallel_tool_calls"] = true;
        if (shape.Temperature.HasValue) request["temperature"] = shape.Temperature;
        return request.ToJsonString();
    }

    /// <summary>
    /// The Responses API 'input': messages, function calls and their outputs, and provider items
    /// (reasoning) exactly as they were returned.
    /// </summary>
    private static JsonArray ResponsesInput(IReadOnlyList<LlmItem> items)
    {
        var input = new JsonArray();
        foreach (var item in items)
        {
            switch (item)
            {
                case LlmTextItem message:
                    input.Add(new JsonObject()
                    {
                        ["role"] = message.Role,
                        ["content"] = new JsonArray(new JsonObject()
                        {
                            ["type"] = message.Role == "assistant" ? "output_text" : "input_text",
                            ["text"] = message.Content,
                        }),
                    });
                    break;
                case LlmFunctionCall call:
                    input.Add(new JsonObject()
                    {
                        ["type"] = "function_call",
                        ["call_id"] = call.CallId,
                        ["name"] = call.Name,
                        ["arguments"] = call.Arguments,
                    });
                    break;
                case LlmFunctionOutput output:
                    input.Add(new JsonObject()
                    {
                        ["type"] = "function_call_output",
                        ["call_id"] = output.CallId,
                        ["output"] = output.Output,
                    });
                    break;
                case LlmProviderItem provider:
                    input.Add(JsonNode.Parse(provider.Json));
                    break;
            }
        }
        return input;
    }

    /// <summary>
    /// The chat-completions 'messages'. An assistant's text and the function calls that follow it
    /// form one assistant message with 'tool_calls'; each output is a 'tool' message. Provider items
    /// only exist in the Responses API and are left out.
    /// </summary>
    private static JsonArray ChatMessages(IReadOnlyList<LlmItem> items)
    {
        var messages = new JsonArray();
        JsonObject? assistant = null;
        foreach (var item in items)
        {
            switch (item)
            {
                case LlmTextItem message when message.Role == "assistant":
                    assistant = new JsonObject() { ["role"] = "assistant", ["content"] = message.Content };
                    messages.Add(assistant);
                    break;
                case LlmTextItem message:
                    assistant = null;
                    messages.Add(new JsonObject() { ["role"] = message.Role, ["content"] = message.Content });
                    break;
                case LlmFunctionCall call:
                    if (assistant == null)
                    {
                        assistant = new JsonObject() { ["role"] = "assistant", ["content"] = null };
                        messages.Add(assistant);
                    }
                    if (assistant["tool_calls"] is not JsonArray calls)
                    {
                        calls = new JsonArray();
                        assistant["tool_calls"] = calls;
                    }
                    calls.Add(new JsonObject()
                    {
                        ["id"] = call.CallId,
                        ["type"] = "function",
                        ["function"] = new JsonObject() { ["name"] = call.Name, ["arguments"] = call.Arguments },
                    });
                    break;
                case LlmFunctionOutput output:
                    assistant = null;
                    messages.Add(new JsonObject() { ["role"] = "tool", ["tool_call_id"] = output.CallId, ["content"] = output.Output });
                    break;
            }
        }
        return messages;
    }

    /// <summary>
    /// Read the response: the model's text, its function calls, and every item to append to the
    /// conversation, in order.
    /// </summary>
    /// <param name="responseJson"></param>
    /// <param name="isResponsesApi"></param>
    /// <param name="attempt"></param>
    /// <returns></returns>
    public static LlmFunctionResult Parse(string responseJson, bool isResponsesApi, int attempt)
    {
        using var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;
        var items = new List<LlmItem>();
        var calls = new List<LlmFunctionCall>();
        var text = new List<string>();
        string? finishReason = null;
        string? refusal = null;
        int? promptTokens = null;
        int? completionTokens = null;

        if (isResponsesApi)
        {
            if (root.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in output.EnumerateArray())
                {
                    var type = item.TryGetProperty("type", out var t) && t.ValueKind == JsonValueKind.String ? t.GetString() : null;
                    switch (type)
                    {
                        case "function_call":
                            var call = new LlmFunctionCall(GetString(item, "call_id"), GetString(item, "name"), GetString(item, "arguments"));
                            calls.Add(call);
                            items.Add(call);
                            break;
                        case "reasoning":
                            items.Add(new LlmProviderItem(item.GetRawText()));
                            break;
                        case "message":
                            var parts = new List<string>();
                            if (item.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var part in content.EnumerateArray())
                                {
                                    if (part.TryGetProperty("text", out var partText) && partText.ValueKind == JsonValueKind.String)
                                        parts.Add(partText.GetString() ?? "");
                                    else if (part.TryGetProperty("refusal", out var declined) && declined.ValueKind == JsonValueKind.String)
                                        refusal ??= declined.GetString();
                                }
                            }
                            var message = String.Join("", parts);
                            if (message.Length > 0)
                            {
                                text.Add(message);
                                items.Add(new LlmTextItem("assistant", message));
                            }
                            break;
                    }
                }
            }
            if (root.TryGetProperty("status", out var status) && status.GetString() == "incomplete"
                && root.TryGetProperty("incomplete_details", out var details) && details.ValueKind == JsonValueKind.Object
                && details.TryGetProperty("reason", out var reason))
                finishReason = reason.GetString();
            else if (root.TryGetProperty("status", out var completed))
                finishReason = completed.GetString() == "completed" ? (calls.Count > 0 ? "tool_calls" : "stop") : completed.GetString();
            if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
            {
                if (usage.TryGetProperty("input_tokens", out var input) && input.ValueKind == JsonValueKind.Number) promptTokens = input.GetInt32();
                if (usage.TryGetProperty("output_tokens", out var outputTokens) && outputTokens.ValueKind == JsonValueKind.Number) completionTokens = outputTokens.GetInt32();
            }
        }
        else
        {
            if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
            {
                var choice = choices[0];
                if (choice.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.Object)
                {
                    if (message.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String && !String.IsNullOrEmpty(content.GetString()))
                    {
                        text.Add(content.GetString()!);
                        items.Add(new LlmTextItem("assistant", content.GetString()!));
                    }
                    if (message.TryGetProperty("refusal", out var declined) && declined.ValueKind == JsonValueKind.String)
                        refusal = declined.GetString();
                    if (message.TryGetProperty("tool_calls", out var toolCalls) && toolCalls.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var toolCall in toolCalls.EnumerateArray())
                        {
                            if (!toolCall.TryGetProperty("function", out var function) || function.ValueKind != JsonValueKind.Object) continue;
                            var call = new LlmFunctionCall(GetString(toolCall, "id"), GetString(function, "name"), GetString(function, "arguments"));
                            calls.Add(call);
                            items.Add(call);
                        }
                    }
                }
                if (choice.TryGetProperty("finish_reason", out var reason) && reason.ValueKind == JsonValueKind.String)
                    finishReason = reason.GetString();
            }
            if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
            {
                if (usage.TryGetProperty("prompt_tokens", out var prompt) && prompt.ValueKind == JsonValueKind.Number) promptTokens = prompt.GetInt32();
                if (usage.TryGetProperty("completion_tokens", out var completion) && completion.ValueKind == JsonValueKind.Number) completionTokens = completion.GetInt32();
            }
        }

        return new LlmFunctionResult(String.Join("\n", text), calls, items, promptTokens, completionTokens, attempt)
        {
            FinishReason = finishReason,
            Refusal = refusal,
        };
    }

    private static string GetString(JsonElement element, string property)
        => element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() ?? "" : "";
}
