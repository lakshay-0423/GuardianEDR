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

builder.Services.AddSingleton<IMachineIdentityProvider, MachineIdentityProvider>();
builder.Services.AddHostedService<AgentHostedService>();

using var host = builder.Build();
await host.RunAsync();
