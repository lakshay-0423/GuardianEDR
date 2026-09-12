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

builder.Services.AddSingleton<ISystemInformationCollector, SystemInformationCollector>();
builder.Services.AddSingleton<IAgentCredentialStore, WindowsAgentCredentialStore>();
builder.Services.AddSingleton<IAgentRegistrationService, AgentRegistrationService>();
builder.Services.AddSingleton<IHeartbeatMetricsCollector, SystemHeartbeatMetricsCollector>();
builder.Services.AddSingleton<IProcessMonitor, WmiProcessMonitor>();
builder.Services.AddSingleton<IProcessTelemetrySink, StructuredProcessTelemetryLogger>();
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
builder.Services.AddHostedService<AgentHostedService>();
builder.Services.AddHostedService<HeartbeatHostedService>();
builder.Services.AddHostedService<ProcessMonitoringHostedService>();

using var host = builder.Build();
await host.RunAsync();
