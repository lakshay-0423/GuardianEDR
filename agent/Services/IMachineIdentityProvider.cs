using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public interface IMachineIdentityProvider
{
    MachineIdentity GetIdentity();
}
