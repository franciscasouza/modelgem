namespace ModelaFlow.PatternCore.Validation;

/// <summary>
/// Min–max ranges from docs/discovery/measurement-schema.md (measurements.v1).
/// </summary>
public static class MeasurementRanges
{
    public static readonly (decimal Min, decimal Max) WaistCirc = (50m, 150m);
    public static readonly (decimal Min, decimal Max) HipCirc = (70m, 180m);
    public static readonly (decimal Min, decimal Max) BustCirc = (70m, 160m);
    public static readonly (decimal Min, decimal Max) SkirtLength = (30m, 120m);
    public static readonly (decimal Min, decimal Max) DressLength = (70m, 160m);
    public static readonly (decimal Min, decimal Max) WaistToHip = (14m, 30m);
    public static readonly (decimal Min, decimal Max) BustToWaist = (14m, 28m);
    public static readonly (decimal Min, decimal Max) ShoulderToBust = (20m, 32m);
    public static readonly (decimal Min, decimal Max) EaseWaist = (0m, 8m);
    public static readonly (decimal Min, decimal Max) EaseHip = (0m, 12m);
    public static readonly (decimal Min, decimal Max) EaseBust = (0m, 12m);
    public static readonly (decimal Min, decimal Max) SeamAllowance = (0.5m, 2.5m);
    public static readonly (decimal Min, decimal Max) HemAllowance = (1m, 8m);
    public static readonly (decimal Min, decimal Max) WaistbandHeight = (0m, 8m);
    public static readonly (decimal Min, decimal Max) NeckCirc = (30m, 45m);
    public static readonly (decimal Min, decimal Max) ShoulderWidth = (10m, 18m);
    public static readonly (decimal Min, decimal Max) ArmholeDepth = (15m, 28m);

    public static bool InRange(decimal value, (decimal Min, decimal Max) range) =>
        value >= range.Min && value <= range.Max;

    public static void RequireInRange(
        ValidationResult result,
        string key,
        decimal value,
        (decimal Min, decimal Max) range)
    {
        if (!InRange(value, range))
        {
            result.AddError(
                "out_of_range",
                $"{key}={value} cm is outside allowed range [{range.Min}, {range.Max}] cm.");
        }
    }
}
