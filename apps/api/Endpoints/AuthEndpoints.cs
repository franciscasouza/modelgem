using System.Security.Claims;
using Microsoft.Extensions.Options;
using ModelaFlow.Api.Auth;
using ModelaFlow.Api.Services;

namespace ModelaFlow.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/v1/auth").WithTags("Auth");

        api.MapPost("/register", async (
            RegisterRequest request,
            AuthService auth,
            HttpContext http,
            IOptions<AuthOptions> authOptions,
            IHostEnvironment env,
            CancellationToken ct) =>
        {
            try
            {
                var session = await auth.RegisterAsync(
                    request.OrganizationName,
                    request.Email,
                    request.DisplayName,
                    request.Password,
                    ct);

                AuthCookieHelper.SetAuthCookie(http, session.AccessToken, authOptions.Value, env);
                return Results.Created("/api/v1/auth/me", ToAuthResponse(session));
            }
            catch (AuthValidationException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.Field] = [ex.Message]
                });
            }
            catch (AuthConflictException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        api.MapPost("/login", async (
            LoginRequest request,
            AuthService auth,
            HttpContext http,
            IOptions<AuthOptions> authOptions,
            IHostEnvironment env,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["credentials"] = ["Email and password are required."]
                });
            }

            var session = await auth.LoginAsync(request.Email, request.Password, ct);
            if (session is null)
                return Results.Unauthorized();

            AuthCookieHelper.SetAuthCookie(http, session.AccessToken, authOptions.Value, env);
            return Results.Ok(ToAuthResponse(session));
        });

        api.MapPost("/logout", async (
            AuthService auth,
            HttpContext http,
            IOptions<AuthOptions> authOptions,
            IHostEnvironment env,
            CancellationToken ct) =>
        {
            Guid? userId = null;
            Guid? tenantId = null;
            if (http.User.Identity?.IsAuthenticated == true)
            {
                if (Guid.TryParse(http.User.FindFirstValue(AuthClaims.UserId)
                        ?? http.User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? http.User.FindFirstValue("sub"), out var uid))
                    userId = uid;
                if (Guid.TryParse(http.User.FindFirstValue(AuthClaims.TenantId), out var tid))
                    tenantId = tid;
            }

            await auth.LogoutAsync(tenantId, userId, ct);
            AuthCookieHelper.ClearAuthCookie(http, authOptions.Value, env);
            return Results.Ok(new { ok = true });
        });

        api.MapGet("/me", async (AuthService auth, HttpContext http, CancellationToken ct) =>
        {
            if (http.User.Identity?.IsAuthenticated != true)
                return Results.Unauthorized();

            var rawId = http.User.FindFirstValue(AuthClaims.UserId)
                ?? http.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? http.User.FindFirstValue("sub");

            if (!Guid.TryParse(rawId, out var userId))
                return Results.Unauthorized();

            var me = await auth.GetMeAsync(userId, ct);
            if (me is null)
                return Results.Unauthorized();

            return Results.Ok(new AuthMeResponse(
                me.UserId,
                me.Email,
                me.DisplayName,
                me.TenantId,
                me.OrganizationName,
                me.Role));
        }).RequireAuthorization();

        return api;
    }

    private static AuthSessionResponse ToAuthResponse(AuthSessionResult session) =>
        new(
            session.User.Id,
            session.User.Email,
            session.User.DisplayName,
            session.User.TenantId,
            session.OrganizationName,
            session.User.Role.ToString());
}

public sealed record RegisterRequest(
    string OrganizationName,
    string Email,
    string Password,
    string? DisplayName = null);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthSessionResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    Guid TenantId,
    string OrganizationName,
    string Role);

public sealed record AuthMeResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    Guid TenantId,
    string OrganizationName,
    string Role);
