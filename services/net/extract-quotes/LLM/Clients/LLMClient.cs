using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Http;
using TNO.Services.ExtractQuotes.Config;
using TNO.Services.ExtractQuotes.LLM.Models;

namespace TNO.Services.ExtractQuotes.LLM.Clients;

/// <summary>
/// LLMClient class, provides functionality to call LLM APIs compatible with OpenAI format
/// </summary>
public class LLMClient : ILLMClient
{
    #region Variables
    private readonly IHttpRequestClient _httpClient;
    private readonly ILogger<LLMClient> _logger;
    private readonly ExtractQuotesOptions _options;
    private int _failureCount = 0;

    /// <summary>
    /// Thread logging information
    /// </summary>
    /// <returns>string</returns>
    private static string GetThreadInfo()
    {
        return $"[Thread {Environment.CurrentManagedThreadId}]";
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a LLMClient object, initializes with specified parameters
    /// </summary>
    /// <param name="httpClient">HTTP client for making API requests</param>
    /// <param name="options">Service configuration options</param>
    /// <param name="logger">Logger for this client</param>
    public LLMClient(
        IHttpRequestClient httpClient,
        IOptions<ExtractQuotesOptions> options,
        ILogger<LLMClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Retry the specified request if it fails
    /// </summary>
    /// <typeparam name="T">Type of response expected</typeparam>
    /// <param name="callbackDelegate">The function to retry</param>
    /// <returns>The result of the function call</returns>
    private async Task<T?> RetryRequestAsync<T>(Func<Task<T?>> callbackDelegate)
    {
        try
        {
            return await callbackDelegate();
        }
        catch (Exception ex)
        {
            // Use Interlocked.Increment for thread-safe increment and wrap-around
            int currentFailureCount = Interlocked.Increment(ref _failureCount);
            if (_options.RetryLimit <= currentFailureCount)
            {
                Interlocked.Exchange(ref _failureCount, 0); // Reset the counter
                throw;
            }

            // Wait before retrying
            _logger.LogError(ex, "LLM API retry attempt {count}.{newline}Error:{body}",
                currentFailureCount, Environment.NewLine, ex.Data["Body"]);
            await Task.Delay(_options.RetryDelayMS);
            return await RetryRequestAsync<T>(callbackDelegate);
        }
    }

    /// <summary>
    /// Helper method that handles creating the full request payload with prompt and calling the LLM API
    /// </summary>
    /// <param name="text">The original text to be processed</param>
    /// <param name="prompt">The prompt to send to the LLM</param>
    /// <param name="modelName">The name of the model to use</param>
    /// <param name="apiKey">The API key to use for authentication</param>
    /// <returns>The raw response content from the LLM API, or null if the request failed</returns>
    public async Task<string?> CallLLMApiWithPrompt(string text, string prompt, string modelName, string apiKey)
    {
        // Create the request body for the LLM API
        var requestBody = new
        {
            model = modelName,
            messages = new[]
            {
                new { role = "system", content = "You are a helpful assistant that extracts quotes from text." },
                new { role = "user", content = prompt }
            },
            temperature = 1.0,
            max_tokens = 4000
        };

        // Convert the request body to JSON
        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Determine which API URL to use based on the model name
        string apiUrl;

        // Check if we're using the primary model
        if (modelName == _options.LLM.Primary.ModelName)
        {
            apiUrl = _options.LLM.Primary.ApiUrl;
        }
        // Check if we're using the fallback model
        else if (modelName == _options.LLM.Fallback.ModelName)
        {
            apiUrl = _options.LLM.Fallback.ApiUrl;
        }
        // If neither, throw an exception
        else
        {
            throw new ArgumentException($"No API URL configured for model {modelName}");
        }

        // Set up the headers for the request
        var requestMessage = new HttpRequestMessage();
        requestMessage.Headers.Add("Authorization", $"Bearer {apiKey}");
        var headers = requestMessage.Headers;

        try
        {
            // Send the request to the LLM API
            if (string.IsNullOrEmpty(apiUrl))
            {
                throw new ArgumentException("Argument 'url' must be a valid URL.");
            }

            _logger.LogInformation("{ThreadInfo} Sending request to LLM API: {url} with model: {model}",
                GetThreadInfo(), apiUrl, modelName);

            // Call using original method with retry logic
            LLMResponse? response = await RetryRequestAsync(async () =>
                await _httpClient.SendAsync<LLMResponse>(apiUrl, HttpMethod.Post, headers, content));

            if (response == null)
            {
                _logger.LogWarning("LLM API call to model '{Model}' returned null response.", modelName);
                return null;
            }

            // Extract the content from the response
            var responseContent = response.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrEmpty(responseContent))
            {
                _logger.LogWarning("LLM response from model '{Model}' did not contain message content.", modelName);
                return null;
            }

            return responseContent;
        }
        catch (HttpRequestException ex)
        {
            // Handle HTTP-level errors (network, DNS, non-success status codes)
            _logger.LogError(ex, "HTTP error during LLM API call for model '{Model}'. Status Code: {StatusCode}", modelName, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during LLM API call for model '{Model}'.", modelName);
            throw;
        }
    }

    /// <summary>
    /// Gets the next API key from the list in a thread-safe, round-robin manner
    /// </summary>
    /// <param name="keys">List of API keys</param>
    /// <param name="indexField">Reference to the index field for this list</param>
    /// <returns>The next API key</returns>
    public string GetNextApiKey(List<string> keys, ref int indexField)
    {
        if (keys == null || keys.Count == 0)
        {
            throw new InvalidOperationException("No API keys available for rotation.");
        }
        // Interlocked.Increment safely increments the indexField
        int currentIndex = Interlocked.Increment(ref indexField);
        // List<string> apiKeys = new List<string> { "key1", "key2", "key3" };
        // (currentIndex - 1) % keys.Count = (1 - 1) % 3 = 0 % 3 = 0 => key1
        // (currentIndex - 1) % keys.Count = (2 - 1) % 3 = 1 % 3 = 1 => key2
        // this help get key never exceed the list
        return keys[(currentIndex - 1) % keys.Count];
    }
    #endregion
}
