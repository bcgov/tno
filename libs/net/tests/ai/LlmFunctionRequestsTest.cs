using System.Net;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TNO.AI;

namespace TNO.Test.AI;

/// <summary>
/// Tests for native function calling: the request each API expects, reading function calls and
/// reasoning back, and how the client handles a deployment that rejects parts of the request.
/// </summary>
public class LlmFunctionRequestsTest
{
    #region Helpers
    private static readonly LlmFunction Lookup = new("lookup", "Find values.",
        new JsonObject() { ["type"] = "object", ["properties"] = new JsonObject() { ["text"] = new JsonObject() { ["type"] = "string" } }, ["required"] = new JsonArray("text"), ["additionalProperties"] = false },
        Strict: true);

    private static readonly LlmFunctionRequestShape Shape = new(1000, null, false, ParallelToolCalls: true, Stateless: true, IncludeReasoning: true);

    private static readonly LlmItem[] Conversation =
    [
        new LlmTextItem("system", "You build filters."),
        new LlmTextItem("user", "CBC stories"),
        new LlmProviderItem("{\"type\":\"reasoning\",\"id\":\"rs_1\",\"encrypted_content\":\"abc\",\"summary\":[]}"),
        new LlmFunctionCall("call_1", "lookup", "{\"text\":\"cbc\"}"),
        new LlmFunctionCall("call_2", "lookup", "{\"text\":\"global\"}"),
        new LlmFunctionOutput("call_1", "{\"id\":1}"),
        new LlmFunctionOutput("call_2", "{\"id\":2}"),
    ];

    /// <summary>
    /// SequenceHandler class, answers each request with the next response and records the bodies sent.
    /// </summary>
    private class SequenceHandler : HttpMessageHandler
    {
        private readonly Queue<(HttpStatusCode Status, string Body)> _responses;
        public List<JsonObject> Requests { get; } = new();

        public SequenceHandler(params (HttpStatusCode, string)[] responses)
        {
            _responses = new Queue<(HttpStatusCode, string)>(responses);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(JsonNode.Parse(await request.Content!.ReadAsStringAsync(cancellationToken))!.AsObject());
            var (status, body) = _responses.Dequeue();
            return new HttpResponseMessage(status) { Content = new StringContent(body) };
        }
    }

    private static readonly LlmEndpoint Responses = new(new Uri("https://example.test/openai/v1/responses"), "key", "gpt");
    private static readonly LlmEndpoint Chat = new(new Uri("https://example.test/openai/v1/chat/completions"), "key", "gpt");

    private const string ResponsesReply = "{\"status\":\"completed\",\"output\":[{\"type\":\"reasoning\",\"id\":\"rs_2\",\"encrypted_content\":\"xyz\",\"summary\":[]},{\"type\":\"function_call\",\"id\":\"fc_1\",\"call_id\":\"call_3\",\"name\":\"lookup\",\"arguments\":\"{\\\"text\\\":\\\"cbc\\\"}\"},{\"type\":\"function_call\",\"id\":\"fc_2\",\"call_id\":\"call_4\",\"name\":\"lookup\",\"arguments\":\"{\\\"text\\\":\\\"ctv\\\"}\"}],\"usage\":{\"input_tokens\":100,\"output_tokens\":20}}";
    #endregion

    [Fact]
    public void ResponsesRequestSendsFunctionsStatelessWithEveryItemInOrder()
    {
        var request = JsonNode.Parse(LlmFunctionRequests.Build("gpt", Conversation, [Lookup], true, true, Shape))!.AsObject();

        request["tool_choice"]!.GetValue<string>().Should().Be("required");
        request["parallel_tool_calls"]!.GetValue<bool>().Should().BeTrue();
        request["store"]!.GetValue<bool>().Should().BeFalse();
        request["include"]!.AsArray().Single()!.GetValue<string>().Should().Be("reasoning.encrypted_content");
        request["max_output_tokens"]!.GetValue<int>().Should().Be(1000);
        var tool = request["tools"]!.AsArray().Single()!;
        tool["type"]!.GetValue<string>().Should().Be("function");
        tool["name"]!.GetValue<string>().Should().Be("lookup");
        tool["strict"]!.GetValue<bool>().Should().BeTrue();

        var input = request["input"]!.AsArray();
        input.Select(i => i!["type"]?.GetValue<string>() ?? i!["role"]!.GetValue<string>())
            .Should().Equal("system", "user", "reasoning", "function_call", "function_call", "function_call_output", "function_call_output");
        input[2]!["encrypted_content"]!.GetValue<string>().Should().Be("abc");
        input[3]!["call_id"]!.GetValue<string>().Should().Be("call_1");
        input[6]!["output"]!.GetValue<string>().Should().Be("{\"id\":2}");
    }

