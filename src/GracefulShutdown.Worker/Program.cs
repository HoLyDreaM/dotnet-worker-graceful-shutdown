using GracefulShutdown.Worker;

var builder = Host.CreateApplicationBuilder(args);

var shutdownSeconds = builder.Configuration.GetValue("Shutdown:HostTimeoutSeconds", 8);
builder.Services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(shutdownSeconds);
});

builder.Services.AddSingleton<InMemoryWorkQueue>();
builder.Services.AddSingleton<JobProcessor>();
builder.Services.AddSingleton<ShutdownStats>();
builder.Services.AddHostedService<QueueConsumerWorker>();

var host = builder.Build();
host.Run();
