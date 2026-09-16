namespace JaydexPlatformApi.Models;

public class TenantConfig
{
    public Guid TenantId { get; set; }

    public string TenantCode { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string ApiBaseUrl { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    public string? PrimaryColor { get; set; }

    public string? SecondaryColor { get; set; }

    public bool IsActive { get; set; }
}