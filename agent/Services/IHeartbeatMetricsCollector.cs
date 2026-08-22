using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public interface IHeartbeatMetricsCollector
{
    HeartbeatMetrics Collect();
}
