using System.Runtime.InteropServices;
using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public sealed class MachineIdentityProvider : IMachineIdentityProvider
{
    public MachineIdentity GetIdentity() => new(
        Hostname: Environment.MachineName,
        OperatingSystem: RuntimeInformation.OSDescription,
        Architecture: RuntimeInformation.OSArchitecture.ToString(),
        Username: Environment.UserName);
}
