namespace GracefulShutdown.Worker;

public sealed record WorkItem(string JobId, string Payload, TimeSpan Duration);
