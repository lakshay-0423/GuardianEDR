using System.Security.Cryptography;
using System.Text.Json;
using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public sealed class WindowsAgentCredentialStore : IAgentCredentialStore
{
    private const string CredentialsFileName = "agent-credentials.dat";
    private const string StorageDirectoryName = "GuardianEDR";

    public async Task StoreAsync(AgentCredentials credentials, CancellationToken cancellationToken)
    {
        var storageDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            StorageDirectoryName);
        Directory.CreateDirectory(storageDirectory);

        var plaintext = JsonSerializer.SerializeToUtf8Bytes(credentials);
        var protectedCredentials = ProtectedData.Protect(
            plaintext,
            optionalEntropy: null,
            DataProtectionScope.CurrentUser);
        CryptographicOperations.ZeroMemory(plaintext);

        var credentialsPath = Path.Combine(storageDirectory, CredentialsFileName);
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
}
