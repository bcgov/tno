using TNO.API.Areas.Admin.Models.Automation.V2;

namespace TNO.Test.DAL.Automation;

public class AutomationDefinitionValidatorTest
{
    #region Helpers
    /// <summary>A minimal valid definition: search into a collection, process it, complete.</summary>
    private static AutomationDefinition ValidDefinition() => AutomationDefinition.Parse("""
    {
      "prompts": { "rules": "Review the story. {lookup:tags}" },
      "saveMode": "end-of-run",
      "steps": [
        {
          "name": "Load", "phase": "init",
          "actions": [ { "type": "search", "filter": 11, "into": "$run.inbox" } ]
        },
        {
          "name": "Process", "phase": "process",
          "source": { "from": "collection", "collection": "$run.inbox" },
          "analyses": [
            { "name": "triage", "prompt": { "ref": "rules" }, "returns": { "sentiment": "int(-5..5)", "publish": "bool" } }
          ],
          "actions": [
            { "type": "exclude", "when": { "field": "body", "op": "lengthLessThan", "value": 100 } },
            { "type": "content.sentiment", "value": { "from": "triage.sentiment" } },
            { "type": "content.publish", "when": { "from": "triage.publish" } }
          ]
        },
        {
          "name": "Distribute", "phase": "complete",
          "actions": [ { "type": "report.run", "report": 5 } ]
        }
      ]
    }
    """);
    #endregion

    #region Tests
    [Fact]
    public void ValidDefinition_HasNoErrors()
    {
        var errors = AutomationDefinitionValidator.Validate(ValidDefinition());
        Assert.Empty(errors.Where(e => e.Severity == "error"));
    }

    [Fact]
    public void UnknownActionType_IsAnError()
    {
        var definition = ValidDefinition();
        definition.Steps[1].Actions.Add(new V2ActionDefinition { Type = "content.teleport" });
        var errors = AutomationDefinitionValidator.Validate(definition);
        Assert.Contains(errors, e => e.Severity == "error" && e.Message.Contains("content.teleport"));
    }

    [Fact]
    public void ProcessStepWithoutSource_IsAnError()
    {
        var definition = ValidDefinition();
        definition.Steps[1].Source = null;
        var errors = AutomationDefinitionValidator.Validate(definition);
        Assert.Contains(errors, e => e.Path == "steps[1].source" && e.Severity == "error");
    }

    [Fact]
    public void InitStepWithSource_IsAnError()
    {
        var definition = ValidDefinition();
        definition.Steps[0].Source = new V2SourceDefinition { From = "collection", Collection = "$run.inbox" };
        var errors = AutomationDefinitionValidator.Validate(definition);
        Assert.Contains(errors, e => e.Path == "steps[0].source");
    }

    [Fact]
    public void ProfileSource_IsAnError()
    {
        // v2 has no profile filter: content enters a run through 'search' actions.
        var definition = ValidDefinition();
        definition.Steps[1].Source = new V2SourceDefinition { From = "profile" };
        var errors = AutomationDefinitionValidator.Validate(definition);
        Assert.Contains(errors, e => e.Path == "steps[1].source.from" && e.Severity == "error");
    }

    [Fact]
    public void PhaseOrderViolation_IsAnError()
    {
        var definition = ValidDefinition();
        // Move the init step after the process step: init cannot follow process.
        var init = definition.Steps[0];
        definition.Steps.RemoveAt(0);
        definition.Steps.Insert(1, init);
        var errors = AutomationDefinitionValidator.Validate(definition);
        Assert.Contains(errors, e => e.Message.Contains("order steps init"));
    }

    [Fact]
    public void UnknownCollectionSource_IsAnError()
    {
        var definition = ValidDefinition();
        definition.Steps[1].Source!.Collection = "$run.nowhere";
        var errors = AutomationDefinitionValidator.Validate(definition);
        Assert.Contains(errors, e => e.Message.Contains("$run.nowhere") && e.Severity == "error");
    }

    [Fact]
    public void CollectionNameWithoutRunPrefix_IsAnError()
    {
        var definition = ValidDefinition();
        definition.Steps[0].Actions[0].Into = "inbox";
        var errors = AutomationDefinitionValidator.Validate(definition);
        Assert.Contains(errors, e => e.Message.Contains("$run."));
    }

    [Fact]
    public void UnknownAnalysisReference_IsAnError()
    {
        var definition = ValidDefinition();
        definition.Steps[1].Actions[1].Value = new V2ValueSource { From = "missing.sentiment" };
        var errors = AutomationDefinitionValidator.Validate(definition);
        Assert.Contains(errors, e => e.Message.Contains("'missing'"));
    }

    [Fact]
    public void UnknownPromptRef_IsAnError()
    {
        var definition = ValidDefinition();
        definition.Steps[1].Analyses[0].Prompt.Ref = "nonexistent";
        var errors = AutomationDefinitionValidator.Validate(definition);
        Assert.Contains(errors, e => e.Message.Contains("nonexistent"));
    }

    [Fact]
    public void UnconsumedAnalysis_IsAWarning()
    {
        var definition = ValidDefinition();
        definition.Steps[1].Analyses.Add(new V2AnalysisDefinition
        {
            Name = "orphan",
            Prompt = new V2PromptDefinition { Text = "unused" },
            Returns = new Dictionary<string, string> { ["x"] = "string" },
        });
        var errors = AutomationDefinitionValidator.Validate(definition);
        // Lazy analyses that nothing consumes never run - a warning, not an error.
        Assert.Contains(errors, e => e.Severity == "warning" && e.Message.Contains("orphan"));
    }

    [Fact]
    public void ConditionWithTwoShapes_IsAnError()
    {
        var definition = ValidDefinition();
        definition.Steps[1].Actions[0].When = new V2ConditionDefinition
        {
            Field = "body",
            Op = "isEmpty",
            From = "triage.publish",
        };
        var errors = AutomationDefinitionValidator.Validate(definition);
        Assert.Contains(errors, e => e.Message.Contains("exactly one shape"));
    }

    [Fact]
    public void SubjectActionInCompleteStep_IsAnError()
    {
        var definition = ValidDefinition();
        definition.Steps[2].Actions.Add(new V2ActionDefinition { Type = "content.publish" });
        var errors = AutomationDefinitionValidator.Validate(definition);
        Assert.Contains(errors, e => e.Path.StartsWith("steps[2]") && e.Message.Contains("process"));
    }

    [Fact]
    public void DraftTargetWithoutCreate_IsAnError()
    {
        var definition = ValidDefinition();
        definition.Steps[1].Actions.Add(new V2ActionDefinition
        {
            Type = "content.update",
            Field = "headline",
            Value = new V2ValueSource { Literal = System.Text.Json.JsonDocument.Parse("\"x\"").RootElement.Clone() },
            Target = "$item.ghost",
        });
        var errors = AutomationDefinitionValidator.Validate(definition);
        Assert.Contains(errors, e => e.Message.Contains("$item.ghost"));
    }

    [Fact]
    public void DefinitionRoundTrip_PreservesShape()
    {
        var definition = ValidDefinition();
        var reparsed = AutomationDefinition.Parse(definition.ToJson());
        Assert.Equal(definition.Steps.Count, reparsed.Steps.Count);
        Assert.Equal(definition.SaveMode, reparsed.SaveMode);
        Assert.Equal("triage", reparsed.Steps[1].Analyses[0].Name);
        Assert.Equal(100, reparsed.Steps[1].Actions[0].When!.Value!.Value.GetInt32());
        Assert.Empty(AutomationDefinitionValidator.Validate(reparsed).Where(e => e.Severity == "error"));
    }
    #endregion
}
