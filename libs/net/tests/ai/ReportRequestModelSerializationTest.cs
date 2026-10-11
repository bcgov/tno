using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using TNO.API.Models.Settings;
using TNO.Entities;
using TNO.Kafka.Models;

namespace TNO.Test.AI;

/// <summary>
/// Tests that a report request carrying an AI section test round-trips through the Kafka JSON
/// options, and that older messages without one still deserialize.
/// </summary>
public class ReportRequestModelSerializationTest
{
    private static JsonSerializerOptions Options() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    [Fact]
    public void MessagesWithoutATestDeserializeAsBefore()
    {
        var json = "{\"destination\":\"ReportingService\",\"reportType\":\"Content\",\"reportId\":1,\"reportInstanceId\":5,\"data\":{},\"prepareAISections\":true,\"requestorId\":7}";

        var model = JsonSerializer.Deserialize<ReportRequestModel>(json, Options())!;

        model.AISectionTest.Should().BeNull();
        model.PrepareAISections.Should().BeTrue();
        model.RequestorId.Should().Be(7);
    }

    [Fact]
    public void TestRequestsRoundTrip()
    {
        var request = new ReportRequestModel(ReportDestination.ReportingService, ReportType.Content, 1, 5, JsonDocument.Parse("{}"))
        {
            PrepareAISections = true,
            RequestorId = 7,
            AIViewContentUrl = "https://mmi.test/view/",
            AISectionTest = new()
            {
                TestId = Guid.NewGuid(),
                SectionId = 9,
                Settings = new ReportSectionSettingsModel()
                {
                    Label = "Summary",
                    UserPrompt = "<p>Hi &amp; bye</p>",
                    AIInputFields = new[] { "headline", "summary" },
                    Temperature = 0.5f,
                    AIScope = "Sections",
                    SourceSections = new[] { "news" },
                    IncludePreviousReports = 2,
                    LLMId = 3,
                },
            },
        };

        var json = JsonSerializer.Serialize(request, Options());
        var copy = JsonSerializer.Deserialize<ReportRequestModel>(json, Options())!;

        copy.AISectionTest.Should().NotBeNull();
        copy.AISectionTest!.TestId.Should().Be(request.AISectionTest!.TestId);
        copy.AISectionTest.SectionId.Should().Be(9);
        copy.AISectionTest.Settings.UserPrompt.Should().Be("<p>Hi &amp; bye</p>");
        copy.AISectionTest.Settings.AIInputFields.Should().Equal("headline", "summary");
        copy.AISectionTest.Settings.Temperature.Should().Be(0.5f);
        copy.AISectionTest.Settings.SourceSections.Should().Equal("news");
        copy.AISectionTest.Settings.IncludePreviousReports.Should().Be(2);
        copy.AISectionTest.Settings.LLMId.Should().Be(3);
        copy.AIViewContentUrl.Should().Be("https://mmi.test/view/");
    }
}
