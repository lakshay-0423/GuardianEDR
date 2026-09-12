using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Guardian.Agent.Models;

namespace Guardian.Agent.Communication;

public sealed class AgentEventClient(HttpClient httpClient) : IAgentEventClient
{
    private const string EventIngestionPath = "api/agent/events";

    public async Task<AgentEventTransmissionResult> SendAsync(
        AgentCredentials credentials,
        ProcessTelemetryEvent processEvent,
        CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, EventIngestionPath)
            {
                Content = JsonContent.Create(new AgentProcessEventRequest(
                    ToEventType(processEvent.EventType),
                    processEvent.Timestamp,
                    new ProcessEventPayload(
                        processEvent.ProcessId,
                        processEvent.ProcessName,
                        processEvent.ExecutablePath,
                        processEvent.ParentProcessId,
                        processEvent.ParentProcessName,
                        processEvent.CommandLine,
                        processEvent.Username))),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credentials.AgentToken);

            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return AgentEventTransmissionResult.Failed(AgentEventTransmissionFailure.AuthenticationRejected);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return AgentEventTransmissionResult.Failed(AgentEventTransmissionFailure.InvalidPayload);
            }

            return response.IsSuccessStatusCode
                ? AgentEventTransmissionResult.Success()
                : AgentEventTransmissionResult.Failed(AgentEventTransmissionFailure.UnexpectedResponse);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return AgentEventTransmissionResult.Failed(AgentEventTransmissionFailure.RequestTimedOut);
        }
        catch (HttpRequestException)
        {
            return AgentEventTransmissionResult.Failed(AgentEventTransmissionFailure.BackendUnavailable);
        }
    }

    private static string ToEventType(ProcessEventType eventType) => eventType switch
    {
        ProcessEventType.Started => "PROCESS_STARTED",
        ProcessEventType.Terminated => "PROCESS_TERMINATED",
        _ => throw new ArgumentOutOfRangeException(nameof(eventType), eventType, "Unsupported process event type."),
    };
}
