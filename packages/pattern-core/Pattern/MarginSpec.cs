namespace ModelaFlow.PatternCore.Pattern;

/// <summary>
/// Seam and hem allowances in centimeters (business unit).
/// </summary>
public sealed record MarginSpec(
    decimal SeamAllowanceCm,
    decimal HemAllowanceCm)
{
    public static MarginSpec Defaults { get; } = new(1.0m, 3.0m);
}
