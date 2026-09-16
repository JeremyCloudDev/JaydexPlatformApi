namespace JaydexPlatformApi.DTOs;

public class ResolveTenantResponse
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string ApiBaseUrl { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    public string? PrimaryColor { get; set; }

    public string? SecondaryColor { get; set; }
}