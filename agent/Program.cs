using Guardian.Agent.Communication;
using Guardian.Agent.Configuration;
using Guardian.Agent.Logging;
using Guardian.Agent.Services;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

if (!OperatingSystem.IsWindows())
{
    Console.Error.WriteLine("Guardian Agent can only run on Windows.");
    Environment.ExitCode = 1;
    return;
}

builder.Configuration.AddEnvironmentVariables(prefix: "GUARDIAN_AGENT_");
builder.Logging.ConfigureGuardianLogging(builder.Configuration);

builder.Services
    .AddOptions<AgentOptions>()
    .Bind(builder.Configuration.GetSection(AgentOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<AgentRegistrationOptions>()
    .Bind(builder.Configuration.GetSection(AgentRegistrationOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(
        options => Uri.TryCreate(options.BackendBaseUrl, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
        "Registration:BackendBaseUrl must use HTTP or HTTPS.")
    .ValidateOnStart();

builder.Services
    .AddOptions<HeartbeatOptions>()
    .Bind(builder.Configuration.GetSection(HeartbeatOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<ProcessMonitoringOptions>()
    .Bind(builder.Configuration.GetSection(ProcessMonitoringOptions.SectionName))
    .ValidateOnStart();

builder.Services
    .AddOptions<ProcessEventTransmissionOptions>()
    .Bind(builder.Configuration.GetSection(ProcessEventTransmissionOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<FileMonitoringOptions>()
    .Bind(builder.Configuration.GetSection(FileMonitoringOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<ISystemInformationCollector, SystemInformationCollector>();
builder.Services.AddSingleton<IAgentCredentialStore, WindowsAgentCredentialStore>();
builder.Services.AddSingleton<IAgentRegistrationService, AgentRegistrationService>();
builder.Services.AddSingleton<IHeartbeatMetricsCollector, SystemHeartbeatMetricsCollector>();
builder.Services.AddSingleton<IProcessMonitor, WmiProcessMonitor>();
builder.Services.AddSingleton<IFileMonitoringPathResolver, WindowsFileMonitoringPathResolver>();
builder.Services.AddSingleton<IFileMonitor, FileSystemWatcherMonitor>();
builder.Services.AddSingleton<IFileTelemetrySink, StructuredFileTelemetryLogger>();
builder.Services.AddSingleton<StructuredProcessTelemetryLogger>();
builder.Services.AddSingleton<IProcessEventBuffer, ProcessEventBuffer>();
builder.Services.AddSingleton<IProcessTelemetrySink>(serviceProvider =>
{
    var telemetryLogger = serviceProvider.GetRequiredService<StructuredProcessTelemetryLogger>();
    var eventBuffer = serviceProvider.GetRequiredService<IProcessEventBuffer>();
    return new CompositeProcessTelemetrySink(telemetryLogger, eventBuffer);
});
builder.Services.AddHttpClient<IAgentRegistrationClient, AgentRegistrationClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<AgentRegistrationOptions>>().Value;
    client.BaseAddress = new Uri($"{options.BackendBaseUrl.TrimEnd('/')}/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});
builder.Services.AddHttpClient<IAgentHeartbeatClient, AgentHeartbeatClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<AgentRegistrationOptions>>().Value;
    client.BaseAddress = new Uri($"{options.BackendBaseUrl.TrimEnd('/')}/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});
builder.Services.AddHttpClient<IAgentEventClient, AgentEventClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<AgentRegistrationOptions>>().Value;
    client.BaseAddress = new Uri($"{options.BackendBaseUrl.TrimEnd('/')}/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});
builder.Services.AddHostedService<AgentHostedService>();
builder.Services.AddHostedService<HeartbeatHostedService>();
builder.Services.AddHostedService<ProcessMonitoringHostedService>();
builder.Services.AddHostedService<ProcessEventTransmissionHostedService>();
builder.Services.AddHostedService<FileMonitoringHostedService>();

using var host = builder.Build();
await host.RunAsync();
