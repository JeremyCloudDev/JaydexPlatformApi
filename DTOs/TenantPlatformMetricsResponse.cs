namespace JaydexPlatformApi.DTOs;

public class TenantPlatformMetricsResponse
{
    public long Admins { get; set; }

    public long Trainers { get; set; }

    public long Clients { get; set; }

    public long TotalUsers { get; set; }

    public long DatabaseSizeBytes { get; set; }

    public double DatabaseSizeMb { get; set; }

    public double DatabaseSizeGb { get; set; }

    public long UploadsSizeBytes { get; set; }

    public double UploadsSizeMb { get; set; }

    public long PrivateUploadsSizeBytes { get; set; }

    public double PrivateUploadsSizeMb { get; set; }

    public long FilesSizeBytes { get; set; }

    public double FilesSizeMb { get; set; }

    public long TotalStorageBytes { get; set; }

    public long TotalStorageMb { get; set; }

    public double TotalStorageGb { get; set; }
}