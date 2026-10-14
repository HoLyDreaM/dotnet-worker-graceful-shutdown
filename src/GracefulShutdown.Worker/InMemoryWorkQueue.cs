using System.Threading.Channels;

namespace GracefulShutdown.Worker;

public sealed class InMemoryWorkQueue
{
    private readonly Channel<WorkItem> _channel = Channel.CreateUnbounded<WorkItem>();
    private int _accepting = 1;

    public bool IsAccepting => Volatile.Read(ref _accepting) == 1;

    public bool Enqueue(WorkItem item)
    {
        if (!IsAccepting)
        {
            return false;
        }

        return _channel.Writer.TryWrite(item);
    }

    public void StopAccepting()
    {
        if (Interlocked.Exchange(ref _accepting, 0) == 1)
        {
            _channel.Writer.TryComplete();
        }
    }

    public IAsyncEnumerable<WorkItem> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
