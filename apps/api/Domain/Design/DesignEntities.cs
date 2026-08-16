namespace ModelaFlow.Api.Domain.Design;

public enum PatternBaseKind
{
    StraightSkirt = 0,
    SimpleDress = 1,
    Blank = 2
}

public enum PatternModelStatus
{
    Draft = 0,
    Ready = 1
}

public enum ExportJobStatus
{
    Queued = 0,
    Running = 1,
    Succeeded = 2,
    Failed = 3
}

public sealed class PatternModel : TenantEntity
{
    public Guid? CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ReferenceCode { get; set; } = string.Empty;
    public PatternBaseKind BaseKind { get; set; } = PatternBaseKind.Blank;
    public PatternModelStatus Status { get; set; } = PatternModelStatus.Draft;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<PatternVersion> Versions { get; set; } = new List<PatternVersion>();
    public TechnicalSheet? TechnicalSheet { get; set; }
}

/// <summary>Append-only geometry/parameters snapshot for a pattern model.</summary>
public sealed class PatternVersion : TenantEntity
{
    public Guid PatternModelId { get; set; }
    public PatternModel? PatternModel { get; set; }

    /// <summary>Monotonic version per model within a tenant, starting at 1.</summary>
    public int Version { get; set; }

    public string ParametersJson { get; set; } = "{}";

    /// <summary>Serialized PatternDocument (pattern.v1). Null for blank until generated.</summary>
    public string? GeometryJson { get; set; }

    /// <summary>Quality issue codes JSON array. Never raw measurements.</summary>
    public string? QualityIssuesJson { get; set; }

    public Guid? CreatedByUserId { get; set; }
}

/// <summary>Minimal technical sheet — 1:1 with pattern model.</summary>
public sealed class TechnicalSheet : TenantEntity
{
    public Guid PatternModelId { get; set; }
    public PatternModel? PatternModel { get; set; }
    public string? MaterialsNotes { get; set; }
    public string? ConstructionNotes { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>In-process PDF export job (Redis queue deferred).</summary>
public sealed class ExportJob : TenantEntity
{
    public Guid PatternModelId { get; set; }
    public Guid? PatternVersionId { get; set; }
    public ExportJobStatus Status { get; set; } = ExportJobStatus.Queued;
    public string Format { get; set; } = "pdf_a4";
    public string? ErrorMessage { get; set; }
    public byte[]? ResultBytes { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
