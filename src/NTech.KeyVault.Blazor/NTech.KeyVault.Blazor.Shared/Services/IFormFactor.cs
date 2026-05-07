namespace NTech.KeyVault.Blazor.Shared.Services;

public interface IFormFactor
{
    public bool IsMobile {  get; }
    public string GetFormFactor();
    public string GetPlatform();
}
