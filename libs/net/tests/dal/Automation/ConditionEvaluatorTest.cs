using System.Text.Json;
using TNO.API.Areas.Admin.Models.Automation;

namespace TNO.Test.DAL.Automation;

public class ConditionEvaluatorTest
{
    #region Helpers
    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    private static Func<string, string?> Fields(params (string Name, string? Value)[] fields)
    {
        var map = fields.ToDictionary(f => f.Name, f => f.Value, StringComparer.OrdinalIgnoreCase);
        return name => map.TryGetValue(name, out var value) ? value : null;
    }
    #endregion

    #region Tests
    [Fact]
    public void LengthLessThan_ShortBody_Passes()
    {
        var condition = new ConditionDefinition { Field = "body", Op = "lengthLessThan", Value = Json("100") };
        var result = ConditionEvaluator.Evaluate(condition, Fields(("body", "too short")));
        Assert.True(result.Passed);
        Assert.Contains("body", result.Detail);
    }

    [Fact]
    public void LengthLessThan_LongBody_Fails()
    {
        var condition = new ConditionDefinition { Field = "body", Op = "lengthLessThan", Value = Json("10") };
        var result = ConditionEvaluator.Evaluate(condition, Fields(("body", new string('x', 500))));
        Assert.False(result.Passed);
    }

    [Fact]
    public void NotIn_PageOutsideList_Passes()
    {
        var condition = new ConditionDefinition { Field = "page", Op = "notIn", Value = Json("[\"A1\",\"A2\",\"NP1\"]") };
        Assert.True(ConditionEvaluator.Evaluate(condition, Fields(("page", "D5"))).Passed);
        Assert.False(ConditionEvaluator.Evaluate(condition, Fields(("page", "a1"))).Passed);
    }

    [Fact]
    public void Any_Combinator_PassesWhenOneChildPasses()
    {
        // The exact 'page' rule from the Morning Process profile, as a declarative condition.
        var condition = new ConditionDefinition
        {
            Any = new List<ConditionDefinition>
            {
                new() { Field = "page", Op = "notIn", Value = Json("[\"A1\",\"A2\"]") },
                new() { Field = "section", Op = "in", Value = Json("[\"Scene\",\"Food\"]") },
            },
        };
        Assert.True(ConditionEvaluator.Evaluate(condition, Fields(("page", "A1"), ("section", "Food"))).Passed);
        Assert.False(ConditionEvaluator.Evaluate(condition, Fields(("page", "A1"), ("section", "News"))).Passed);
    }

    [Fact]
    public void All_Combinator_FailsWhenAnyChildFails()
    {
        var condition = new ConditionDefinition
        {
            All = new List<ConditionDefinition>
            {
                new() { Field = "page", Op = "equals", Value = Json("\"A1\"") },
                new() { Field = "body", Op = "lengthGreaterThan", Value = Json("5") },
            },
        };
        Assert.True(ConditionEvaluator.Evaluate(condition, Fields(("page", "A1"), ("body", "long enough"))).Passed);
        Assert.False(ConditionEvaluator.Evaluate(condition, Fields(("page", "A2"), ("body", "long enough"))).Passed);
    }

    [Fact]
    public void Not_InvertsChild()
    {
        var condition = new ConditionDefinition
        {
            Not = new ConditionDefinition { Field = "section", Op = "exists" },
        };
        Assert.True(ConditionEvaluator.Evaluate(condition, Fields(("section", null))).Passed);
        Assert.False(ConditionEvaluator.Evaluate(condition, Fields(("section", "News"))).Passed);
    }

    [Fact]
    public void From_BooleanAnalysisGate_UsesResolver()
    {
        var condition = new ConditionDefinition { From = "triage.publish" };
        Assert.True(ConditionEvaluator.Evaluate(condition, Fields(), _ => true).Passed);
        Assert.False(ConditionEvaluator.Evaluate(condition, Fields(), _ => false).Passed);
        // No resolver: fail closed with an explanatory detail rather than throwing.
        var result = ConditionEvaluator.Evaluate(condition, Fields());
        Assert.False(result.Passed);
        Assert.Contains("no boolean result", result.Detail);
    }

    /// <summary>
    /// A reference paired with an operator compares the referenced value - how an action gates on
    /// what an earlier action in its step did.
    /// </summary>
    [Fact]
    public void From_WithOperator_ComparesTheReferencedValue()
    {
        var condition = new ConditionDefinition { From = "Publish Content.outcome", Op = "equals", Value = Json("\"executed\"") };
        var outcomes = new Dictionary<string, string> { ["Publish Content.outcome"] = "executed" };
        Assert.True(ConditionEvaluator.Evaluate(condition, Fields(), null, r => outcomes.GetValueOrDefault(r)).Passed);

        outcomes["Publish Content.outcome"] = "skipped";
        var failed = ConditionEvaluator.Evaluate(condition, Fields(), null, r => outcomes.GetValueOrDefault(r));
        Assert.False(failed.Passed);
        Assert.Contains("Publish Content.outcome", failed.Detail);
    }

