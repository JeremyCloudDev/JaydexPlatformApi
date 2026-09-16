using JaydexPlatformApi.Models;
using JaydexPlatformApi.DTOs;
using Npgsql;

namespace JaydexPlatformApi.Services;

public class TenantService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TenantService> _logger;

    public TenantService(
        IConfiguration configuration,
        ILogger<TenantService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<TenantConfig?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var connectionString =
            _configuration.GetConnectionString(
                "DefaultConnection"
            );

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection is not configured."
            );
        }

        const string sql = """
            SELECT
                fldtenant_id,
                fldtenant_code,
                flddisplay_name,
                fldapi_base_url,
                fldlogo_url,
                fldprimary_color,
                fldsecondary_color,
                fldis_active
            FROM tbltenants
            WHERE UPPER(fldtenant_code) = UPPER(@code)
              AND fldis_active = TRUE
            LIMIT 1;
            """;

        await using var connection =
            new NpgsqlConnection(connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "code",
            code.Trim()
        );

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken
            );

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new TenantConfig
        {
            TenantId = reader.GetGuid(0),
            TenantCode = reader.GetString(1),
            DisplayName = reader.GetString(2),
            ApiBaseUrl = reader.GetString(3),

            LogoUrl = reader.IsDBNull(4)
                ? null
                : reader.GetString(4),

            PrimaryColor = reader.IsDBNull(5)
                ? null
                : reader.GetString(5),

            SecondaryColor = reader.IsDBNull(6)
                ? null
                : reader.GetString(6),

            IsActive = reader.GetBoolean(7)
        };
    }

    public async Task<List<TenantConfig>> GetActiveTenantsAsync(
    CancellationToken cancellationToken = default)
    {
        var connectionString =
            _configuration.GetConnectionString(
                "DefaultConnection"
            );

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection is not configured."
            );
        }

        const string sql = """
        SELECT
            fldtenant_id,
            fldtenant_code,
            flddisplay_name,
            fldapi_base_url,
            fldlogo_url,
            fldprimary_color,
            fldsecondary_color,
            fldis_active
        FROM tbltenants
        WHERE fldis_active = TRUE
        ORDER BY fldtenant_code;
        """;

        var tenants = new List<TenantConfig>();

        await using var connection =
            new NpgsqlConnection(connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new NpgsqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken
            );

        while (await reader.ReadAsync(cancellationToken))
        {
            tenants.Add(new TenantConfig
            {
                TenantId = reader.GetGuid(0),
                TenantCode = reader.GetString(1),
                DisplayName = reader.GetString(2),
                ApiBaseUrl = reader.GetString(3),

                LogoUrl = reader.IsDBNull(4)
                    ? null
                    : reader.GetString(4),

                PrimaryColor = reader.IsDBNull(5)
                    ? null
                    : reader.GetString(5),

                SecondaryColor = reader.IsDBNull(6)
                    ? null
                    : reader.GetString(6),

                IsActive = reader.GetBoolean(7)
            });
        }

        return tenants;
    }

    public async Task UpdateMetricsAsync(
    Guid tenantId,
    TenantPlatformMetricsResponse metrics,
    CancellationToken cancellationToken = default)
    {
        var connectionString =
            _configuration.GetConnectionString(
                "DefaultConnection"
            );

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection is not configured."
            );
        }

        const string sql = """
        UPDATE tbltenants
        SET
            fldtotal_admins = @admins,
            fldtotal_trainers = @trainers,
            fldtotal_clients = @clients,
            fldtotal_users = @totalUsers,
            fldstorage_used_mb = @storageUsedMb,
            fldstorage_last_updated = NOW(),
            fldupdated_at = NOW()
        WHERE fldtenant_id = @tenantId;
        """;

        await using var connection =
            new NpgsqlConnection(connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "admins",
            checked((int)metrics.Admins)
        );

        command.Parameters.AddWithValue(
            "trainers",
            checked((int)metrics.Trainers)
        );

        command.Parameters.AddWithValue(
            "clients",
            checked((int)metrics.Clients)
        );

        command.Parameters.AddWithValue(
            "totalUsers",
            checked((int)metrics.TotalUsers)
        );

        command.Parameters.AddWithValue(
            "storageUsedMb",
            metrics.TotalStorageMb
        );

        command.Parameters.AddWithValue(
            "tenantId",
            tenantId
        );

        await command.ExecuteNonQueryAsync(
            cancellationToken
        );
    }

}