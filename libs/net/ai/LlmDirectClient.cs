using System.Net.Http;
using System.Net.Http.Json;
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
/// <param name="Content">The response text.</param>
/// <param name="PromptTokens">Prompt tokens reported by the provider (null when unavailable).</param>
/// <param name="CompletionTokens">Completion tokens reported by the provider.</param>
/// <param name="Attempts">How many attempts the request took.</param>
public record LlmResult(string Content, int? PromptTokens, int? CompletionTokens, int Attempts);

/// <summary>
/// LlmDirectClient class, sends chat requests to a deployment-based LLM endpoint.
/// The endpoint path determines the request shape: the Responses API uses 'input' with typed
/// content parts, the classic chat-completions API uses 'messages'. Supports JSON mode
/// (structured output) with automatic fallback for deployments that reject response_format.
/// Throttling (429), server errors, timeouts, and connection failures are retried.
/// </summary>
public class LlmDirectClient
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
    public async Task<LlmResult> InvokeAsync(
        LlmEndpoint llm,
        IReadOnlyList<(string Role, string Content)> messages,
        bool jsonMode = false,
        int attempts = 3,
        CancellationToken cancellationToken = default)
    {
        var isResponsesApi = llm.Endpoint.AbsolutePath.Contains("/responses", StringComparison.OrdinalIgnoreCase);
        var requestJson = BuildRequest(llm.DeploymentName, messages, isResponsesApi, jsonMode);
        attempts = Math.Max(1, attempts);

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                // A request message can only be sent once; build a fresh one per attempt.
                using var request = new HttpRequestMessage(HttpMethod.Post, llm.Endpoint);
                request.Headers.Add("api-key", llm.ApiKey);
                request.Content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

                var status = (int)response.StatusCode;
                if ((status == 429 || status >= 500) && attempt < attempts)
                {
                    _logger.LogWarning("LLM request failed ({status}); retrying attempt {next} of {attempts}.", status, attempt + 1, attempts);
                    await Task.Delay(TimeSpan.FromSeconds(5 * attempt), cancellationToken);
                    continue;
                }
                // Some deployments reject response_format/text.format; retry once without JSON mode.
                if (status == 400 && jsonMode && responseJson.Contains("format", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("LLM rejected the structured-output request; retrying without JSON mode.");
                    jsonMode = false;
                    requestJson = BuildRequest(llm.DeploymentName, messages, isResponsesApi, jsonMode);
                    continue;
                }
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"LLM request failed ({status}): {Truncate(responseJson, 500)}");

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

    private static string BuildRequest(string deployment, IReadOnlyList<(string Role, string Content)> messages, bool isResponsesApi, bool jsonMode)
    {
        object body;
        if (isResponsesApi)
        {
            var request = new Dictionary<string, object?>
            {
                ["model"] = deployment,
                ["input"] = messages.Select(message => (object)new
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
                }).ToArray(),
            };
            if (jsonMode) request["text"] = new { format = new { type = "json_object" } };
            body = request;
        }
        else
        {
            var request = new Dictionary<string, object?>
            {
                ["model"] = deployment,
                ["messages"] = messages.Select(message => (object)new { role = message.Role, content = message.Content }).ToArray(),
            };
            if (jsonMode) request["response_format"] = new { type = "json_object" };
            body = request;
        }
        return JsonSerializer.Serialize(body);
    }

    private static LlmResult Parse(string responseJson, bool isResponsesApi, int attempt)
    {
        using var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;
        string content = "";
        int? promptTokens = null;
        int? completionTokens = null;

        if (isResponsesApi)
        {
            // Prefer the aggregated 'output_text'; otherwise walk output[].content[].text.
            if (root.TryGetProperty("output_text", out var outputText) && outputText.ValueKind == JsonValueKind.String)
                content = outputText.GetString() ?? "";
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
                content = string.Join("", parts);
            }
            if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
            {
                if (usage.TryGetProperty("input_tokens", out var input) && input.ValueKind == JsonValueKind.Number) promptTokens = input.GetInt32();
                if (usage.TryGetProperty("output_tokens", out var output2) && output2.ValueKind == JsonValueKind.Number) completionTokens = output2.GetInt32();
            }
        }
        else
        {
            if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
            {
                var message = choices[0];
                if (message.TryGetProperty("message", out var inner) && inner.TryGetProperty("content", out var text) && text.ValueKind == JsonValueKind.String)
                    content = text.GetString() ?? "";
            }
            if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
            {
                if (usage.TryGetProperty("prompt_tokens", out var prompt) && prompt.ValueKind == JsonValueKind.Number) promptTokens = prompt.GetInt32();
                if (usage.TryGetProperty("completion_tokens", out var completion) && completion.ValueKind == JsonValueKind.Number) completionTokens = completion.GetInt32();
            }
        }

        return new LlmResult(content, promptTokens, completionTokens, attempt);
    }

    private static string Truncate(string value, int length)
        => value.Length <= length ? value : value[..length];
}