    /// <summary>
    /// An action that never ran at all - disabled, or stopped before its turn - publishes nothing,
    /// so its reference resolves to null. Every positive gate on it fails; a negated one passes,
    /// which is what makes the editor's 'did not run' option cover the never-reached case too.
    /// </summary>
    [Fact]
    public void UnresolvedReference_FailsPositiveGates_AndPassesNegatedOnes()
    {
        Func<string, string?> unresolved = _ => null;
        var ran = new ConditionDefinition { From = "Publish Content.outcome", Op = "equals", Value = Json("\"executed\"") };
        var didNotRun = new ConditionDefinition { From = "Publish Content.outcome", Op = "notEquals", Value = Json("\"executed\"") };
        var boolGate = new ConditionDefinition { From = "Publish Content.executed" };
        var notBoolGate = new ConditionDefinition { Not = new ConditionDefinition { From = "Publish Content.executed" } };

        Assert.False(ConditionEvaluator.Evaluate(ran, Fields(), null, unresolved).Passed);
        Assert.True(ConditionEvaluator.Evaluate(didNotRun, Fields(), null, unresolved).Passed);
        Assert.False(ConditionEvaluator.Evaluate(boolGate, Fields(), _ => null, unresolved).Passed);
        Assert.True(ConditionEvaluator.Evaluate(notBoolGate, Fields(), _ => null, unresolved).Passed);
    }

    /// <summary>'Did not run' covers every outcome that is not 'executed', blocked ones included.</summary>
    [Fact]
    public void DidNotRunGate_MatchesEveryNonExecutedOutcome()
    {
        var didNotRun = new ConditionDefinition { From = "Publish Content.outcome", Op = "notEquals", Value = Json("\"executed\"") };
        foreach (var outcome in new[] { Outcomes.Skipped, Outcomes.Failed, Outcomes.ConditionFailed, Outcomes.NotConfirmed })
            Assert.True(ConditionEvaluator.Evaluate(didNotRun, Fields(), null, _ => outcome).Passed, outcome);
        Assert.False(ConditionEvaluator.Evaluate(didNotRun, Fields(), null, _ => Outcomes.Executed).Passed);
    }

    /// <summary>Without an operator a reference is still read as the boolean it always was.</summary>
    [Fact]
    public void From_WithoutOperator_StaysTheBooleanGate()
    {
        var condition = new ConditionDefinition { From = "Publish Content.executed" };
        Assert.True(ConditionEvaluator.Evaluate(condition, Fields(), _ => true, _ => "never read").Passed);
        Assert.False(ConditionEvaluator.Evaluate(condition, Fields(), _ => false, _ => "never read").Passed);
    }

    [Fact]
    public void HasTag_MatchesTokenList()
    {
        var condition = new ConditionDefinition { Field = "tags", Op = "hasTag", Value = Json("\"HLTH\"") };
        Assert.True(ConditionEvaluator.Evaluate(condition, Fields(("tags", "FIN, HLTH ,EDU"))).Passed);
        Assert.False(ConditionEvaluator.Evaluate(condition, Fields(("tags", "FIN,EDU"))).Passed);
    }

    [Fact]
    public void GreaterThan_NonNumericActual_Fails()
    {
        var condition = new ConditionDefinition { Field = "sentiment", Op = "greaterThan", Value = Json("2") };
        Assert.False(ConditionEvaluator.Evaluate(condition, Fields(("sentiment", "positive"))).Passed);
        Assert.True(ConditionEvaluator.Evaluate(condition, Fields(("sentiment", "4"))).Passed);
    }

    [Fact]
    public void UnknownOperator_FailsWithDetail()
    {
        var condition = new ConditionDefinition { Field = "page", Op = "resembles", Value = Json("\"A1\"") };
        var result = ConditionEvaluator.Evaluate(condition, Fields(("page", "A1")));
        Assert.False(result.Passed);
        Assert.Contains("unknown operator", result.Detail);
    }

    [Fact]
    public void Detail_ExplainsWhatWasCompared()
    {
        // The run log records the detail so a failed gate is explainable.
        var condition = new ConditionDefinition { Field = "page", Op = "in", Value = Json("[\"A1\"]") };
        var result = ConditionEvaluator.Evaluate(condition, Fields(("page", "D5")));
        Assert.Contains("page in", result.Detail);
        Assert.Contains("\"D5\"", result.Detail);
        Assert.Contains("failed", result.Detail);
    }
    #endregion
}
