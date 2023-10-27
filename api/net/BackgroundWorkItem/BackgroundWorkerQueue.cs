using System.Collections.Concurrent;

namespace TNO.API.BackgroundWorkItem;

/// <summary>
///
/// </summary>
public interface IBackgroundTaskQueue
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    /// <summary>
    ///
    /// </summary>
    /// <param name="workItem"></param>
    void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);
}
/// <summary>
///
/// </summary>
public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new ConcurrentQueue<Func<CancellationToken, Task>>();
    private SemaphoreSlim _signal = new SemaphoreSlim(0);

    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);
        _workItems.TryDequeue(out var workItem);

        return workItem;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="workItem"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
    {
        if (workItem == null)
        {
            throw new ArgumentNullException(nameof(workItem));
        }

        _workItems.Enqueue(workItem);
        _signal.Release();
    }
}
