namespace TNO.API.Areas.Admin.Models.Automation;

/// <summary>
/// ValidationError record, one validation finding with the definition path it anchors to,
/// so the editor can highlight it in place.
/// </summary>
/// <param name="Path">Definition path (e.g. 'steps[2].actions[4].against').</param>
/// <param name="Message">What is wrong.</param>
/// <param name="Severity">'error' blocks save; 'warning' does not.</param>
public record ValidationError(string Path, string Message, string Severity = "error");

/// <summary>
/// ContentActionSpec record, the part of a content action a definition can be validated against.
/// The catalog knows a field holds a content action id; only the database knows what that action
/// stores, so the caller supplies these when it can reach the lookups.
/// </summary>
/// <param name="Id">The action id a definition references.</param>
/// <param name="Name">Display name, for the finding's message.</param>
/// <param name="ValueType">What the action stores.</param>
public record ContentActionSpec(int Id, string Name, TNO.Entities.ValueType ValueType);

/// <summary>
/// AutomationDefinitionValidator class, validates a definition document against the action
/// catalog and its own internal references, so configuration errors surface at save rather than
/// in a run.
/// </summary>
public static class AutomationDefinitionValidator
{
    /// <summary>
    /// Validate the specified definition. Returns every finding; the caller decides whether
    /// warnings block.
    /// </summary>
    /// <param name="definition"></param>
    /// <param name="contentActions">The content actions the definition may reference; when supplied,
    /// an action that stores a value is checked for one. Omitted, those checks are skipped.</param>
    /// <returns></returns>
    public static List<ValidationError> Validate(AutomationDefinition definition, IEnumerable<ContentActionSpec>? contentActions = null)
    {
        var errors = new List<ValidationError>();
        var contentActionsById = contentActions?.GroupBy(a => a.Id).ToDictionary(g => g.Key, g => g.First());

        if (definition.Steps.Count == 0)
            errors.Add(new("steps", "The definition has no steps."));

        // Track named collections as they are created, in step order, so references to unknown
        // names are caught. Collections created by add/move into a new name are registered too.
        var collections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // Collections that receive drafts (items created during the run without a database id).
        var draftCollections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var usedPrompts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var objectives = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var stepNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var lastPhaseRank = 0;

        for (var s = 0; s < definition.Steps.Count; s++)
        {
            var step = definition.Steps[s];
            var stepPath = $"steps[{s}]";

            if (string.IsNullOrWhiteSpace(step.Name))
                errors.Add(new($"{stepPath}.name", "Step name is required."));
            else if (!stepNames.Add(step.Name))
                errors.Add(new($"{stepPath}.name", $"Step name '{step.Name}' is not unique."));

            if (!AutomationPhases.All.Contains(step.Phase))
            {
                errors.Add(new($"{stepPath}.phase", $"Phase '{step.Phase}' is not one of: {string.Join(", ", AutomationPhases.All)}."));
                continue;
            }

            // Phase order: init steps before process steps before complete steps.
            var phaseRank = Array.IndexOf(AutomationPhases.All, step.Phase);
            if (phaseRank < lastPhaseRank)
                errors.Add(new($"{stepPath}.phase", $"A '{step.Phase}' step cannot follow a '{AutomationPhases.All[lastPhaseRank]}' step; order steps init → process → complete."));
            lastPhaseRank = Math.Max(lastPhaseRank, phaseRank);


            // Source rules per phase: process requires a source, complete may declare one (and
            // then iterates it like a process step), init never has one.
            var sourceIsDraftCollection = false;
            var iterates = step.Phase != AutomationPhases.Init && step.Source != null;
            if (step.Phase != AutomationPhases.Init)
            {
                // Process steps are per-item by definition; a complete step only needs a source
                // when it contains per-item actions (the action rule below enforces that).
                if (step.Source == null)
                {
                    if (step.Phase == AutomationPhases.Process)
                        errors.Add(new($"{stepPath}.source", "A process step requires a source."));
                }
                else
                {
                    var source = step.Source;
                    var sourcePath = $"{stepPath}.source";
                    switch (source.From)
                    {
                        case "filter":
                            if (!source.Filter.HasValue)
                                errors.Add(new($"{sourcePath}.filter", "A filter source requires a filter id."));
                            break;
                        case "collection":
                            if (string.IsNullOrWhiteSpace(source.Collection))
                                errors.Add(new($"{sourcePath}.collection", "A collection source requires a collection name."));
                            else
                            {
                                ValidateCollectionName(source.Collection, $"{sourcePath}.collection", errors);
                                if (!collections.Contains(source.Collection))
                                    errors.Add(new($"{sourcePath}.collection", $"Collection '{source.Collection}' is not created by any earlier step."));
                                sourceIsDraftCollection = draftCollections.Contains(source.Collection);
                            }
                            break;
                        default:
                            errors.Add(new($"{sourcePath}.from", $"Source '{source.From}' is not one of: filter, collection (content enters a run through 'search' actions)."));
                            break;
                    }
                }
            }
            else if (step.Source != null)
                errors.Add(new($"{stepPath}.source", "An init step runs once and cannot declare a source."));

            // Analyses.
            // Every draft the step creates, wherever it is created: an analysis runs at the
            // position of the action that consumes it, not at a position of its own, so a target
            // is judged against the step as a whole rather than against what precedes it.
            var stepDrafts = step.Actions
                .Where(a => a.Type == "content.create" && !string.IsNullOrWhiteSpace(a.As))
                .Select(a => a.As!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var analyses = new Dictionary<string, AnalysisDefinition>(StringComparer.OrdinalIgnoreCase);
            for (var a = 0; a < step.Analyses.Count; a++)
            {
                var analysis = step.Analyses[a];
                var path = $"{stepPath}.analyses[{a}]";
                if (string.IsNullOrWhiteSpace(analysis.Name))
                    errors.Add(new($"{path}.name", "Analysis name is required."));
                else if (!analyses.TryAdd(analysis.Name, analysis))
                    errors.Add(new($"{path}.name", $"Analysis name '{analysis.Name}' is not unique within the step."));

                ValidatePrompt(analysis.Prompt, definition, usedPrompts, $"{path}.prompt", errors);
                var analysisPromptText = $"{analysis.Prompt.Text} {analysis.Prompt.Override} " +
                    (analysis.Prompt.Ref != null && definition.Prompts.TryGetValue(analysis.Prompt.Ref, out var libraryEntry) ? libraryEntry.Text : "");
                ValidatePromptTokens(analysisPromptText, $"{path}.prompt", allowCandidates: false, errors);

                if (!string.IsNullOrWhiteSpace(analysis.Target) && !stepDrafts.Contains(analysis.Target!))
                    errors.Add(new($"{path}.target", $"Draft '{analysis.Target}' is not created by a content.create action in this step."));
                // A '{target...}' token with nothing to read renders as nothing, which looks like
                // the model ignored the instruction rather than like a missing setting.
                else if (string.IsNullOrWhiteSpace(analysis.Target) && _targetToken.IsMatch(analysisPromptText))
                    errors.Add(new($"{path}.target", "The prompt uses a '{target}' token but the analysis has no target, so the token renders as nothing. Set the target to the draft it should read.", "warning"));

                if (!analysis.Raw && analysis.Returns.Count == 0)
                    errors.Add(new($"{path}.returns", "A structured analysis must declare at least one return key (or set raw)."));
                if (!string.IsNullOrWhiteSpace(analysis.Chain) && !analyses.ContainsKey(analysis.Chain!))
                    errors.Add(new($"{path}.chain", $"Chained analysis '{analysis.Chain}' is not declared earlier in this step."));
            }

            // Actions.
            var consumedAnalyses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var drafts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            // Analysis + confirmation-statement pairs already used, to flag accidental copies:
            // two actions sharing a marker against the same response both fire on it.
            var confirmations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            // Dedupe actions publish '<name>.isDuplicate' results later actions may reference.
            var dedupeResults = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < step.Actions.Count; i++)
            {
                var action = step.Actions[i];
                var path = $"{stepPath}.actions[{i}]";

                if (!ActionCatalog.Types.TryGetValue(action.Type, out var descriptor))
                {
                    errors.Add(new($"{path}.type", $"Action type '{action.Type}' is not registered."));
                    continue;
                }

                if (!descriptor.Phases.Contains(step.Phase))
                    errors.Add(new($"{path}.type", $"Action '{action.Type}' is not valid in a '{step.Phase}' step (allowed: {string.Join(", ", descriptor.Phases)})."));
                if (descriptor.RequiresSubject && !iterates)
                    errors.Add(new($"{path}.type", $"Action '{action.Type}' requires an iterated item and can only appear in a process step, or a complete step that declares a source."));
                if (descriptor.RequiresPersistedId && sourceIsDraftCollection)
                    errors.Add(new($"{path}.type", $"Action '{action.Type}' requires persisted ids; make sure the drafts are saved (Save Content Now or a Save Collection action) before this step runs.", "warning"));

                // Required fields per the descriptor.
                foreach (var field in descriptor.Fields.Where(f => f.Required))
                {
                    if (!HasField(action, field.Name))
                        errors.Add(new($"{path}.{field.Name}", $"Action '{action.Type}' requires '{field.Name}'."));
                }

                // A content action that stores a value needs one; without it the stamp is
                // meaningless (a Commentary timeout of 'true' is not a number of days).
                if (contentActionsById != null
                    && descriptor.Fields.Any(f => f.Kind == "contentActionValue")
                    && action.ContentAction.HasValue
                    && contentActionsById.TryGetValue(action.ContentAction.Value, out var contentAction)
                    && contentAction.ValueType != TNO.Entities.ValueType.Boolean
                    && !HasValueSource(action.Value))
                    errors.Add(new($"{path}.value", $"Content action '{contentAction.Name}' stores a {contentAction.ValueType.ToString().ToLower()} value; give the action a value to stamp."));

                // Collection references.
                foreach (var (name, value) in CollectionRefs(action))
                {
                    if (string.IsNullOrWhiteSpace(value)) continue;
                    ValidateCollectionName(value!, $"{path}.{name}", errors);
                    var creates = (action.Type == "search" && name == "into")
                        || (action.Type == "collection.create" && name == "into")
                        || (name == "into" && (action.Type.StartsWith("collection.") || action.Type == "select-top"));
                    if (creates) collections.Add(value!);
                    else if (!collections.Contains(value!))
                        errors.Add(new($"{path}.{name}", $"Collection '{value}' is not created by any earlier action.", "warning"));
                }
                // Draft items entering a collection make it a draft collection.
                if (action.Type is "collection.add" or "collection.move" && action.Item != null
                    && action.Item.StartsWith("$item.", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(action.Into))
                    draftCollections.Add(action.Into!);

                // Analysis references from gates and value sources.
                foreach (var (refPath, reference) in AnalysisRefs(action))
                {
                    var name = reference.Split('.', 2)[0];
                    if (name.Equals("content", StringComparison.OrdinalIgnoreCase)) continue;
                    if (analyses.ContainsKey(name)) consumedAnalyses.Add(name);
                    else if (!dedupeResults.Contains(name))
                        errors.Add(new($"{path}.{refPath}", $"Analysis '{name}' is not declared on this step (and no earlier Detect Duplicate action publishes it)."));
                }
                // Objectives are recorded by score actions and consumed by select-top.
                if (action.Type == "score" && !string.IsNullOrWhiteSpace(action.Objective))
                    objectives.Add(action.Objective!);
                if (action.Type == "select-top" && !string.IsNullOrWhiteSpace(action.Objective) && !objectives.Contains(action.Objective!))
                    errors.Add(new($"{path}.objective", $"No earlier score action records objective '{action.Objective}'; select-top will find nothing to rank.", "warning"));
                // How many to keep: a fixed count, a score threshold, or a threshold with a cap.
                // Neither means the action has no rule at all, so it would silently select nothing.
                if (action.Type == "select-top" && !action.Take.HasValue && !action.MinScore.HasValue)
                    errors.Add(new($"{path}.take", "Select Top Scored needs 'take' (keep a fixed count), 'minScore' (keep everything at or above a score), or both."));

                if (action.Type == "dedupe")
                {
                    dedupeResults.Add(action.Name ?? "dedupe");
                    // The prompt is exactly what is sent, so it must place the data itself.
                    var effective = $"{action.Prompt?.Text} {action.Prompt?.Override} " +
                        (action.Prompt?.Ref != null && definition.Prompts.TryGetValue(action.Prompt.Ref, out var entry) ? entry.Text : "");
                    if (action.Prompt == null && definition.Prompts.TryGetValue("default-dedupe", out var customized))
                        effective = customized.Text;
                    var hasCustomText = !string.IsNullOrWhiteSpace(effective.Trim());
                    ValidatePromptTokens(effective, $"{path}.prompt", allowCandidates: true, errors);
                    if (hasCustomText && !effective.Contains("{content"))
                        errors.Add(new($"{path}.prompt", "The comparison prompt never includes the current story - add {content} (or {content.*} fields) where it belongs.", "warning"));
                    if (hasCustomText && !effective.Contains("{candidate"))
                        errors.Add(new($"{path}.prompt", "The comparison prompt never includes the candidate stories - add {candidates} (or {candidate.*} fields in iterate mode) where they belong.", "warning"));
                    if (string.Equals(action.Mode, "batch", StringComparison.OrdinalIgnoreCase) && effective.Contains("{candidate."))
                        errors.Add(new($"{path}.prompt", "Field-level {candidate.*} tokens only resolve in iterate mode; batch prompts should use {candidates} (the full list).", "warning"));
                }
                if (!string.IsNullOrWhiteSpace(action.Analysis))
                {
                    if (!analyses.ContainsKey(action.Analysis!))
                        errors.Add(new($"{path}.analysis", $"Analysis '{action.Analysis}' is not declared on this step."));
                    else consumedAnalyses.Add(action.Analysis!);
                }
                else if (!string.IsNullOrWhiteSpace(action.Confirm))
                {
                    // Confirm without a named analysis requires exactly one to be unambiguous.
                    if (analyses.Count == 1) consumedAnalyses.Add(analyses.Keys.First());
                    else errors.Add(new($"{path}.confirm", "Confirm requires 'analysis' to name which analysis response to match (the step has more than one)."));
                }
                if (!string.IsNullOrWhiteSpace(action.Confirm))
                {
                    var confirmAnalysis = action.Analysis ?? (analyses.Count == 1 ? analyses.Keys.First() : "");
                    if (!confirmations.Add($"{confirmAnalysis}|{action.Confirm!.Trim()}"))
                        errors.Add(new($"{path}.confirm", $"Confirmation '{action.Confirm}' is already used by an earlier action against analysis '{confirmAnalysis}'; both actions will fire on the same response marker (fine if the fan-out is deliberate).", "warning"));
                }

                if (action.When != null)
                    ValidateCondition(action.When, $"{path}.when", errors);
                if (action.Where != null)
                    ValidateCondition(action.Where, $"{path}.where", errors);
                if (action.Prompt != null)
                    ValidatePrompt(action.Prompt, definition, usedPrompts, $"{path}.prompt", errors);

                // Copy fields only apply when copyFrom names a source; unset means 'start blank'.
                if (action.Type == "content.create" && action.CopyFields is { Count: > 0 } && string.IsNullOrWhiteSpace(action.CopyFrom))
                    errors.Add(new($"{path}.copyFields", "'Copy fields' has no effect because 'copyFrom' is not set - the draft starts blank. Set copyFrom to the original item to copy from it.", "warning"));

                // Draft registry: created by content.create, referenced by target/item.
                if (action.Type == "content.create" && !string.IsNullOrWhiteSpace(action.As))
                {
                    if (!action.As!.StartsWith("$item.", StringComparison.OrdinalIgnoreCase))
                        errors.Add(new($"{path}.as", "A draft name must start with '$item.' (drafts are scoped to the iteration)."));
                    else if (!drafts.Add(action.As!))
                        errors.Add(new($"{path}.as", $"Draft name '{action.As}' is already created by an earlier action in this step; later references would only see the last one."));
                }
                if (!string.IsNullOrWhiteSpace(action.Target) && !drafts.Contains(action.Target!))
                    errors.Add(new($"{path}.target", $"Draft '{action.Target}' is not created by an earlier content.create in this step."));
                if (!string.IsNullOrWhiteSpace(action.Item)
                    && !action.Item!.Equals("$item", StringComparison.OrdinalIgnoreCase)
                    && !drafts.Contains(action.Item!))
                    errors.Add(new($"{path}.item", $"Item '{action.Item}' is neither '$item' nor a draft created earlier in this step."));
            }

            // Unconsumed analyses never run (they are lazy); surface as warnings.
            foreach (var name in analyses.Keys.Where(n => !consumedAnalyses.Contains(n)))
                errors.Add(new($"{stepPath}.analyses", $"Analysis '{name}' is not consumed by any action and will never run.", "warning"));
        }

        // Unused prompt library entries.
        foreach (var name in definition.Prompts.Keys.Where(k => !usedPrompts.Contains(k)
            && !k.StartsWith("default-", StringComparison.OrdinalIgnoreCase)))
            errors.Add(new($"prompts.{name}", $"Prompt '{name}' is not referenced.", "warning"));

        return errors;
    }


    private static void ValidateCollectionName(string name, string path, List<ValidationError> errors)
    {
        if (!name.StartsWith("$run.", StringComparison.OrdinalIgnoreCase))
            errors.Add(new(path, $"Collection name '{name}' must start with '$run.' (collections are run-scoped)."));
    }

    private static void ValidatePrompt(PromptDefinition prompt, AutomationDefinition definition, HashSet<string> used, string path, List<ValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(prompt.Ref) && string.IsNullOrWhiteSpace(prompt.Text))
            errors.Add(new(path, "A prompt requires a library reference or inline text."));
        if (!string.IsNullOrWhiteSpace(prompt.Ref))
        {
            if (!definition.Prompts.ContainsKey(prompt.Ref!))
                errors.Add(new($"{path}.ref", $"Prompt '{prompt.Ref}' is not in the prompt library."));
            else used.Add(prompt.Ref!);
        }
    }

