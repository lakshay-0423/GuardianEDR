using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Guardian.Agent.Models;

namespace Guardian.Agent.Communication;

public sealed class AgentHeartbeatClient(HttpClient httpClient) : IAgentHeartbeatClient
{
    private const string HeartbeatPath = "api/agent/heartbeat";

    public async Task<AgentHeartbeatResult> SendAsync(
        AgentCredentials credentials,
        HeartbeatMetrics metrics,
        CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, HeartbeatPath)
            {
                Content = JsonContent.Create(new AgentHeartbeatRequest(
                    metrics.CpuUsage,
                    metrics.MemoryUsage,
                    DateTimeOffset.UtcNow)),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credentials.AgentToken);

            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return AgentHeartbeatResult.Failed(AgentHeartbeatFailure.AuthenticationRejected);
            }

            return response.IsSuccessStatusCode
                ? AgentHeartbeatResult.Success()
                : AgentHeartbeatResult.Failed(AgentHeartbeatFailure.UnexpectedResponse);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return AgentHeartbeatResult.Failed(AgentHeartbeatFailure.RequestTimedOut);
        }
        catch (HttpRequestException)
        {
            return AgentHeartbeatResult.Failed(AgentHeartbeatFailure.BackendUnavailable);
        }
    }
}
