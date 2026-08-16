using ModelaFlow.Api.Domain.Identity;
using ModelaFlow.Api.Services;
using ModelaFlow.PatternCore.Measurements;

namespace ModelaFlow.Api.Endpoints;

public static class PlatformEndpoints
{
    public static RouteGroupBuilder MapPlatformEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/v1").WithTags("Platform");

        api.MapPost("/organizations", async (CreateOrganizationRequest request, PlatformService service, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["name"] = ["Name is required."]
                });
            }

            var org = await service.CreateOrganizationAsync(request.Name, ct);
            return Results.Created($"/api/v1/organizations/{org.Id}", new OrganizationResponse(org.Id, org.TenantId, org.Name, org.CreatedAt));
        });

        api.MapPost("/tenants/{tenantId:guid}/users", async (
            Guid tenantId,
            CreateUserRequest request,
            PlatformService service,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.DisplayName))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["email"] = ["Email and displayName are required."]
                });
            }

            try
            {
                var user = await service.CreateUserAsync(tenantId, request.Email, request.DisplayName, request.Role, ct);
                return Results.Created(
                    $"/api/v1/tenants/{tenantId}/users/{user.Id}",
                    new UserResponse(user.Id, user.TenantId, user.Email, user.DisplayName, user.Role.ToString(), user.CreatedAt));
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        });

        api.MapPost("/tenants/{tenantId:guid}/customers", async (
            Guid tenantId,
            CreateCustomerRequest request,
            PlatformService service,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["name"] = ["Name is required."]
                });
            }

            try
            {
                var customer = await service.CreateCustomerAsync(tenantId, request.Name, request.Notes, request.ActorUserId, ct);
                return Results.Created(
                    $"/api/v1/tenants/{tenantId}/customers/{customer.Id}",
                    new CustomerResponse(customer.Id, customer.TenantId, customer.Name, customer.Notes, customer.CreatedAt));
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        });

        api.MapGet("/tenants/{tenantId:guid}/customers", async (Guid tenantId, PlatformService service, CancellationToken ct) =>
        {
            var customers = await service.ListCustomersAsync(tenantId, ct);
            return Results.Ok(customers.Select(c => new CustomerResponse(c.Id, c.TenantId, c.Name, c.Notes, c.CreatedAt)));
        });

        api.MapPost("/tenants/{tenantId:guid}/customers/{customerId:guid}/measurement-sets", async (
            Guid tenantId,
            Guid customerId,
            CreateMeasurementSetRequest request,
            PlatformService service,
            CancellationToken ct) =>
        {
            var measurements = new BodyMeasurementsCm(
                BustCirc: request.BustCircCm,
                WaistCirc: request.WaistCircCm,
                HipCirc: request.HipCircCm,
                SkirtLength: request.SkirtLengthCm,
                DressLength: request.DressLengthCm,
                ShoulderWidth: request.ShoulderWidthCm,
                WaistToHip: request.WaistToHipCm,
                EaseBust: request.EaseBustCm,
                EaseWaist: request.EaseWaistCm,
                EaseHip: request.EaseHipCm);

            try
            {
                var set = await service.CreateMeasurementSetAsync(tenantId, customerId, measurements, request.ActorUserId, ct);
                return Results.Created(
                    $"/api/v1/tenants/{tenantId}/customers/{customerId}/measurement-sets/{set.Id}",
                    ToMeasurementResponse(set));
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        });

        api.MapGet("/tenants/{tenantId:guid}/customers/{customerId:guid}/measurement-sets", async (
            Guid tenantId,
            Guid customerId,
            PlatformService service,
            CancellationToken ct) =>
        {
            var versions = await service.ListMeasurementVersionsAsync(tenantId, customerId, ct);
            return Results.Ok(versions.Select(ToMeasurementResponse));
        });

        return api;
    }

    private static MeasurementSetResponse ToMeasurementResponse(Domain.Customer.MeasurementSet set) =>
        new(
            set.Id,
            set.TenantId,
            set.CustomerId,
            set.Version,
            set.SchemaVersion,
            MeasurementSchema.Unit,
            set.ValuesCm,
            set.CreatedByUserId,
            set.CreatedAt);
}

public sealed record CreateOrganizationRequest(string Name);

public sealed record CreateUserRequest(string Email, string DisplayName, UserRole Role = UserRole.Member);

public sealed record CreateCustomerRequest(string Name, string? Notes = null, Guid? ActorUserId = null);

public sealed record CreateMeasurementSetRequest(
    decimal? BustCircCm = null,
    decimal? WaistCircCm = null,
    decimal? HipCircCm = null,
    decimal? SkirtLengthCm = null,
    decimal? DressLengthCm = null,
    decimal? ShoulderWidthCm = null,
    decimal? WaistToHipCm = null,
    decimal? EaseBustCm = null,
    decimal? EaseWaistCm = null,
    decimal? EaseHipCm = null,
    Guid? ActorUserId = null);

public sealed record OrganizationResponse(Guid Id, Guid TenantId, string Name, DateTimeOffset CreatedAt);

public sealed record UserResponse(Guid Id, Guid TenantId, string Email, string DisplayName, string Role, DateTimeOffset CreatedAt);

public sealed record CustomerResponse(Guid Id, Guid TenantId, string Name, string? Notes, DateTimeOffset CreatedAt);

public sealed record MeasurementSetResponse(
    Guid Id,
    Guid TenantId,
    Guid CustomerId,
    int Version,
    int SchemaVersion,
    string Unit,
    IReadOnlyDictionary<string, decimal> ValuesCm,
    Guid? CreatedByUserId,
    DateTimeOffset CreatedAt);
