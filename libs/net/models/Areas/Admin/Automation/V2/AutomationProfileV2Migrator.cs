using System.Text.Json;

namespace TNO.API.Areas.Admin.Models.Automation.V2;

/// <summary>
/// V2MigrationResult record, the outcome of migrating a v1 profile: the built definition plus
/// warnings for the constructs that could only be approximated and need review.
/// </summary>
/// <param name="Definition">The built v2 definition.</param>
/// <param name="Warnings">Human-readable review notes.</param>
public record V2MigrationResult(AutomationDefinition Definition, List<string> Warnings);

/// <summary>
/// AutomationProfileV2Migrator class, converts a v1 profile (steps/actions tables) into a v2
/// definition document that issues the same prompts in the same order:
/// - the profile filter becomes an init 'search' into $run.inbox;
/// - step targets map to phases (start→init, content→process, end→complete, none→init);
/// - each v1 action becomes a raw single-response analysis plus a v2 action gated by the same
///   confirmation statement, preserving call count and parsing behaviour;
/// - identical prompt text across steps/actions is extracted into the prompt library.
/// The migration is approximate for extract-data/create-content and prior-action deduplication;
/// each approximation is recorded as a warning.
/// </summary>
public static class AutomationProfileV2Migrator
{
    /// <summary>
    /// The collection the profile filter's results are searched into.
    /// </summary>
    public const string InboxCollection = "$run.inbox";

    /// <summary>
    /// Migrate the specified v1 profile model to a v2 definition.
    /// </summary>
    /// <param name="profile"></param>
    /// <returns></returns>
    public static V2MigrationResult Migrate(AutomationProfileModel profile)
    {
        var definition = new AutomationDefinition();
        var warnings = new List<string>();
        // Prompt text -> library entry name, so byte-identical prompts collapse to one entry.
        var promptIndex = new Dictionary<string, string>(StringComparer.Ordinal);
        // v1 action id -> the collection its results became (fetch-content only).
        var fetchCollections = new Dictionary<int, string>();
        var usedCollectionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { InboxCollection };

        // The profile filter becomes an init search into $run.inbox.
        var init = new V2StepDefinition { Name = "Load content", Phase = V2Phases.Init };
        if (profile.FilterId.HasValue)
            init.Actions.Add(new V2ActionDefinition { Type = "search", Filter = profile.FilterId, Into = InboxCollection });

        var steps = profile.Steps.OrderBy(s => s.Priority).ToArray();

        // First pass: register fetch-content collections so later dedupe actions can reference
        // them regardless of ordering.
        foreach (var step in steps)
            foreach (var action in step.Actions)
                if (action.ActionType == "fetch-content" && action.Id != 0)
                    fetchCollections[action.Id] = UniqueName(usedCollectionNames, $"$run.{Slug(action.Name)}");

        var processSteps = new List<V2StepDefinition>();
        var completeSteps = new List<V2StepDefinition>();

        foreach (var step in steps)
        {
            var phase = step.Target switch
            {
                "start" => V2Phases.Init,
                "content" => V2Phases.Process,
                "end" => V2Phases.Complete,
                _ => V2Phases.Init,
            };
            if (step.Target == "none")
                warnings.Add($"Step '{step.Name}' had target 'none'; migrated as an init step.");
            if (step.IterateStepFilter && step.FilterId.HasValue)
            {
                phase = V2Phases.Process;
            }

            var v2Step = new V2StepDefinition
            {
                Name = step.Name,
                Description = string.IsNullOrWhiteSpace(step.Description) ? null : step.Description,
                Phase = phase,
                IsEnabled = step.IsEnabled,
                LlmId = step.LLMId,
            };

            if (phase == V2Phases.Process)
            {
                v2Step.Source = step.IterateStepFilter && step.FilterId.HasValue
                    ? new V2SourceDefinition { From = "filter", Filter = step.FilterId }
                    : new V2SourceDefinition
                    {
                        From = "collection",
                        Collection = InboxCollection,
                        Include = step.ApplyToAutomationFilter && step.FilterId.HasValue
                            ? new List<int> { step.FilterId.Value }
                            : new List<int>(),
                    };
                if (!step.ApplyToAutomationFilter && step.FilterId.HasValue && !step.IterateStepFilter)
                    warnings.Add($"Step '{step.Name}' used its filter for prompt enrichment ({{results}}); v2 prompts reference collections instead - review its prompt.");
            }

            MigrateActions(step, v2Step, definition, promptIndex, fetchCollections, usedCollectionNames, warnings);

            switch (phase)
            {
                case V2Phases.Init:
                    // Fold start/none steps' actions into the shared init step to keep phase order.
                    foreach (var analysis in v2Step.Analyses) init.Analyses.Add(analysis);
                    foreach (var action in v2Step.Actions) init.Actions.Add(action);
                    if (v2Step.Analyses.Count > 0)
                        warnings.Add($"Step '{step.Name}' ran prompts at start; its analyses were folded into the init step.");
                    break;
                case V2Phases.Process:
                    processSteps.Add(v2Step);
                    break;
                default:
                    completeSteps.Add(v2Step);
                    break;
            }
        }

        definition.Steps.Add(init);
        definition.Steps.AddRange(processSteps);
        definition.Steps.AddRange(completeSteps);
        return new V2MigrationResult(definition, warnings);
    }

