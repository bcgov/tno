namespace TNO.Test.Entities;

using System.Diagnostics.CodeAnalysis;
using TNO.Entities;
using TNO.Entities.Models;

[Trait("category", "unit")]
[Trait("group", "entities")]
[ExcludeFromCodeCoverage]
public class AnalysisMetadataTest
{
    private static AnalysisRun Run(string requestId, AnalysisRunStatus status, DateTime requestedOn)
        => new() { RequestId = requestId, Status = status, RequestedOn = requestedOn, FinishedOn = DateTime.UtcNow };

    [Fact]
    public void Record_TheNewestRequestIsTheStatus()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var metadata = new AnalysisMetadata();

        // Act
        metadata.Record(Run("newer", AnalysisRunStatus.Completed, now));
        metadata.Record(Run("older", AnalysisRunStatus.Failed, now.AddMinutes(-1)));

        // Assert
        Assert.Equal(AnalysisRunStatus.Completed, metadata.Status);
        Assert.Equal(new[] { "newer", "older" }, metadata.Runs.Select(r => r.RequestId));
    }

    [Fact]
    public void Record_ARetryReplacesItsRequestsRun()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var metadata = new AnalysisMetadata();
        metadata.Record(Run("request", AnalysisRunStatus.Retrying, now));

        // Act
        metadata.Record(Run("request", AnalysisRunStatus.Failed, now));

        // Assert
        var run = Assert.Single(metadata.Runs);
        Assert.Equal(AnalysisRunStatus.Failed, run.Status);
        Assert.Equal(AnalysisRunStatus.Failed, metadata.Status);
    }

    [Fact]
    public void Record_KeepsTheNewestRuns()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var metadata = new AnalysisMetadata();

        // Act
        for (var i = 0; i < AnalysisMetadata.MaxRuns + 3; i++)
            metadata.Record(Run($"run-{i}", AnalysisRunStatus.Completed, now.AddMinutes(i)));
        // Older than every kept run, so it is not kept.
        metadata.Record(Run("late", AnalysisRunStatus.Failed, now.AddMinutes(-1)));

        // Assert
        Assert.Equal(AnalysisMetadata.MaxRuns, metadata.Runs.Count);
        Assert.Equal($"run-{AnalysisMetadata.MaxRuns + 2}", metadata.Runs[0].RequestId);
        Assert.DoesNotContain(metadata.Runs, r => r.RequestId == "late");
        Assert.Equal(AnalysisRunStatus.Completed, metadata.Status);
    }
}