    [Fact]
    public void ChatRequestGroupsCallsIntoOneAssistantMessageAndAnswersEachAsATool()
    {
        var request = JsonNode.Parse(LlmFunctionRequests.Build("gpt", Conversation, [Lookup], true, false, Shape with { Stateless = false, IncludeReasoning = false }))!.AsObject();

        request.ContainsKey("store").Should().BeFalse();
        request["max_completion_tokens"]!.GetValue<int>().Should().Be(1000);
        request["tools"]!.AsArray().Single()!["function"]!["name"]!.GetValue<string>().Should().Be("lookup");

        var messages = request["messages"]!.AsArray();
        messages.Select(m => m!["role"]!.GetValue<string>()).Should().Equal("system", "user", "assistant", "tool", "tool");
        var calls = messages[2]!["tool_calls"]!.AsArray();
        calls.Select(c => c!["id"]!.GetValue<string>()).Should().Equal("call_1", "call_2");
        calls[1]!["function"]!["arguments"]!.GetValue<string>().Should().Be("{\"text\":\"global\"}");
        messages[4]!["tool_call_id"]!.GetValue<string>().Should().Be("call_2");
    }

    [Fact]
    public void ResponsesReplyReturnsEveryCallAndKeepsReasoningToSendBack()
    {
        var result = LlmFunctionRequests.Parse(ResponsesReply, true, 1);

        result.Calls.Select(c => (c.CallId, c.Arguments)).Should().Equal(("call_3", "{\"text\":\"cbc\"}"), ("call_4", "{\"text\":\"ctv\"}"));
        result.Items.Should().HaveCount(3);
        result.Items[0].Should().BeOfType<LlmProviderItem>().Which.Json.Should().Contain("\"encrypted_content\":\"xyz\"");
        result.FinishReason.Should().Be("tool_calls");
        result.PromptTokens.Should().Be(100);
    }

    [Fact]
    public void ChatReplyReturnsTheToolCallsAndTheText()
    {
        var result = LlmFunctionRequests.Parse("{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"Checking.\",\"tool_calls\":[{\"id\":\"call_9\",\"type\":\"function\",\"function\":{\"name\":\"lookup\",\"arguments\":\"{}\"}}]},\"finish_reason\":\"tool_calls\"}]}", false, 1);

        result.Text.Should().Be("Checking.");
        result.Calls.Single().CallId.Should().Be("call_9");
        result.Items.Select(i => i.GetType().Name).Should().Equal(nameof(LlmTextItem), nameof(LlmFunctionCall));
        result.IsTruncated.Should().BeFalse();
    }

    [Fact]
    public async Task ADeploymentThatRejectsParallelCallsIsRetriedWithoutThem()
    {
        var handler = new SequenceHandler(
            (HttpStatusCode.BadRequest, "{\"error\":{\"message\":\"Unsupported parameter: 'parallel_tool_calls'.\"}}"),
            (HttpStatusCode.OK, ResponsesReply));
        var client = new LlmDirectClient(new HttpClient(handler), NullLogger.Instance);

        var result = await client.InvokeWithFunctionsAsync(Responses, Conversation, [Lookup], cancellationToken: TestContext.Current.CancellationToken);

        result.Calls.Should().HaveCount(2);
        handler.Requests[0].ContainsKey("parallel_tool_calls").Should().BeTrue();
        handler.Requests[1].ContainsKey("parallel_tool_calls").Should().BeFalse();
    }

    [Fact]
    public async Task ADeploymentThatRejectsToolsThrowsAFunctionCallingException()
    {
        var handler = new SequenceHandler((HttpStatusCode.BadRequest, "{\"error\":{\"message\":\"The model does not support 'tools'.\"}}"));
        var client = new LlmDirectClient(new HttpClient(handler), NullLogger.Instance);

        var act = () => client.InvokeWithFunctionsAsync(Chat, Conversation, [Lookup]);

        (await act.Should().ThrowAsync<LlmFunctionCallingException>()).Which.Message.Should().Contain("does not support 'tools'");
    }
}
