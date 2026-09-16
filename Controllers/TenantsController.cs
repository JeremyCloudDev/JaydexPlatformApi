using JaydexPlatformApi.DTOs;
using JaydexPlatformApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace JaydexPlatformApi.Controllers;

[ApiController]
[Route("api/tenants")]
public class TenantsController : ControllerBase
{
    private readonly TenantService _tenantService;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(
        TenantService tenantService,
        ILogger<TenantsController> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    [HttpPost("resolve")]
    public async Task<IActionResult> Resolve(
        [FromBody] ResolveTenantRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new
            {
                message = "Gym code is required."
            });
        }

        try
        {
            var normalizedCode =
                request.Code.Trim().ToUpperInvariant();

            var tenant =
                await _tenantService.GetByCodeAsync(
                    normalizedCode,
                    cancellationToken
                );

            if (tenant is null)
            {
                return NotFound(new
                {
                    message = "Gym code was not found."
                });
            }

            return Ok(new ResolveTenantResponse
            {
                Id = tenant.TenantId,
                Code = tenant.TenantCode,
                DisplayName = tenant.DisplayName,
                ApiBaseUrl = tenant.ApiBaseUrl,
                LogoUrl = tenant.LogoUrl,
                PrimaryColor = tenant.PrimaryColor,
                SecondaryColor = tenant.SecondaryColor
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to resolve tenant code."
            );

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Unable to resolve gym code."
                }
            );
        }
    }
}