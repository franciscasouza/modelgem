namespace ModelaFlow.Api.Domain.Customer;

public class Customer : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public ICollection<MeasurementSet> MeasurementSets { get; set; } = new List<MeasurementSet>();
}

/// <summary>
/// Versioned body measurements for a customer. New versions append; never overwrite.
/// Values are stored as JSON dictionary (keys from PatternCore, unit: cm).
/// </summary>
public class MeasurementSet : TenantEntity
{
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    /// <summary>Monotonic version per customer within a tenant, starting at 1.</summary>
    public int Version { get; set; }

    /// <summary>Measurement values in centimeters, keyed by canonical names.</summary>
    public Dictionary<string, decimal> ValuesCm { get; set; } = new(StringComparer.Ordinal);

    public int SchemaVersion { get; set; } = PatternCore.Measurements.MeasurementSchema.SchemaVersion;
    public Guid? CreatedByUserId { get; set; }
}
