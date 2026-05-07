using NTech.KeyVault.Blazor.Shared.Services;

namespace NTech.KeyVault.Blazor.Web.Services;

public class FormFactor : IFormFactor
{
    public bool IsMobile => false;

    public string GetFormFactor()
    {
        return "Web";
    }

    public string GetPlatform()
    {
        return Environment.OSVersion.ToString();
    }
}
