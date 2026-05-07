namespace NTech.KeyVault.Blazor.Shared.Components.CrossPlatform
{
    public sealed record HostValidationResult(
        bool IsValid,
        string? Scheme,
        string? Host,
        int? Port,
        bool IsIpAddress,
        bool IsIpv6,
        bool IsLocalhost,
        string FullAddress
    );
}
