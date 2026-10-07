using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TNO.AI;

/// <summary>
/// LlmEndpoint record, the connection details of a deployment-based (API key) LLM.
/// </summary>
/// <param name="Endpoint">The project endpoint URL.</param>
/// <param name="ApiKey">The API key.</param>
/// <param name="DeploymentName">The model deployment name.</param>
public record LlmEndpoint(Uri Endpoint, string ApiKey, string DeploymentName);

/// <summary>
/// LlmResult record, one completed LLM exchange with its reported token usage.
/// </summary>
/// <param name="Content">The response text (the first choice).</param>
/// <param name="PromptTokens">Prompt tokens reported by the provider (null when unavailable).</param>
/// <param name="CompletionTokens">Completion tokens reported by the provider.</param>
/// <param name="Attempts">How many attempts the request took.</param>
public record LlmResult(string Content, int? PromptTokens, int? CompletionTokens, int Attempts)
{
    /// <summary>
    /// get/init - Why the model stopped ('stop', 'length', ...), when the provider reports it.
    /// </summary>
    public string? FinishReason { get; init; }

    /// <summary>
    /// get/init - Every choice's text, in order (one unless several were requested).
    /// </summary>
    public IReadOnlyList<string> Choices { get; init; } = Array.Empty<string>();

    /// <summary>
    /// get - The response stopped at the output token limit, so its text is incomplete.
    /// </summary>
    public bool IsTruncated => FinishReason == "length" || FinishReason == "max_output_tokens";
}

/// <summary>
/// LlmRequestOptions record, optional request settings.
/// </summary>
/// <param name="MaxOutputTokens">The most tokens the response may contain.</param>
/// <param name="Temperature">The sampling temperature; omitted when null.</param>
/// <param name="ChoiceCount">The number of choices to generate (chat completions only).</param>
public record LlmRequestOptions(int? MaxOutputTokens = null, float? Temperature = null, int? ChoiceCount = null);

/// <summary>
/// LlmContextLengthException class, the provider rejected a request because it exceeds the model's
/// context window. Callers split the input and retry.
/// </summary>
public class LlmContextLengthException : HttpRequestException
{
    /// <summary>
    /// Creates a new instance of a LlmContextLengthException.
    /// </summary>
    /// <param name="message"></param>
    public LlmContextLengthException(string message) : base(message, null, System.Net.HttpStatusCode.BadRequest) { }
}

/// <summary>
/// ILlmClient interface, sends chat requests to a model deployment.
/// </summary>
public interface ILlmClient
{
    /// <summary>
    /// Send the specified conversation and return the response with token usage.
    /// </summary>
    /// <param name="llm">The endpoint to call.</param>
    /// <param name="messages">The conversation: roles 'system', 'user', 'assistant'.</param>
    /// <param name="jsonMode">Request structured JSON output.</param>
    /// <param name="attempts">Total attempts before the last failure is thrown.</param>
    /// <param name="options">Optional request settings.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<LlmResult> InvokeAsync(
        LlmEndpoint llm,
        IReadOnlyList<(string Role, string Content)> messages,
        bool jsonMode = false,
        int attempts = 3,
        LlmRequestOptions? options = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// LlmDirectClient class, sends chat requests to a deployment-based LLM endpoint.
/// The endpoint path determines the request shape: the Responses API uses 'input' with typed
/// content parts, the classic chat-completions API uses 'messages'. Supports JSON mode
/// (structured output) with automatic fallback for deployments that reject response_format.
/// Throttling (429), server errors, timeouts, and connection failures are retried. A request that
/// exceeds the model's context window throws LlmContextLengthException.
/// </summary>
public class LlmDirectClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new instance of an LlmDirectClient.
    /// </summary>
    /// <param name="httpClient">The HttpClient to send requests with (its Timeout applies per attempt).</param>
    /// <param name="logger"></param>
    public LlmDirectClient(HttpClient httpClient, ILogger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Send the specified conversation and return the response with token usage.
    /// </summary>
    /// <param name="llm">The endpoint to call.</param>
    /// <param name="messages">The conversation: roles 'system', 'user', 'assistant'.</param>
    /// <param name="jsonMode">Request structured JSON output.</param>
    /// <param name="attempts">Total attempts before the last failure is thrown.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<LlmResult> InvokeAsync(
        LlmEndpoint llm,
        IReadOnlyList<(string Role, string Content)> messages,
        bool jsonMode,
        int attempts,
        CancellationToken cancellationToken)
        => InvokeAsync(llm, messages, jsonMode, attempts, null, cancellationToken);

