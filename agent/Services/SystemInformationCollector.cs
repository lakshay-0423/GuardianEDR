using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using Guardian.Agent.Models;
using Microsoft.Win32;

namespace Guardian.Agent.Services;

public sealed class SystemInformationCollector : ISystemInformationCollector
{
    private const string CurrentVersionRegistryPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
    private const string CryptographyRegistryPath = @"SOFTWARE\Microsoft\Cryptography";

    public EndpointSystemInformation Collect() => new(
        Hostname: GetSafely(() => Environment.MachineName),
        WindowsOsName: GetRegistryValue(CurrentVersionRegistryPath, "ProductName"),
        WindowsOsVersion: GetSafely(() => Environment.OSVersion.Version.ToString()),
        Architecture: GetSafely(() => RuntimeInformation.OSArchitecture.ToString()),
        CurrentUsername: GetSafely(() => Environment.UserName),
        AgentVersion: GetSafely(GetAgentVersion),
        DeviceIdentifier: GetRegistryValue(CryptographyRegistryPath, "MachineGuid"),
        LocalIpAddress: GetSafely(GetLocalIpv4Address));

    private static string? GetAgentVersion() => Assembly.GetEntryAssembly()
        ?.GetName()
        .Version
        ?.ToString();

    private static string? GetLocalIpv4Address() => Dns
        .GetHostEntry(Dns.GetHostName())
        .AddressList
        .Where(address => address.AddressFamily == AddressFamily.InterNetwork)
        .FirstOrDefault(address => !IPAddress.IsLoopback(address))
        ?.ToString();

    private static string? GetRegistryValue(string registryPath, string valueName) => GetSafely(() =>
    {
        using var registryKey = Registry.LocalMachine.OpenSubKey(registryPath);
        return registryKey?.GetValue(valueName)?.ToString();
    });

    private static string? GetSafely(Func<string?> valueFactory)
    {
        try
        {
            return valueFactory();
        }
        catch (Exception exception) when (
            exception is InvalidOperationException
            or PlatformNotSupportedException
            or System.ComponentModel.Win32Exception
            or System.Security.SecurityException
            or UnauthorizedAccessException
            or SocketException)
        {
            return null;
        }
    }
}
