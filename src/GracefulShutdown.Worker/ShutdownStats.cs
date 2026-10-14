namespace GracefulShutdown.Worker;

public sealed class ShutdownStats
{
    private int _active;
    private int _completed;
    private int _cancelled;
    private int _retried;

    public int Active => Volatile.Read(ref _active);
    public int Completed => Volatile.Read(ref _completed);
    public int Cancelled => Volatile.Read(ref _cancelled);
    public int Retried => Volatile.Read(ref _retried);

    public void Enter() => Interlocked.Increment(ref _active);

    public void Leave() => Interlocked.Decrement(ref _active);

    public void MarkCompleted() => Interlocked.Increment(ref _completed);

    public void MarkCancelled() => Interlocked.Increment(ref _cancelled);

    public void MarkRetried() => Interlocked.Increment(ref _retried);
}
