using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModelaFlow.Api.Data;

namespace ModelaFlow.Api.Tests;

public class AuthApiTests : IClassFixture<AuthApiTests.AuthWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AuthWebApplicationFactory _factory;

    public AuthApiTests(AuthWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_Login_Me_RoundTrip()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var email = $"owner-{Guid.NewGuid():N}@example.com";
        var register = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            organizationName = "Atelier Auth",
            email,
            displayName = "Owner",
            password = "ChangeMe!99"
        });

        Assert.Equal(HttpStatusCode.Created, register.StatusCode);
        var registered = await register.Content.ReadFromJsonAsync<AuthDto>(JsonOptions);
        Assert.NotNull(registered);
        Assert.Equal(email, registered!.Email);
        Assert.Equal("Owner", registered.Role);
        Assert.True(register.Headers.TryGetValues("Set-Cookie", out var cookies));
        Assert.Contains(cookies, c => c.StartsWith("mf_auth=", StringComparison.Ordinal));

        var meViaCookie = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.OK, meViaCookie.StatusCode);
        var me = await meViaCookie.Content.ReadFromJsonAsync<AuthDto>(JsonOptions);
        Assert.NotNull(me);
        Assert.Equal(registered.UserId, me!.UserId);
        Assert.Equal(registered.TenantId, me.TenantId);

        var logout = await client.PostAsync("/api/v1/auth/logout", null);
        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);

        var meAfterLogout = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meAfterLogout.StatusCode);

        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password = "ChangeMe!99"
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var meAfterLogin = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.OK, meAfterLogin.StatusCode);
    }

    [Fact]
    public async Task Login_InvalidPassword_Returns401()
    {
        var client = _factory.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@example.com";
        var register = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            organizationName = "Atelier",
            email,
            password = "ChangeMe!99"
        });
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password = "wrong-password"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task TenantRoutes_Unauthenticated_Return401()
    {
        var client = _factory.CreateClient();
        var tenantId = Guid.NewGuid();

        var customers = await client.GetAsync($"/api/v1/tenants/{tenantId}/customers");
        Assert.Equal(HttpStatusCode.Unauthorized, customers.StatusCode);

        var patterns = await client.GetAsync($"/api/v1/tenants/{tenantId}/patterns");
        Assert.Equal(HttpStatusCode.Unauthorized, patterns.StatusCode);
    }

    [Fact]
    public async Task TenantRoutes_CrossTenant_Return403()
    {
        var clientA = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        var clientB = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        var emailA = $"a-{Guid.NewGuid():N}@example.com";
        var emailB = $"b-{Guid.NewGuid():N}@example.com";

        var regA = await clientA.PostAsJsonAsync("/api/v1/auth/register", new
        {
            organizationName = "Org A",
            email = emailA,
            password = "ChangeMe!99"
        });
        var userA = await regA.Content.ReadFromJsonAsync<AuthDto>(JsonOptions);
        Assert.NotNull(userA);

        var regB = await clientB.PostAsJsonAsync("/api/v1/auth/register", new
        {
            organizationName = "Org B",
            email = emailB,
            password = "ChangeMe!99"
        });
        var userB = await regB.Content.ReadFromJsonAsync<AuthDto>(JsonOptions);
        Assert.NotNull(userB);

        // Authenticated as A, access B's tenant
        var cross = await clientA.GetAsync($"/api/v1/tenants/{userB!.TenantId}/customers");
        Assert.Equal(HttpStatusCode.Forbidden, cross.StatusCode);

        // Own tenant OK
        var own = await clientA.GetAsync($"/api/v1/tenants/{userA!.TenantId}/customers");
        Assert.Equal(HttpStatusCode.OK, own.StatusCode);

        // Bearer token also works for own tenant
        var login = await clientA.PostAsJsonAsync("/api/v1/auth/login", new { email = emailA, password = "ChangeMe!99" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        // Extract cookie token and call with Authorization header on a fresh client
        var setCookie = login.Headers.GetValues("Set-Cookie").First(c => c.StartsWith("mf_auth=", StringComparison.Ordinal));
        var token = setCookie.Split(';')[0]["mf_auth=".Length..];

        var bearerClient = _factory.CreateClient();
        bearerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var withBearer = await bearerClient.GetAsync($"/api/v1/tenants/{userA.TenantId}/patterns");
        Assert.Equal(HttpStatusCode.OK, withBearer.StatusCode);

        var crossBearer = await bearerClient.GetAsync($"/api/v1/tenants/{userB.TenantId}/patterns");
        Assert.Equal(HttpStatusCode.Forbidden, crossBearer.StatusCode);
    }

    public sealed class AuthWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = $"auth-tests-{Guid.NewGuid():N}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<ModelaFlowDbContext>));
                services.RemoveAll(typeof(ModelaFlowDbContext));

                services.AddDbContext<ModelaFlowDbContext>(options =>
                    options.UseInMemoryDatabase(_dbName));
            });
        }
    }

    private sealed record AuthDto(
        Guid UserId,
        string Email,
        string DisplayName,
        Guid TenantId,
        string OrganizationName,
        string Role);
}
