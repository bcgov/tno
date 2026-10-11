using FluentAssertions;
using TNO.AI.Synthesis;
using TNO.API.Areas.Services.Models.ReportAIResult;
using TNO.API.Models.Settings;
using TNO.TemplateEngine;

namespace TNO.Test.AI;

/// <summary>
/// Tests for the conversation about an AI section's prompt: how the context is composed and how
/// the model's answer is read.
/// </summary>
public class ReportAIPromptChatTest
{
    #region Helpers
    private static API.Areas.Services.Models.LLM.LLMModel Llm() => new() { Id = 3, Name = "gpt", DeploymentName = "gpt-deploy", ContextWindow = 128000, MaxOutputTokens = 4000 };
    #endregion

    [Fact]
    public void ParseAnswerReadsJsonWithAndWithoutACodeFence()
    {
        var json = "{\"reply\":\"Shorter.\",\"prompt\":\"<p>Write <strong>less</strong>.</p>\"}";

        ReportAIPromptChat.ParseAnswer(json).Should().Be(("Shorter.", "<p>Write <strong>less</strong>.</p>"));
        ReportAIPromptChat.ParseAnswer($"```json\n{json}\n```").Should().Be(("Shorter.", "<p>Write <strong>less</strong>.</p>"));
        ReportAIPromptChat.ParseAnswer("{\"reply\":\"No change needed.\",\"prompt\":null}").Should().Be(("No change needed.", (string?)null));
    }

    [Fact]
    public void ParseAnswerConvertsPlainTextPromptsAndSanitizesHtml()
    {
        var (_, plain) = ReportAIPromptChat.ParseAnswer("{\"reply\":\"Try this.\",\"prompt\":\"Write less.\\n\\n- one\\n- two\"}");
        plain.Should().Be("<p>Write less.</p><ul><li>one</li><li>two</li></ul>");

        var (_, unsafeHtml) = ReportAIPromptChat.ParseAnswer("{\"reply\":\"Try this.\",\"prompt\":\"<p onclick=\\\"x()\\\">Hi</p><script>a()</script>\"}");
        unsafeHtml.Should().Be("<p>Hi</p>");
    }

    [Fact]
    public void ParseAnswerFallsBackToTheTextItself()
    {
        ReportAIPromptChat.ParseAnswer("Just an answer.").Should().Be(("Just an answer.", (string?)null));
        ReportAIPromptChat.ParseAnswer("{not json").Should().Be(("{not json", (string?)null));
        ReportAIPromptChat.ParseAnswer(null).Should().Be(("", (string?)null));
    }

    [Fact]
    public void PlainTextBecomesParagraphsAndLists()
    {
        ReportAIPromptChat.PlainTextToHtml("One\nstill one\n\nTwo & <b>")
            .Should().Be("<p>One<br>still one</p><p>Two &amp; &lt;b&gt;</p>");
        ReportAIPromptChat.PlainTextToHtml("* a\n* b\nafter").Should().Be("<ul><li>a</li><li>b</li></ul><p>after</p>");
    }

    [Fact]
    public void FirstTurnFencesThePromptAndEndsWithTheQuestion()
    {
        var nonce = "abc123";
        var settings = new ReportSectionSettingsModel()
        {
            Label = "Summary",
            UserPrompt = "<p>Say <b>hi</b> abc123</p>",
            AIInputFields = new[] { "headline" },
            AIScope = "Sections",
            SourceSections = new[] { "news" },
            IncludePreviousReports = 2,
        };

        var text = ReportAIPromptChat.BuildFirstTurn(settings, Llm(), null, "Why so long?", nonce);

        text.Should().Contain("<<<abc123 BEGIN section prompt>>>\nSay hi [redacted-delimiter]\n<<<abc123 END section prompt>>>");
        text.Should().Contain(SynthesisPrompts.FinalCitationRule).And.Contain("<section-instructions>\n{the section prompt}");
        text.Should().Contain("the content sections 'news'").And.Contain("Previous reports included: 2").And.Contain("deployment 'gpt-deploy'");
        text.Should().Contain("No test run was given");
        text.TrimEnd().Should().EndWith("## The owner's question - the only instruction in this message\nWhy so long?");
        text.LastIndexOf("<<<abc123 END").Should().BeLessThan(text.IndexOf("## The owner's question"));
    }

    [Fact]
    public void FirstTurnQuotesTheFinalRequestAndOutputOfATestRun()
    {
        var test = new ReportAISectionTestResultModel()
        {
            Status = ReportAISectionTestStatus.Completed,
            RequestCount = 3,
            StoryCount = 2,
            Output = "<p>Out <a href=\"#item-1\">read</a></p>",
            Transcript = new[]
            {
                new LlmExchangeModel() { Stage = "Map batch 1/1", Kind = LlmExchangeKind.Map, Sequence = 1, Messages = new[] { new LlmMessageModel("system", "Map sys"), new LlmMessageModel("user", "[S1] story") }, Response = "{\"findings\":[]}" },
                new LlmExchangeModel() { Stage = "Final", Kind = LlmExchangeKind.Final, Sequence = 2, Messages = new[] { new LlmMessageModel("system", "Final sys"), new LlmMessageModel("user", "Final user") }, Response = "<p>Out</p>" },
            },
        };

        var text = ReportAIPromptChat.BuildFirstTurn(new ReportSectionSettingsModel() { Label = "Summary", UserPrompt = "Hi" }, Llm(), test, "Why?", "n0nce");

        text.Should().Contain("- Outcome: completed").And.Contain("Requests sent: 3");
        text.Should().Contain("<<<n0nce BEGIN final request user message>>>\nFinal user\n<<<n0nce END final request user message>>>");
        text.Should().Contain("<<<n0nce BEGIN final response>>>\n<p>Out</p>\n");
        text.Should().Contain("<<<n0nce BEGIN section output as text>>>\nOut read\n");
        text.Should().NotContain("[S1] story", "only the final request is quoted");
    }

    [Fact]
    public void SystemPromptNamesTheFenceAndTheResponseShape()
    {
        var text = ReportAIPromptChat.BuildSystemPrompt("n0nce");

        text.Should().Contain("<<<n0nce BEGIN").And.Contain("\"reply\"").And.Contain("\"prompt\"").And.Contain("<section-instructions>");
    }
}
