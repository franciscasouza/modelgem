namespace ModelaFlow.Api.Domain.Audit;

public class AuditEvent : TenantEntity
{
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public Guid? ActorUserId { get; set; }
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Non-sensitive metadata only (ids, counts). Never store raw body measures or images.</summary>
    public string? MetadataJson { get; set; }
}
