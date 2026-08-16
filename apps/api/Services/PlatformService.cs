using Microsoft.EntityFrameworkCore;
using ModelaFlow.Api.Data;
using ModelaFlow.Api.Domain.Audit;
using ModelaFlow.Api.Domain.Customer;
using ModelaFlow.Api.Domain.Identity;
using ModelaFlow.PatternCore.Measurements;

namespace ModelaFlow.Api.Services;

public sealed class PlatformService
{
    private readonly ModelaFlowDbContext _db;

    public PlatformService(ModelaFlowDbContext db)
    {
        _db = db;
    }

    public async Task<Organization> CreateOrganizationAsync(string name, CancellationToken ct = default)
    {
        var id = Guid.NewGuid();
        var org = new Organization
        {
            Id = id,
            TenantId = id,
            Name = name.Trim()
        };

        _db.Organizations.Add(org);
        await AddAuditAsync(org.TenantId, null, "organization.created", nameof(Organization), org.Id, null, ct);
        await _db.SaveChangesAsync(ct);
        return org;
    }

    public async Task<User> CreateUserAsync(
        Guid tenantId,
        string email,
        string displayName,
        UserRole role,
        CancellationToken ct = default)
    {
        await EnsureOrganizationAsync(tenantId, ct);

        var user = new User
        {
            TenantId = tenantId,
            OrganizationId = tenantId,
            Email = email.Trim().ToLowerInvariant(),
            DisplayName = displayName.Trim(),
            Role = role,
            PasswordHash = string.Empty,
            SecurityStamp = Guid.NewGuid().ToString("N")
        };

        _db.Users.Add(user);
        await AddAuditAsync(tenantId, user.Id, "user.created", nameof(User), user.Id, null, ct);
        await _db.SaveChangesAsync(ct);
        return user;
    }

    public async Task<Customer> CreateCustomerAsync(
        Guid tenantId,
        string name,
        string? notes,
        Guid? actorUserId,
        CancellationToken ct = default)
    {
        await EnsureOrganizationAsync(tenantId, ct);

        var customer = new Customer
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };

        _db.Customers.Add(customer);
        await AddAuditAsync(tenantId, actorUserId, "customer.created", nameof(Customer), customer.Id, null, ct);
        await _db.SaveChangesAsync(ct);
        return customer;
    }

    public async Task<Customer?> GetCustomerAsync(Guid tenantId, Guid customerId, CancellationToken ct = default) =>
        await _db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == customerId, ct);

    public async Task<IReadOnlyList<Customer>> ListCustomersAsync(Guid tenantId, CancellationToken ct = default) =>
        await _db.Customers
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public async Task<MeasurementSet> CreateMeasurementSetAsync(
        Guid tenantId,
        Guid customerId,
        BodyMeasurementsCm measurements,
        Guid? actorUserId,
        CancellationToken ct = default)
    {
        var customer = await _db.Customers
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == customerId, ct)
            ?? throw new InvalidOperationException("Customer not found for tenant.");

        var nextVersion = await _db.MeasurementSets
            .Where(m => m.TenantId == tenantId && m.CustomerId == customer.Id)
            .Select(m => (int?)m.Version)
            .MaxAsync(ct) ?? 0;

        var set = new MeasurementSet
        {
            TenantId = tenantId,
            CustomerId = customer.Id,
            Version = nextVersion + 1,
            ValuesCm = new Dictionary<string, decimal>(measurements.ToDictionary(), StringComparer.Ordinal),
            SchemaVersion = MeasurementSchema.SchemaVersion,
            CreatedByUserId = actorUserId
        };

        _db.MeasurementSets.Add(set);
        await AddAuditAsync(
            tenantId,
            actorUserId,
            "measurement_set.created",
            nameof(MeasurementSet),
            set.Id,
            $"{{\"version\":{set.Version},\"customerId\":\"{customer.Id}\"}}",
            ct);
        await _db.SaveChangesAsync(ct);
        return set;
    }

    public async Task<IReadOnlyList<MeasurementSet>> ListMeasurementVersionsAsync(
        Guid tenantId,
        Guid customerId,
        CancellationToken ct = default) =>
        await _db.MeasurementSets
            .AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.CustomerId == customerId)
            .OrderByDescending(m => m.Version)
            .ToListAsync(ct);

    private async Task EnsureOrganizationAsync(Guid tenantId, CancellationToken ct)
    {
        var exists = await _db.Organizations.AnyAsync(o => o.TenantId == tenantId, ct);
        if (!exists)
        {
            throw new InvalidOperationException("Organization (tenant) not found.");
        }
    }

    private Task AddAuditAsync(
        Guid tenantId,
        Guid? actorUserId,
        string action,
        string entityType,
        Guid entityId,
        string? metadataJson,
        CancellationToken ct)
    {
        _db.AuditEvents.Add(new AuditEvent
        {
            TenantId = tenantId,
            ActorUserId = actorUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            MetadataJson = metadataJson,
            OccurredAt = DateTimeOffset.UtcNow
        });
        return Task.CompletedTask;
    }
}
