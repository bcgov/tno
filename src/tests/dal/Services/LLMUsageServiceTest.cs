using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TNO.Core.Exceptions;
using TNO.DAL;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Test.Core;

namespace TNO.Test.DAL.Services;

/// <summary>
/// Everything that uses an LLM connection must be found, and move to another one together.
/// </summary>
public class LLMUsageServiceTest : IDisposable
{
    const int Source = 1;
    const int Target = 2;
    const int Other = 3;
    readonly TestHelper helper = new();

    public LLMUsageServiceTest()
    {
        helper.Build((services) =>
        {
            services.AddTNOContext($"llm-usage-{Guid.NewGuid()}");
            services.AddPrincipalForRole("administrator");
            services.AddOptions();
            services.AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(Microsoft.Extensions.Logging.Abstractions.NullLogger<>));
            services.AddSingleton<ILLMService, LLMService>();
        });
    }

    private TNOContext Context => helper.Provider.GetRequiredService<TNOContext>();
    private ILLMService Service => helper.Provider.GetRequiredService<ILLMService>();

    private static LLM Connection(int id, string name, Action<LLM>? configure = null)
    {
        var llm = new LLM(name, "deployment")
        {
            Id = id,
            IsEnabled = true,
            IsPublic = true,
            ApiKey = "key",
            ProjectEndpoint = new Uri("http://localhost"),
            ContextWindow = 1000,
            MaxOutputTokens = 100,
        };
        configure?.Invoke(llm);
        return llm;
    }

    private void Seed(Action<LLM>? configureTarget = null, string ownerRoles = "[administrator]", string sectionTemperature = "0.5")
    {
        var context = Context;
        context.LLM.AddRange(Connection(Source, "source"), Connection(Target, "target", configureTarget), Connection(Other, "other"));
        context.Users.Add(new User("owner", "owner@test.com") { Id = 7, Roles = ownerRoles });
        context.Reports.Add(new Report(11, "Report", 1, 7) { IsEnabled = true });
        context.ReportSections.AddRange(
            new ReportSection(21, "guid-a", ReportSectionType.AI, 11)
            {
                IsEnabled = true,
                Settings = JsonDocument.Parse($$$"""{"llmId":{{{Source}}},"label":"Summary","userPrompt":"keep me","systemPrompt":"keep me too","temperature":{{{sectionTemperature}}},"custom":{"a":1}}"""),
            },
            new ReportSection(22, "guid-b", ReportSectionType.AI, 11) { Settings = JsonDocument.Parse($$"""{"llmId":{{Other}},"temperature":0.5}""") },
            new ReportSection(23, "guid-c", ReportSectionType.Content, 11) { Settings = JsonDocument.Parse($$"""{"llmId":{{Source}}}""") });
        context.AutomationProfiles.AddRange(
            new AutomationProfile(31, "Default") { LLMId = Source },
            new AutomationProfile(32, "Overrides")
            {
                LLMId = Other,
                Definition = JsonDocument.Parse($$"""
                    {"unknown":{"llmId":{{Source}}},"steps":[{"name":"one","llmId":{{Source}},"analyses":[{"name":"a1","llmId":{{Source}}},{"name":"a2","llmId":{{Other}}}],"actions":[{"type":"dedupe","llmId":{{Source}}}]}]}
                    """),
            },
            new AutomationProfile(33, "Unrelated") { LLMId = Other, Definition = JsonDocument.Parse("""{"steps":[{"name":"one"}]}""") });
        context.Settings.AddRange(
            new Setting(41, AdminConfigurableSettingNames.ContentAnalysisLLMId.ToString(), Source.ToString()),
            new Setting(42, ILLMService.FilterAISettingName, Source.ToString()));
        context.ContentAnalyses.Add(new ContentAnalysis(100, "hash") { LLMId = Source });
        context.SaveChanges();
        context.ChangeTracker.Clear();
    }

    [Fact]
    public void FindUsage_FindsEverythingThatUsesTheConnection()
    {
        Seed();

        var usage = Service.FindUsage(Source);
        var items = usage.Items.ToArray();

        Assert.Equal(new[] { "ReportSection", "AutomationProfile", "AutomationProfile", "ContentAnalysis", "Search" }, items.Select(i => i.Type));
        Assert.Equal(1, usage.ContentAnalysisResultCount);

        var section = items[0];
        Assert.Equal((11, 21, "Report", "Summary", "owner"), (section.Id, section.SectionId, section.Name, section.Detail, section.Owner));

        Assert.Equal("Profile default", items.Single(i => i.Id == 31).Detail);
        Assert.Equal("Step 'one', Analysis 'a1' in step 'one', Action 'dedupe' in step 'one'", items.Single(i => i.Id == 32).Detail);
    }

    [Fact]
    public void Migrate_MovesEverythingAndLeavesTheRestUnchanged()
    {
        Seed();

        var result = Service.Migrate(Source, Target, false, false);
        Context.ChangeTracker.Clear();

        Assert.Equal(5, result.Migrated);
        Assert.Empty(result.Usage.Items);

        var section = Context.ReportSections.Single(s => s.Id == 21).Settings.RootElement;
        Assert.Equal(Target, section.GetProperty("llmId").GetInt32());
        Assert.Equal("keep me", section.GetProperty("userPrompt").GetString());
        Assert.Equal("keep me too", section.GetProperty("systemPrompt").GetString());
        Assert.Equal(0.5f, section.GetProperty("temperature").GetSingle());
        Assert.Equal(1, section.GetProperty("custom").GetProperty("a").GetInt32());

        Assert.Equal(Other, Context.ReportSections.Single(s => s.Id == 22).Settings.RootElement.GetProperty("llmId").GetInt32());
        Assert.Equal(Source, Context.ReportSections.Single(s => s.Id == 23).Settings.RootElement.GetProperty("llmId").GetInt32());

        Assert.Equal(Target, Context.AutomationProfiles.Single(p => p.Id == 31).LLMId);
        var overrides = Context.AutomationProfiles.Single(p => p.Id == 32);
        Assert.Equal(Other, overrides.LLMId);
        var root = overrides.Definition!.RootElement;
        var step = root.GetProperty("steps")[0];
        Assert.Equal(Target, step.GetProperty("llmId").GetInt32());
        Assert.Equal(Target, step.GetProperty("analyses")[0].GetProperty("llmId").GetInt32());
        Assert.Equal(Other, step.GetProperty("analyses")[1].GetProperty("llmId").GetInt32());
        Assert.Equal(Target, step.GetProperty("actions")[0].GetProperty("llmId").GetInt32());
        Assert.Equal(Source, root.GetProperty("unknown").GetProperty("llmId").GetInt32());

        Assert.All(Context.Settings.Where(s => s.Id == 41 || s.Id == 42).ToArray(), s => Assert.Equal(Target.ToString(), s.Value));
        Assert.Equal(Source, Context.ContentAnalyses.Single().LLMId);
    }

    [Theory]
    [InlineData("0.1", 0.3f)]
    [InlineData("0.9", 0.8f)]
    public void Migrate_MovesATemperatureOutsideTheRangeToItsNearestLimit(string temperature, float expected)
    {
        Seed(llm => { llm.MinTemperature = 0.3f; llm.MaxTemperature = 0.8f; }, sectionTemperature: temperature);

        Assert.Contains(Service.CheckMigration(Source, Target, false, false).Warnings, w => w.Contains("temperature"));
        var result = Service.Migrate(Source, Target, false, false);
        Context.ChangeTracker.Clear();

        Assert.Equal(1, result.TemperaturesChanged);
        Assert.Equal(expected, Context.ReportSections.Single(s => s.Id == 21).Settings.RootElement.GetProperty("temperature").GetSingle());
    }

    public static TheoryData<string, Action<LLM>> InvalidTargets => new()
    {
        { "disabled", llm => llm.IsEnabled = false },
        { "is an agent", llm => llm.AgentName = "agent" },
        { "project endpoint", llm => llm.ProjectEndpoint = null },
        { "no API key", llm => llm.ApiKey = null },
        { "no deployment name", llm => llm.DeploymentName = "" },
        { "context window", llm => llm.MaxOutputTokens = 1000 },
    };

    [Theory]
    [MemberData(nameof(InvalidTargets))]
    public void Migrate_RefusesAnInvalidTargetAndChangesNothing(string error, Action<LLM> configure)
    {
        Seed(configure);

        Assert.Contains(Service.CheckMigration(Source, Target, true, true).Errors, e => e.Contains(error));
        Assert.Throws<BadRequestException>(() => Service.Migrate(Source, Target, true, true));
        Context.ChangeTracker.Clear();

        Assert.Equal(5, Service.FindUsage(Source).Items.Count());
    }

    [Fact]
    public void CheckMigration_ReportsAndSearchCanUseConfiguredDefaults()
    {
        Seed(llm => llm.ApiKey = null);
        Context.AutomationProfiles.RemoveRange(Context.AutomationProfiles);
        Context.Settings.RemoveRange(Context.Settings.Where(s => s.Id == 41));
        Context.SaveChanges();
        Context.ChangeTracker.Clear();

        Assert.Empty(Service.CheckMigration(Source, Target, true, true).Errors);
        Assert.Contains(Service.CheckMigration(Source, Target, false, true).Errors, e => e.Contains("Report AI and Search"));
    }

    [Theory]
    [InlineData(false, "[editor]", true)]
    [InlineData(true, "[editor]", false)]
    [InlineData(false, "[editor],[administrator]", false)]
    public void CheckMigration_WarnsWhenANonAdminReportMovesToANonPublicTarget(bool isPublic, string roles, bool warns)
    {
        Seed(llm => llm.IsPublic = isPublic, roles);

        var check = Service.CheckMigration(Source, Target, false, false);

        Assert.Empty(check.Errors);
        Assert.Equal(warns, check.Warnings.Any(w => w.Contains("not public")));
    }

    [Fact]
    public void CheckMigration_RefusesTheSameConnectionAndUnusedConnections()
    {
        Seed();

        Assert.Contains(Service.CheckMigration(Source, Source, false, false).Errors, e => e.Contains("different"));
        Assert.Contains(Service.CheckMigration(Target, Source, false, false).Errors, e => e.Contains("Nothing"));
        Assert.Contains(Service.CheckMigration(Source, 999, false, false).Errors, e => e.Contains("does not exist"));
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
