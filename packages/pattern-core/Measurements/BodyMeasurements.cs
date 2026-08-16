namespace ModelaFlow.PatternCore.Measurements;

/// <summary>
/// Canonical measurement schema (business unit: centimeters).
/// Source of truth for ids: docs/discovery/measurement-schema.md
/// Geometric pattern rules live elsewhere; this package only carries typed measures.
/// </summary>
public static class MeasurementSchema
{
    public const string Unit = "cm";
    public const string SchemaId = "measurements.v1";
    public const int SchemaVersion = 1;
}

/// <summary>
/// Canonical body/garment measurement keys (MVP: saia reta / vestido simples).
/// </summary>
public static class MeasurementKeys
{
    public const string BustCirc = "bust_circ";
    public const string WaistCirc = "waist_circ";
    public const string HipCirc = "hip_circ";
    public const string SkirtLength = "skirt_length";
    public const string DressLength = "dress_length";
    public const string ShoulderWidth = "shoulder_width";
    public const string WaistToHip = "waist_to_hip";
    public const string EaseBust = "ease_bust";
    public const string EaseWaist = "ease_waist";
    public const string EaseHip = "ease_hip";
}

/// <summary>
/// Typed measures in centimeters. Null means not provided.
/// </summary>
public sealed record BodyMeasurementsCm(
    decimal? BustCirc = null,
    decimal? WaistCirc = null,
    decimal? HipCirc = null,
    decimal? SkirtLength = null,
    decimal? DressLength = null,
    decimal? ShoulderWidth = null,
    decimal? WaistToHip = null,
    decimal? EaseBust = null,
    decimal? EaseWaist = null,
    decimal? EaseHip = null)
{
    public static BodyMeasurementsCm Empty { get; } = new();

    public IReadOnlyDictionary<string, decimal> ToDictionary()
    {
        var map = new Dictionary<string, decimal>(StringComparer.Ordinal);
        Add(map, MeasurementKeys.BustCirc, BustCirc);
        Add(map, MeasurementKeys.WaistCirc, WaistCirc);
        Add(map, MeasurementKeys.HipCirc, HipCirc);
        Add(map, MeasurementKeys.SkirtLength, SkirtLength);
        Add(map, MeasurementKeys.DressLength, DressLength);
        Add(map, MeasurementKeys.ShoulderWidth, ShoulderWidth);
        Add(map, MeasurementKeys.WaistToHip, WaistToHip);
        Add(map, MeasurementKeys.EaseBust, EaseBust);
        Add(map, MeasurementKeys.EaseWaist, EaseWaist);
        Add(map, MeasurementKeys.EaseHip, EaseHip);
        return map;
    }

    public static BodyMeasurementsCm FromDictionary(IReadOnlyDictionary<string, decimal> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        return new BodyMeasurementsCm(
            Get(values, MeasurementKeys.BustCirc),
            Get(values, MeasurementKeys.WaistCirc),
            Get(values, MeasurementKeys.HipCirc),
            Get(values, MeasurementKeys.SkirtLength),
            Get(values, MeasurementKeys.DressLength),
            Get(values, MeasurementKeys.ShoulderWidth),
            Get(values, MeasurementKeys.WaistToHip),
            Get(values, MeasurementKeys.EaseBust),
            Get(values, MeasurementKeys.EaseWaist),
            Get(values, MeasurementKeys.EaseHip));
    }

    private static void Add(IDictionary<string, decimal> map, string key, decimal? value)
    {
        if (value is { } v) map[key] = v;
    }

    private static decimal? Get(IReadOnlyDictionary<string, decimal> values, string key) =>
        values.TryGetValue(key, out var value) ? value : null;
}
