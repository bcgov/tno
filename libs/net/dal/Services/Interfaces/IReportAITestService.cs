using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// IReportAITestService interface, stores the short-lived test runs of AI report sections so the
/// browser-facing API and the services API see the same run.
/// </summary>
public interface IReportAITestService : IBaseService
{
    /// <summary>
    /// Find a run.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    ReportAITest? FindById(Guid id);

    /// <summary>
    /// Find the run the user still has pending for the section, started on or after 'since'.
    /// </summary>
    /// <param name="requestorId"></param>
    /// <param name="sectionId"></param>
    /// <param name="since">Older pending runs are treated as lost.</param>
    /// <returns></returns>
    ReportAITest? FindPending(int requestorId, int sectionId, DateTime since);

    /// <summary>
    /// Start a run: the requestor's other runs of the section are removed first.
    /// </summary>
    /// <param name="test"></param>
    /// <returns></returns>
    ReportAITest Start(ReportAITest test);

    /// <summary>
    /// Record the outcome of a run. A run that is no longer stored is added back, so a late outcome
    /// still reaches the requestor.
    /// </summary>
    /// <param name="test">The outcome; only its outcome fields replace the stored row's.</param>
    /// <returns>The stored run.</returns>
    ReportAITest Complete(ReportAITest test);

    /// <summary>
    /// Remove a run.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Whether a run was removed.</returns>
    bool Delete(Guid id);

    /// <summary>
    /// Remove every run started before 'olderThan'.
    /// </summary>
    /// <param name="olderThan"></param>
    /// <returns>The number of runs removed.</returns>
    int Purge(DateTime olderThan);
}
