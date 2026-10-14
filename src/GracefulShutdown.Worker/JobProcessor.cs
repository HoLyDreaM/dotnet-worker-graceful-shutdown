namespace GracefulShutdown.Worker;

public sealed class JobProcessor
{
    private readonly HashSet<string> _done = new(StringComparer.Ordinal);
    private readonly object _gate = new();

    public async Task<bool> ProcessAsync(WorkItem item, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (_done.Contains(item.JobId))
            {
                return true;
            }
        }

        await Task.Delay(item.Duration, cancellationToken);

        lock (_gate)
        {
            _done.Add(item.JobId);
        }

        return false;
    }
}
