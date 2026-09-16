using System.Net.Http.Json;
using JaydexPlatformApi.DTOs;

namespace JaydexPlatformApi.Services;

public class TenantMetricsCollectorService
{
    private readonly HttpClient _httpClient;
    private readonly TenantService _tenantService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TenantMetricsCollectorService> _logger;

    public TenantMetricsCollectorService(
        HttpClient httpClient,
        TenantService tenantService,
        IConfiguration configuration,
        ILogger<TenantMetricsCollectorService> logger)
    {
        _httpClient = httpClient;
        _tenantService = tenantService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task CollectAllAsync(
        CancellationToken cancellationToken = default)
    {
        var tenants =
            await _tenantService.GetActiveTenantsAsync(
                cancellationToken
            );

        foreach (var tenant in tenants)
        {
            try
            {
                await CollectTenantAsync(
                    tenant.TenantId,
                    tenant.TenantCode,
                    tenant.ApiBaseUrl,
                    cancellationToken
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to collect metrics for tenant {TenantCode}.",
                    tenant.TenantCode
                );
            }
        }
    }

    public async Task CollectTenantAsync(
        Guid tenantId,
        string tenantCode,
        string apiBaseUrl,
        CancellationToken cancellationToken = default)
    {
        var apiKey =
            _configuration[
                $"TenantMetrics:ApiKeys:{tenantCode}"
            ];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning(
                "Metrics collection skipped for {TenantCode}: API key is not configured.",
                tenantCode
            );

            return;
        }

        var normalizedBaseUrl =
            apiBaseUrl.TrimEnd('/');

        var requestUrl =
            $"{normalizedBaseUrl}/api/internal/platform-metrics";

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                requestUrl
            );

        request.Headers.Add(
            "X-Jaydex-Platform-Key",
            apiKey
        );

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken
            );

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Metrics request for {TenantCode} returned HTTP {StatusCode}.",
                tenantCode,
                (int)response.StatusCode
            );

            return;
        }

        var metrics =
            await response.Content
                .ReadFromJsonAsync<TenantPlatformMetricsResponse>(
                    cancellationToken: cancellationToken
                );

        if (metrics is null)
        {
            _logger.LogWarning(
                "Metrics response for {TenantCode} was empty.",
                tenantCode
            );

            return;
        }

        await _tenantService.UpdateMetricsAsync(
            tenantId,
            metrics,
            cancellationToken
        );

        _logger.LogInformation(
            "Metrics updated successfully for {TenantCode}.",
            tenantCode
        );
    }
}