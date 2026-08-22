using System.Net.Http.Json;
using Guardian.Agent.Models;

namespace Guardian.Agent.Communication;

public sealed class AgentRegistrationClient(HttpClient httpClient) : IAgentRegistrationClient
{
    private const string RegistrationPath = "api/agent/register";

    public async Task<AgentRegistrationResponse> RegisterAsync(
        AgentRegistrationRequest registrationRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                RegistrationPath,
                registrationRequest,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return AgentRegistrationResponse.Failure(
                    $"Registration endpoint returned HTTP {(int)response.StatusCode} ({response.StatusCode}).");
            }

            var payload = await response.Content.ReadFromJsonAsync<RegistrationApiResponse>(
                cancellationToken: cancellationToken);

            var registrationData = payload?.Data;

            if (payload is null
                || !payload.Success
                || string.IsNullOrWhiteSpace(registrationData?.AgentId)
                || string.IsNullOrWhiteSpace(registrationData.AgentToken)
                || registrationData.HeartbeatInterval <= 0)
            {
                return AgentRegistrationResponse.Failure("Registration endpoint returned an invalid response.");
            }

            return AgentRegistrationResponse.Success(
                registrationData.AgentId!,
                registrationData.AgentToken!,
                registrationData.HeartbeatInterval);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return AgentRegistrationResponse.Failure("Registration request timed out.");
        }
        catch (HttpRequestException)
        {
            return AgentRegistrationResponse.Failure("Guardian backend is unavailable.");
        }
        catch (NotSupportedException)
        {
            return AgentRegistrationResponse.Failure("Registration endpoint returned an unsupported response.");
        }
        catch (System.Text.Json.JsonException)
        {
            return AgentRegistrationResponse.Failure("Registration endpoint returned malformed JSON.");
        }
    }

    private sealed class RegistrationApiResponse
    {
        public bool Success { get; init; }

        public RegistrationData? Data { get; init; }
    }

    private sealed class RegistrationData
    {
        public string? AgentId { get; init; }

        public string? AgentToken { get; init; }

        public int HeartbeatInterval { get; init; }
    }
}