    private static readonly System.Text.RegularExpressions.Regex _targetToken =
        new(@"\{target(\.[^}]*)?\}", System.Text.RegularExpressions.RegexOptions.Compiled);

    private static readonly System.Text.RegularExpressions.Regex _promptToken =
        new(@"\{(?<name>[a-zA-Z][a-zA-Z0-9_-]*)(?<rest>[.:][^}]*)?\}", System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>
    /// Warn about tokens the engine does not recognize: they are sent to the LLM as literal
    /// text, which usually means a typo (e.g. '{duplicates}' instead of '{candidates}').
    /// </summary>
    private static void ValidatePromptTokens(string? text, string path, bool allowCandidates, List<ValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        var known = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "content", "target", "lookup", "collection", "value" };
        if (allowCandidates) { known.Add("candidate"); known.Add("candidates"); }
        var unknown = _promptToken.Matches(text!)
            .Select(m => m.Groups["name"].Value)
            .Where(name => !known.Contains(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        foreach (var name in unknown)
            errors.Add(new(path, $"Token '{{{name}}}' is not recognized and will be sent to the LLM as literal text{(allowCandidates ? " (did you mean {candidates} or {candidate.*}?)" : "")}.", "warning"));
    }

    private static void ValidateCondition(ConditionDefinition condition, string path, List<ValidationError> errors)
    {
        var shapes = 0;
        if (condition.All is { Count: > 0 }) { shapes++; for (var i = 0; i < condition.All.Count; i++) ValidateCondition(condition.All[i], $"{path}.all[{i}]", errors); }
        if (condition.Any is { Count: > 0 }) { shapes++; for (var i = 0; i < condition.Any.Count; i++) ValidateCondition(condition.Any[i], $"{path}.any[{i}]", errors); }
        if (condition.Not != null) { shapes++; ValidateCondition(condition.Not, $"{path}.not", errors); }
        if (!string.IsNullOrWhiteSpace(condition.From)) shapes++;
        if (!string.IsNullOrWhiteSpace(condition.Field) || !string.IsNullOrWhiteSpace(condition.Op))
        {
            shapes++;
            if (string.IsNullOrWhiteSpace(condition.Field))
                errors.Add(new($"{path}.field", "A leaf condition requires a field."));
            if (string.IsNullOrWhiteSpace(condition.Op))
                errors.Add(new($"{path}.op", "A leaf condition requires an operator."));
            else if (!ConditionOps.All.Contains(condition.Op))
                errors.Add(new($"{path}.op", $"Operator '{condition.Op}' is not one of: {string.Join(", ", ConditionOps.All)}."));
        }
        if (shapes == 0)
            errors.Add(new(path, "A condition requires a leaf (field/op), a combinator (all/any/not), or an analysis gate (from)."));
        else if (shapes > 1)
            errors.Add(new(path, "A condition must be exactly one shape: a leaf, a combinator, or an analysis gate."));
    }

    /// <summary>
    /// Whether a value source actually supplies something. An empty literal, a blank reference,
    /// or a blank template is the editor's 'not filled in yet' shape, not a value.
    /// </summary>
    private static bool HasValueSource(ValueSource? value)
    {
        if (value == null) return false;
        if (!string.IsNullOrWhiteSpace(value.From)) return true;
        if (!string.IsNullOrWhiteSpace(value.Template)) return true;
        if (value.Literal.HasValue)
        {
            var literal = value.Literal.Value;
            return literal.ValueKind switch
            {
                System.Text.Json.JsonValueKind.String => !string.IsNullOrWhiteSpace(literal.GetString()),
                System.Text.Json.JsonValueKind.Null or System.Text.Json.JsonValueKind.Undefined => false,
                _ => true,
            };
        }
        return false;
    }

    private static bool HasField(ActionDefinition action, string name) => name switch
    {
        "filter" => action.Filter.HasValue,
        "into" => !string.IsNullOrWhiteSpace(action.Into),
        "from" => !string.IsNullOrWhiteSpace(action.FromCollection),
        "with" => !string.IsNullOrWhiteSpace(action.With),
        "item" => !string.IsNullOrWhiteSpace(action.Item),
        "by" => !string.IsNullOrWhiteSpace(action.By),
        "where" => action.Where != null,
        "count" => action.Count.HasValue,
        "field" => !string.IsNullOrWhiteSpace(action.Field),
        "value" => action.Value != null,
        "against" => !string.IsNullOrWhiteSpace(action.Against),
        "objective" => !string.IsNullOrWhiteSpace(action.Objective),
        "take" => action.Take.HasValue,
        "minScore" => action.MinScore.HasValue,
        "contentAction" => action.ContentAction.HasValue,
        "report" => action.Report.HasValue,
        "notification" => action.Notification.HasValue,
        "as" => !string.IsNullOrWhiteSpace(action.As),
        _ => true,
    };

    private static IEnumerable<(string Name, string? Value)> CollectionRefs(ActionDefinition action)
    {
        yield return ("into", action.Into);
        yield return ("from", action.FromCollection);
        yield return ("with", action.With);
        yield return ("against", action.Against);
        yield return ("using", action.Using);
    }

    private static IEnumerable<(string Path, string Reference)> AnalysisRefs(ActionDefinition action)
    {
        if (!string.IsNullOrWhiteSpace(action.Value?.From)) yield return ("value.from", action.Value!.From!);
        foreach (var reference in ConditionRefs(action.When, "when")) yield return reference;
        if (action.Set != null)
            foreach (var (key, source) in action.Set.Where(kv => !string.IsNullOrWhiteSpace(kv.Value.From)))
                yield return ($"set.{key}.from", source.From!);
    }

    private static IEnumerable<(string Path, string Reference)> ConditionRefs(ConditionDefinition? condition, string path)
    {
        if (condition == null) yield break;
        if (!string.IsNullOrWhiteSpace(condition.From)) yield return ($"{path}.from", condition.From!);
        if (condition.All != null)
            for (var i = 0; i < condition.All.Count; i++)
                foreach (var reference in ConditionRefs(condition.All[i], $"{path}.all[{i}]")) yield return reference;
        if (condition.Any != null)
            for (var i = 0; i < condition.Any.Count; i++)
                foreach (var reference in ConditionRefs(condition.Any[i], $"{path}.any[{i}]")) yield return reference;
        foreach (var reference in ConditionRefs(condition.Not, $"{path}.not")) yield return reference;
    }
}