    private static void MigrateActions(
        AutomationStepModel step,
        V2StepDefinition v2Step,
        AutomationDefinition definition,
        Dictionary<string, string> promptIndex,
        Dictionary<int, string> fetchCollections,
        HashSet<string> usedCollectionNames,
        List<string> warnings)
    {
        // The v1 step prompt is shared context for every action prompt; it becomes a library
        // entry referenced by each migrated analysis, preserving the composed-prompt behaviour
        // as closely as the one-analysis-per-action model allows.
        var stepPromptRef = string.IsNullOrWhiteSpace(step.Prompt)
            ? null
            : InternPrompt(definition, promptIndex, step.Prompt, $"step-{Slug(step.Name)}");

        var index = 0;
        foreach (var action in step.Actions)
        {
            index++;
            var name = $"{Slug(action.Name)}-{index}";
            switch (action.ActionType)
            {
                case "fetch-content":
                    {
                        var into = action.Id != 0 && fetchCollections.TryGetValue(action.Id, out var known)
                            ? known
                            : UniqueName(usedCollectionNames, $"$run.{Slug(action.Name)}");
                        v2Step.Actions.Add(new V2ActionDefinition { Type = "search", Name = action.Name, IsEnabled = action.IsEnabled, Filter = action.FilterId, Into = into });
                        continue;
                    }
                case "deduplicate":
                    {
                        var against = action.PriorActionId.HasValue && fetchCollections.TryGetValue(action.PriorActionId.Value, out var collection)
                            ? collection
                            : InboxCollection;
                        if (!action.PriorActionId.HasValue || !fetchCollections.ContainsKey(action.PriorActionId.Value))
                            warnings.Add($"Dedupe action '{action.Name}' in step '{step.Name}' compared against a prior action's processed items; v2 compares against a collection - it was pointed at {InboxCollection}, review it.");
                        v2Step.Actions.Add(new V2ActionDefinition
                        {
                            Type = "dedupe",
                            Name = action.Name,
                            IsEnabled = action.IsEnabled,
                            Against = against,
                            Mode = ReadSetting(action.Settings, "deduplicate", "mode") ?? "iterate",
                            BatchSize = ReadSettingInt(action.Settings, "deduplicate", "batchSize"),
                            MaxComparisons = ReadSettingInt(action.Settings, "deduplicate", "maxComparisons"),
                            OnDuplicate = "abort",
                            Prompt = string.IsNullOrWhiteSpace(action.Prompt) ? null : new V2PromptDefinition
                            {
                                Ref = InternPrompt(definition, promptIndex, action.Prompt, Slug(action.Name)),
                            },
                            LlmId = action.LLMId,
                        });
                        continue;
                    }
                case "extract-data":
                case "create-content":
                    warnings.Add($"Action '{action.Name}' ({action.ActionType}) in step '{step.Name}' was not migrated automatically; recreate it with content.create and analysis value sources.");
                    continue;
            }

            // Every other v1 action: a raw analysis carrying the original prompt, plus a v2
            // action gated by the original confirmation statement against that analysis.
            string? analysisName = null;
            if (!action.AutoExecute && !string.IsNullOrWhiteSpace(action.Prompt))
            {
                analysisName = name;
                v2Step.Analyses.Add(new V2AnalysisDefinition
                {
                    Name = name,
                    Raw = true,
                    Prompt = new V2PromptDefinition
                    {
                        Ref = stepPromptRef ?? InternPrompt(definition, promptIndex, action.Prompt, Slug(action.Name)),
                        Override = stepPromptRef != null ? action.Prompt : null,
                    },
                    LlmId = action.LLMId,
                });
            }

            var v2Action = new V2ActionDefinition
            {
                Name = action.Name,
                IsEnabled = action.IsEnabled,
                Analysis = analysisName,
                Confirm = action.AutoExecute || string.IsNullOrWhiteSpace(action.ConfirmationStatement) ? null : action.ConfirmationStatement,
            };
            var captured = new V2ValueSource { From = analysisName != null ? $"{analysisName}.value" : null };

            switch (action.ActionType)
            {
                case "update-content-field":
                    v2Action.Type = "content.update";
                    v2Action.Field = action.ContentField;
                    v2Action.Value = captured;
                    break;
                case "add-tags":
                    v2Action.Type = "content.tags";
                    v2Action.Value = captured;
                    break;
                case "add-sentiment":
                    v2Action.Type = "content.sentiment";
                    v2Action.Value = captured;
                    break;
                case "select-columnist":
                    v2Action.Type = "content.contributor";
                    v2Action.Value = captured;
                    break;
                case "add-action":
                    v2Action.Type = "content.action";
                    v2Action.ContentAction = action.ContentActionId;
                    break;
                case "publish-content":
                    v2Action.Type = "content.publish";
                    break;
                case "unpublish-content":
                    v2Action.Type = "content.unpublish";
                    break;
                case "abort-step":
                    v2Action.Type = "abort";
                    break;
                case "score-content":
                    v2Action.Type = "score";
                    v2Action.Objective = action.Objective;
                    v2Action.Value = captured;
                    break;
                case "select-top":
                    v2Action.Type = "select-top";
                    v2Action.Objective = action.Objective;
                    v2Action.Take = action.MaxCalls ?? 10;
                    v2Action.ContentAction = action.ContentActionId;
                    v2Action.Into = UniqueName(usedCollectionNames, $"$run.{Slug(action.Objective ?? action.Name)}");
                    v2Action.Confirm = null;
                    v2Action.Analysis = null;
                    warnings.Add($"Select-top action '{action.Name}' in step '{step.Name}' was migrated as deterministic top-{action.MaxCalls ?? 10} by score; the v1 LLM selection prompt was dropped.");
                    break;
                case "run-report":
                    v2Action.Type = "report.run";
                    v2Action.Report = action.ReportId;
                    v2Action.Confirm = null;
                    v2Action.Analysis = null;
                    break;
                case "run-notification":
                    v2Action.Type = "notification.run";
                    v2Action.Notification = action.NotificationId;
                    v2Action.Confirm = null;
                    v2Action.Analysis = null;
                    break;
                default:
                    warnings.Add($"Action '{action.Name}' ({action.ActionType}) in step '{step.Name}' has no v2 equivalent and was skipped.");
                    continue;
            }
            v2Step.Actions.Add(v2Action);
        }
    }