    /// <summary>
    /// Send the specified conversation and return the response with token usage.
    /// </summary>
    /// <param name="llm">The endpoint to call.</param>
    /// <param name="messages">The conversation: roles 'system', 'user', 'assistant'.</param>
    /// <param name="jsonMode">Request structured JSON output.</param>
    /// <param name="attempts">Total attempts before the last failure is thrown.</param>
    /// <param name="options">Optional request settings.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<LlmResult> InvokeAsync(
        LlmEndpoint llm,
        IReadOnlyList<(string Role, string Content)> messages,
        bool jsonMode = false,
        int attempts = 3,
        LlmRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var isResponsesApi = llm.Endpoint.AbsolutePath.Contains("/responses", StringComparison.OrdinalIgnoreCase);
        var request = new RequestShape(jsonMode, options?.MaxOutputTokens, options?.Temperature, isResponsesApi ? null : options?.ChoiceCount);
        var requestJson = BuildRequest(llm.DeploymentName, messages, isResponsesApi, request);
        attempts = Math.Max(1, attempts);

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                // A request message can only be sent once; build a fresh one per attempt.
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, llm.Endpoint);
                httpRequest.Headers.Add("api-key", llm.ApiKey);
                httpRequest.Content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

                var status = (int)response.StatusCode;
                if ((status == 429 || status >= 500) && attempt < attempts)
                {
                    _logger.LogWarning("LLM request failed ({status}); retrying attempt {next} of {attempts}.", status, attempt + 1, attempts);
                    await Task.Delay(TimeSpan.FromSeconds(5 * attempt), cancellationToken);
                    continue;
                }
                if (status == 400 && IsContextLengthError(responseJson))
                    throw new LlmContextLengthException($"LLM request exceeds the context window: {Truncate(responseJson, 500)}");

                // Some deployments reject optional parameters; drop the one named and retry.
                if (status == 400 && TryRelax(ref request, responseJson))
                {
                    requestJson = BuildRequest(llm.DeploymentName, messages, isResponsesApi, request);
                    continue;
                }
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"LLM request failed ({status}): {Truncate(responseJson, 500)}", null, response.StatusCode);

                return Parse(responseJson, isResponsesApi, attempt);
            }
            catch (TaskCanceledException) when (attempt < attempts && !cancellationToken.IsCancellationRequested)
            {
                // The request timed out (HttpClient.Timeout); retry.
                _logger.LogWarning("LLM request timed out; retrying attempt {next} of {attempts}.", attempt + 1, attempts);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == null && attempt < attempts)
            {
                // A connection-level failure (DNS, socket reset); retry.
                _logger.LogWarning(ex, "LLM request failed to connect; retrying attempt {next} of {attempts}.", attempt + 1, attempts);
                await Task.Delay(TimeSpan.FromSeconds(5 * attempt), cancellationToken);
            }
        }
    }

    /// <summary>
    /// The optional parts of a request, relaxed one at a time when a deployment rejects them.
    /// </summary>
    private record RequestShape(bool JsonMode, int? MaxOutputTokens, float? Temperature, int? ChoiceCount, bool UseLegacyMaxTokens = false);

    /// <summary>
    /// Whether a 400 response says the request exceeds the model's context window.
    /// </summary>
    /// <param name="responseJson"></param>
    /// <returns></returns>
    public static bool IsContextLengthError(string responseJson)
    {
        return responseJson.Contains("context_length_exceeded", StringComparison.OrdinalIgnoreCase)
            || responseJson.Contains("maximum context length", StringComparison.OrdinalIgnoreCase)
            || responseJson.Contains("too many tokens", StringComparison.OrdinalIgnoreCase)
            || responseJson.Contains("exceeds the context window", StringComparison.OrdinalIgnoreCase);
    }

    private bool TryRelax(ref RequestShape request, string responseJson)
    {
        if (request.JsonMode && responseJson.Contains("format", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("LLM rejected the structured-output request; retrying without JSON mode.");
            request = request with { JsonMode = false };
            return true;
        }
        if (request.Temperature.HasValue && responseJson.Contains("temperature", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("LLM rejected the temperature; retrying without it.");
            request = request with { Temperature = null };
            return true;
        }
        if (request.MaxOutputTokens.HasValue && !request.UseLegacyMaxTokens && responseJson.Contains("max_completion_tokens", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("LLM rejected 'max_completion_tokens'; retrying with 'max_tokens'.");
            request = request with { UseLegacyMaxTokens = true };
            return true;
        }
        if (request.ChoiceCount > 1 && responseJson.Contains("\"n\"", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("LLM rejected several choices; retrying with one.");
            request = request with { ChoiceCount = null };
            return true;
        }
        return false;
    }

    private static string BuildRequest(string deployment, IReadOnlyList<(string Role, string Content)> messages, bool isResponsesApi, RequestShape shape)
    {
        var request = new Dictionary<string, object?>
        {
            ["model"] = deployment,
        };
        if (isResponsesApi)
        {
            request["input"] = messages.Select(message => (object)new
            {
                role = message.Role,
                content = new object[]
                {
                    new
                    {
                        type = message.Role == "assistant" ? "output_text" : "input_text",
                        text = message.Content,
                    },
                },
            }).ToArray();
            if (shape.JsonMode) request["text"] = new { format = new { type = "json_object" } };
            if (shape.MaxOutputTokens.HasValue) request["max_output_tokens"] = shape.MaxOutputTokens;
        }
        else
        {
            request["messages"] = messages.Select(message => (object)new { role = message.Role, content = message.Content }).ToArray();
            if (shape.JsonMode) request["response_format"] = new { type = "json_object" };
            if (shape.MaxOutputTokens.HasValue) request[shape.UseLegacyMaxTokens ? "max_tokens" : "max_completion_tokens"] = shape.MaxOutputTokens;
            if (shape.ChoiceCount > 1) request["n"] = shape.ChoiceCount;
        }
        if (shape.Temperature.HasValue) request["temperature"] = shape.Temperature;
        return JsonSerializer.Serialize(request);
    }

    private static LlmResult Parse(string responseJson, bool isResponsesApi, int attempt)
    {
        using var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;
        var choices = new List<string>();
        string? finishReason = null;
        int? promptTokens = null;
        int? completionTokens = null;

        if (isResponsesApi)
        {
            // Prefer the aggregated 'output_text'; otherwise walk output[].content[].text.
            if (root.TryGetProperty("output_text", out var outputText) && outputText.ValueKind == JsonValueKind.String)
                choices.Add(outputText.GetString() ?? "");
            else if (root.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Array)
            {
                var parts = new List<string>();
                foreach (var item in output.EnumerateArray())
                {
                    if (!item.TryGetProperty("content", out var itemContent) || itemContent.ValueKind != JsonValueKind.Array) continue;
                    foreach (var part in itemContent.EnumerateArray())
                        if (part.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                            parts.Add(text.GetString() ?? "");
                }
                choices.Add(String.Join("", parts));
            }
            if (root.TryGetProperty("status", out var status) && status.GetString() == "incomplete"
                && root.TryGetProperty("incomplete_details", out var details) && details.ValueKind == JsonValueKind.Object
                && details.TryGetProperty("reason", out var reason))
                finishReason = reason.GetString();
            else if (root.TryGetProperty("status", out var completed))
                finishReason = completed.GetString() == "completed" ? "stop" : completed.GetString();
            if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
            {
                if (usage.TryGetProperty("input_tokens", out var input) && input.ValueKind == JsonValueKind.Number) promptTokens = input.GetInt32();
                if (usage.TryGetProperty("output_tokens", out var output2) && output2.ValueKind == JsonValueKind.Number) completionTokens = output2.GetInt32();
            }
        }
        else
        {
            if (root.TryGetProperty("choices", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                foreach (var choice in items.EnumerateArray())
                {
                    choices.Add(choice.TryGetProperty("message", out var inner) && inner.TryGetProperty("content", out var text) && text.ValueKind == JsonValueKind.String
                        ? text.GetString() ?? ""
                        : "");
                    // A truncated choice makes the whole response incomplete.
                    if (choice.TryGetProperty("finish_reason", out var reason) && reason.ValueKind == JsonValueKind.String
                        && (finishReason == null || reason.GetString() == "length"))
                        finishReason = reason.GetString();
                }
            }
            if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
            {
                if (usage.TryGetProperty("prompt_tokens", out var prompt) && prompt.ValueKind == JsonValueKind.Number) promptTokens = prompt.GetInt32();
                if (usage.TryGetProperty("completion_tokens", out var completion) && completion.ValueKind == JsonValueKind.Number) completionTokens = completion.GetInt32();
            }
        }

        return new LlmResult(choices.FirstOrDefault() ?? "", promptTokens, completionTokens, attempt)
        {
            FinishReason = finishReason,
            Choices = choices,
        };
    }

    private static string Truncate(string value, int length)
        => value.Length <= length ? value : value[..length];
}
