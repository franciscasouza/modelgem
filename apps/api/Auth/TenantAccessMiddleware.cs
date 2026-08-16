using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ModelaFlow.Api.Auth;

/// <summary>
/// Requires authentication for /api/v1/tenants/{tenantId}/... and ensures path tenant matches claim.
/// </summary>
public sealed partial class TenantAccessMiddleware
{
    private readonly RequestDelegate _next;

    public TenantAccessMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var match = TenantPathRegex().Match(path);
        if (!match.Success)
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Authentication required." });
            return;
        }

        if (!Guid.TryParse(match.Groups["tenantId"].Value, out var pathTenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid tenant id." });
            return;
        }

        var claimTenant = context.User.FindFirstValue(AuthClaims.TenantId)
            ?? context.User.FindFirstValue("tenant_id");

        if (!Guid.TryParse(claimTenant, out var userTenantId) || userTenantId != pathTenantId)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "Tenant access denied." });
            return;
        }

        await _next(context);
    }

    [GeneratedRegex(@"^/api/v1/tenants/(?<tenantId>[0-9a-fA-F-]{36})(/|$)", RegexOptions.CultureInvariant)]
    private static partial Regex TenantPathRegex();
}