    private static string InternPrompt(AutomationDefinition definition, Dictionary<string, string> index, string text, string baseName)
    {
        if (index.TryGetValue(text, out var existing)) return existing;
        var name = baseName;
        var suffix = 1;
        while (definition.Prompts.ContainsKey(name)) name = $"{baseName}-{++suffix}";
        definition.Prompts[name] = text;
        index[text] = name;
        return name;
    }

    private static string UniqueName(HashSet<string> used, string baseName)
    {
        var name = baseName;
        var suffix = 1;
        while (!used.Add(name)) name = $"{baseName}-{++suffix}";
        return name;
    }

    private static string Slug(string value)
    {
        var chars = value.Trim().ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        var slug = new string(chars);
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        return slug.Trim('-') is { Length: > 0 } trimmed ? trimmed : "item";
    }

    private static string? ReadSetting(JsonDocument settings, string group, string key)
    {
        if (settings.RootElement.ValueKind != JsonValueKind.Object) return null;
        if (!settings.RootElement.TryGetProperty(group, out var groupElement) || groupElement.ValueKind != JsonValueKind.Object) return null;
        if (!groupElement.TryGetProperty(key, out var value)) return null;
        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText();
    }

    private static int? ReadSettingInt(JsonDocument settings, string group, string key)
    {
        var text = ReadSetting(settings, group, key);
        return int.TryParse(text, out var number) ? number : null;
    }
}
