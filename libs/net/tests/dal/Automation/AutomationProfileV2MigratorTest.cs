using TNO.API.Areas.Admin.Models.Automation;
using TNO.API.Areas.Admin.Models.Automation.V2;

namespace TNO.Test.DAL.Automation;

public class AutomationProfileV2MigratorTest
{
    #region Helpers
    /// <summary>A miniature of the real 'Morning Process' shape: profile filter, a start
    /// fetch-content step, two content steps with identical prompts, and an end step.</summary>
    private static AutomationProfileModel V1Profile() => new()
    {
        Id = 3,
        Name = "Morning Process",
        SchemaVersion = 1,
        FilterId = 11,
        LLMId = 2,
        Steps = new[]
        {
            new AutomationStepModel
            {
                Id = 1, Name = "CPNEWS", Target = "start", Priority = 0, IsEnabled = true,
                Actions = new[]
                {
                    new AutomationActionModel { Id = 170, Name = "cpnews", ActionType = "fetch-content", FilterId = 18, IsEnabled = true },
                },
            },
            new AutomationStepModel
            {
                Id = 2, Name = "Vancouver Sun", Target = "content", Priority = 1, IsEnabled = true,
                FilterId = 12, ApplyToAutomationFilter = true,
                Prompt = "<p>Shared editorial rules.</p>",
                Actions = new[]
                {
                    new AutomationActionModel { Id = 200, Name = "sentiment", ActionType = "add-sentiment", Prompt = "<p>Rate the sentiment.</p>", ConfirmationStatement = "[SENTIMENT:{value}]", IsEnabled = true },
                    new AutomationActionModel { Id = 201, Name = "duplicates", ActionType = "deduplicate", PriorActionId = 170, IsEnabled = true },
                    new AutomationActionModel { Id = 202, Name = "publish", ActionType = "publish-content", Prompt = "<p>Publish?</p>", ConfirmationStatement = "[PUBLISH CONTENT]", IsEnabled = true },
                },
            },
            new AutomationStepModel
            {
                Id = 3, Name = "Globe & Mail", Target = "content", Priority = 2, IsEnabled = true,
                FilterId = 13, ApplyToAutomationFilter = true,
                Prompt = "<p>Shared editorial rules.</p>",
                Actions = new[]
                {
                    new AutomationActionModel { Id = 210, Name = "sentiment", ActionType = "add-sentiment", Prompt = "<p>Rate the sentiment.</p>", ConfirmationStatement = "[SENTIMENT:{value}]", IsEnabled = true },
                },
            },
            new AutomationStepModel
            {
                Id = 4, Name = "Select Stories", Target = "end", Priority = 3, IsEnabled = true,
                Actions = new[]
                {
                    new AutomationActionModel { Id = 220, Name = "top-stories", ActionType = "select-top", Objective = "top-story", MaxCalls = 10, ContentActionId = 1, IsEnabled = true },
                    new AutomationActionModel { Id = 221, Name = "Morning Report", ActionType = "run-report", ReportId = 5, AutoExecute = true, IsEnabled = true },
                },
            },
        },
    };
    #endregion

    #region Tests
    [Fact]
    public void Migrate_ProducesValidDefinition()
    {
        var result = AutomationProfileV2Migrator.Migrate(V1Profile());
        var errors = AutomationDefinitionValidator.Validate(result.Definition).Where(e => e.Severity == "error").ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Migrate_ProfileFilterBecomesInitSearch()
    {
        var result = AutomationProfileV2Migrator.Migrate(V1Profile());
        var init = result.Definition.Steps.First(s => s.Phase == V2Phases.Init);
        var search = init.Actions.First(a => a.Type == "search" && a.Filter == 11);
        Assert.Equal(AutomationProfileV2Migrator.InboxCollection, search.Into);
    }

    [Fact]
    public void Migrate_FetchContentBecomesNamedCollectionSearch()
    {
        var result = AutomationProfileV2Migrator.Migrate(V1Profile());
        var init = result.Definition.Steps.First(s => s.Phase == V2Phases.Init);
        var search = init.Actions.First(a => a.Type == "search" && a.Filter == 18);
        Assert.Equal("$run.cpnews", search.Into);
    }

    [Fact]
    public void Migrate_DedupeAgainstFetchContentCollection()
    {
        var result = AutomationProfileV2Migrator.Migrate(V1Profile());
        var dedupe = result.Definition.Steps.SelectMany(s => s.Actions).First(a => a.Type == "dedupe");
        // The v1 prior-action reference (170, the fetch-content action) resolves to its collection.
        Assert.Equal("$run.cpnews", dedupe.Against);
    }

    [Fact]
    public void Migrate_GateFilterBecomesSourceInclude()
    {
        var result = AutomationProfileV2Migrator.Migrate(V1Profile());
        var sun = result.Definition.Steps.First(s => s.Name == "Vancouver Sun");
        Assert.Equal(V2Phases.Process, sun.Phase);
        Assert.Equal("collection", sun.Source!.From);
        Assert.Contains(12, sun.Source.Include);
    }

    [Fact]
    public void Migrate_ActionBecomesRawAnalysisWithSameConfirmation()
    {
        var result = AutomationProfileV2Migrator.Migrate(V1Profile());
        var sun = result.Definition.Steps.First(s => s.Name == "Vancouver Sun");
        var sentiment = sun.Actions.First(a => a.Type == "content.sentiment");
        // Same confirmation statement, gated against a raw single-response analysis: the migrated
        // profile issues the same prompts and parses the same way.
        Assert.Equal("[SENTIMENT:{value}]", sentiment.Confirm);
        Assert.NotNull(sentiment.Analysis);
        var analysis = sun.Analyses.First(a => a.Name == sentiment.Analysis);
        Assert.True(analysis.Raw);
    }

    [Fact]
    public void Migrate_IdenticalPromptsCollapseToOneLibraryEntry()
    {
        var result = AutomationProfileV2Migrator.Migrate(V1Profile());
        // Two steps carry the byte-identical step prompt; the library holds it once.
        var entries = result.Definition.Prompts.Values.Count(entry => entry.Text.Contains("Shared editorial rules"));
        Assert.Equal(1, entries);
    }

    [Fact]
    public void Migrate_SelectTopIsDeterministicWithWarning()
    {
        var result = AutomationProfileV2Migrator.Migrate(V1Profile());
        var selectTop = result.Definition.Steps.SelectMany(s => s.Actions).First(a => a.Type == "select-top");
        Assert.Equal(10, selectTop.Take);
        Assert.Equal(1, selectTop.ContentAction);
        Assert.Contains(result.Warnings, w => w.Contains("top-stories"));
    }

    [Fact]
    public void Migrate_AutoExecuteActionHasNoGate()
    {
        var result = AutomationProfileV2Migrator.Migrate(V1Profile());
        var report = result.Definition.Steps.SelectMany(s => s.Actions).First(a => a.Type == "report.run");
        Assert.Null(report.Confirm);
        Assert.Null(report.When);
        Assert.Equal(5, report.Report);
    }

    [Fact]
    public void Migrate_RoundTripsThroughJson()
    {
        var result = AutomationProfileV2Migrator.Migrate(V1Profile());
        var reparsed = AutomationDefinition.Parse(result.Definition.ToJson());
        Assert.Empty(AutomationDefinitionValidator.Validate(reparsed).Where(e => e.Severity == "error"));
    }
    #endregion
}
