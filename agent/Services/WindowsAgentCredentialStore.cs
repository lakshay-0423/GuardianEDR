using System.Security.Cryptography;
using System.Text.Json;
using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public sealed class WindowsAgentCredentialStore : IAgentCredentialStore
{
    private const string CredentialsFileName = "agent-credentials.dat";
    private const string StorageDirectoryName = "GuardianEDR";

    public async Task<AgentCredentialLoadResult> LoadAsync(CancellationToken cancellationToken)
    {
        var credentialsPath = GetCredentialsPath();

        if (!File.Exists(credentialsPath))
        {
            return AgentCredentialLoadResult.Missing();
        }

        byte[]? protectedCredentials = null;
        byte[]? plaintext = null;

        try
        {
            protectedCredentials = await File.ReadAllBytesAsync(credentialsPath, cancellationToken);
            plaintext = ProtectedData.Unprotect(
                protectedCredentials,
                optionalEntropy: null,
                DataProtectionScope.CurrentUser);
            var credentials = JsonSerializer.Deserialize<AgentCredentials>(plaintext);

            return IsValid(credentials)
                ? AgentCredentialLoadResult.Available(credentials!)
                : AgentCredentialLoadResult.Corrupted();
        }
        catch (FileNotFoundException)
        {
            return AgentCredentialLoadResult.Missing();
        }
        catch (DirectoryNotFoundException)
        {
            return AgentCredentialLoadResult.Missing();
        }
        catch (CryptographicException)
        {
            return AgentCredentialLoadResult.Corrupted();
        }
        catch (JsonException)
        {
            return AgentCredentialLoadResult.Corrupted();
        }
        catch (IOException)
        {
            return AgentCredentialLoadResult.Unavailable();
        }
        catch (UnauthorizedAccessException)
        {
            return AgentCredentialLoadResult.Unavailable();
        }
        finally
        {
            if (protectedCredentials is not null)
            {
                CryptographicOperations.ZeroMemory(protectedCredentials);
            }

            if (plaintext is not null)
            {
                CryptographicOperations.ZeroMemory(plaintext);
            }
        }
    }

    public async Task StoreAsync(AgentCredentials credentials, CancellationToken cancellationToken)
    {
        var storageDirectory = GetStorageDirectory();
        Directory.CreateDirectory(storageDirectory);

        var plaintext = JsonSerializer.SerializeToUtf8Bytes(credentials);
        var protectedCredentials = ProtectedData.Protect(
            plaintext,
            optionalEntropy: null,
            DataProtectionScope.CurrentUser);
        CryptographicOperations.ZeroMemory(plaintext);

        var credentialsPath = GetCredentialsPath();
        var temporaryPath = Path.Combine(storageDirectory, $"{CredentialsFileName}.{Guid.NewGuid():N}.tmp");

        try
        {
            await File.WriteAllBytesAsync(temporaryPath, protectedCredentials, cancellationToken);
            File.Move(temporaryPath, credentialsPath, overwrite: true);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(protectedCredentials);

            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static string GetStorageDirectory() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        StorageDirectoryName);

    private static string GetCredentialsPath() => Path.Combine(
        GetStorageDirectory(),
        CredentialsFileName);

    private static bool IsValid(AgentCredentials? credentials) => credentials is
    {
        AgentId.Length: > 0,
        AgentToken.Length: > 0,
        HeartbeatIntervalSeconds: > 0,
        RegisteredAtUtc: { } registeredAt,
    } && registeredAt != default;
}
