namespace GracefulShutdown.Worker;

public sealed class QueueConsumerWorker : BackgroundService
{
    private readonly InMemoryWorkQueue _queue;
    private readonly JobProcessor _processor;
    private readonly ShutdownStats _stats;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<QueueConsumerWorker> _logger;
    private readonly TimeSpan _jobTimeout;
    private readonly TimeSpan _autoStopAfter;

    public QueueConsumerWorker(
        InMemoryWorkQueue queue,
        JobProcessor processor,
        ShutdownStats stats,
        IHostApplicationLifetime lifetime,
        IConfiguration configuration,
        ILogger<QueueConsumerWorker> logger)
    {
        _queue = queue;
        _processor = processor;
        _stats = stats;
        _lifetime = lifetime;
        _logger = logger;
        _jobTimeout = TimeSpan.FromSeconds(configuration.GetValue("Shutdown:JobTimeoutSeconds", 5));
        _autoStopAfter = TimeSpan.FromSeconds(configuration.GetValue("Demo:AutoStopAfterSeconds", 2));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Seed();
        _ = StopLaterAsync(stoppingToken);

        await foreach (var item in _queue.ReadAllAsync(stoppingToken))
        {
            if (stoppingToken.IsCancellationRequested)
            {
                LeaveRetryable(item, "kuyruktan alindi fakat islenmedi");
                break;
            }

            await HandleAsync(item, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        var started = DateTimeOffset.UtcNow;
        _logger.LogInformation("Kapanis basladi. Aktif is: {Active}", _stats.Active);
        _queue.StopAccepting();
        _logger.LogInformation("Yeni teslimat durduruldu.");

        await base.StopAsync(cancellationToken);

        var elapsed = DateTimeOffset.UtcNow - started;
        _logger.LogInformation(
            "Kapanis bitti. Sure={ElapsedMs}ms Tamamlanan={Completed} Iptal={Cancelled} YenidenTeslim={Retried} Aktif={Active}",
            (int)elapsed.TotalMilliseconds,
            _stats.Completed,
            _stats.Cancelled,
            _stats.Retried,
            _stats.Active);
    }

    private void Seed()
    {
        _queue.Enqueue(new WorkItem("job-1", "kisa", TimeSpan.FromMilliseconds(200)));
        _queue.Enqueue(new WorkItem("job-1", "kisa-tekrar", TimeSpan.FromMilliseconds(200)));
        _queue.Enqueue(new WorkItem("job-2", "uzun", TimeSpan.FromSeconds(30)));
        _queue.Enqueue(new WorkItem("job-3", "kuyrukta-kalacak", TimeSpan.FromMilliseconds(200)));
    }

    private async Task StopLaterAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(_autoStopAfter, cancellationToken);
            _lifetime.StopApplication();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task HandleAsync(WorkItem item, CancellationToken stoppingToken)
    {
        _stats.Enter();
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        linked.CancelAfter(_jobTimeout);

        try
        {
            var alreadyDone = await _processor.ProcessAsync(item, linked.Token);
            _stats.MarkCompleted();
            _logger.LogInformation(
                "Ack. JobId={JobId} IdempotentAtlama={Skipped}",
                item.JobId,
                alreadyDone);
        }
        catch (OperationCanceledException)
        {
            LeaveRetryable(item, "sure veya kapanis siniri");
        }
        finally
        {
            _stats.Leave();
        }
    }

    private void LeaveRetryable(WorkItem item, string reason)
    {
        _stats.MarkCancelled();
        _stats.MarkRetried();
        _logger.LogWarning(
            "Ack yok, yeniden denenebilir. JobId={JobId} Neden={Reason}",
            item.JobId,
            reason);
    }
}
