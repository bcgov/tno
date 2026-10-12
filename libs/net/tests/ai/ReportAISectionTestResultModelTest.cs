using System.Text.Json;
using FluentAssertions;
using TNO.API.Areas.Services.Models.ReportAIResult;
using TNO.Entities;

namespace TNO.Test.AI;

/// <summary>
/// The test result DTO round-trips through the DB entity, transcript included.
/// </summary>
public class ReportAISectionTestResultModelTest
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    [Fact]
    public void RoundTripsThroughTheEntity()
    {
        var model = new ReportAISectionTestResultModel
        {
            TestId = Guid.NewGuid(),
            ReportId = 15,
            ReportInstanceId = 65164,
            SectionId = 9,
            RequestorId = 400,
            Status = ReportAISectionTestStatus.Completed,
            Output = "<p>Done</p>",
            RequestCount = 12,
            PromptTokens = 120097,
            CompletionTokens = 6610,
            DurationMs = 60698,
            StoryCount = 26,
            ReductionDepth = 1,
            Transcript = new[]
            {
                new LlmExchangeModel
                {
                    Stage = "Final", Kind = "Final", Sequence = 12, JsonMode = false, DurationMs = 9000, PromptTokens = 17009, CompletionTokens = 900, EstimatedTokens = 16000,
                    Messages = new[] { new LlmMessageModel { Role = "system", Content = "You write" }, new LlmMessageModel { Role = "user", Content = "## Instructions\nSummarize" } },
                    Response = "<p>Done</p>",
                },
            },
            CompletedOn = new DateTime(2026, 10, 11, 23, 35, 52, DateTimeKind.Utc),
        };

        var entity = model.ToEntity(Options);
        entity.Id.Should().Be(model.TestId);
        entity.Status.Should().Be(ReportAITestStatus.Completed);
        entity.Transcript.Should().Contain("\"stage\":\"Final\"").And.Contain("## Instructions");

        var back = new ReportAISectionTestResultModel(entity, Options);
        back.Should().BeEquivalentTo(model, o => o.Excluding(m => m.CreatedOn));
        back.Transcript.Should().ContainSingle().Which.Messages.Should().HaveCount(2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not json")]
    public void ATranscriptThatCannotBeReadIsEmpty(string? stored)
    {
        var entity = new ReportAITest(Guid.NewGuid(), 15, 65164, 9, 400) { Status = ReportAITestStatus.Failed, Error = "boom", Transcript = stored! };

        var model = new ReportAISectionTestResultModel(entity, Options);

        model.Status.Should().Be(ReportAISectionTestStatus.Failed);
        model.Error.Should().Be("boom");
        model.Transcript.Should().BeEmpty();
    }
}
