namespace ModelaFlow.Api.Domain;

/// <summary>Base for multi-tenant domain entities. Every query must filter by TenantId.</summary>
public abstract class TenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
