namespace ModelaFlow.PatternCore.Bases;

using ModelaFlow.PatternCore.Pattern;
using ModelaFlow.PatternCore.Validation;

/// <summary>
/// Input for straight skirt base v1 (cm).
/// MVP decision: 2 parts (front + back), symmetric; waistband_height = 0 (no separate waistband).
/// </summary>
public sealed record StraightSkirtInput
{
    public required decimal WaistCirc { get; init; }
    public required decimal HipCirc { get; init; }
    public required decimal SkirtLength { get; init; }
    public decimal EaseWaist { get; init; } = 2m;
    public decimal EaseHip { get; init; } = 4m;
    public decimal WaistToHip { get; init; } = 20m;
    public decimal SeamAllowance { get; init; } = 1.0m;
    public decimal HemAllowance { get; init; } = 3.0m;
    public decimal WaistbandHeight { get; init; } = 0m;
    public bool LengthIncludesHem { get; init; }
}

/// <summary>
/// Deterministic straight skirt (saia reta) — schema pattern.v1 / base straight_skirt.v1.
/// No AI involved; same input → same geometry.
/// </summary>
public static class StraightSkirtPattern
{
    public const string BaseId = "straight_skirt";
    public const string BaseVersion = "v1";

    public static readonly IReadOnlyList<string> Limitations =
    [
        "MVP: exactly 2 parts (front + back), symmetric; no single-panel option.",
        "waistband_height must be 0 in base v1 (no separate waistband piece).",
        "No darts, godês, or complex waist finishes.",
        "Side seams use mild cubic shaping between stations; not industrial grading."
    ];

    public static PatternDocument Generate(StraightSkirtInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        Validate(input).ThrowIfInvalid();

        var waistEff = input.WaistCirc + input.EaseWaist;
        var hipEff = input.HipCirc + input.EaseHip;
        var waistHalf = PanelBuilder.HalfWidthFromCircumference(waistEff);
        var hipHalf = PanelBuilder.HalfWidthFromCircumference(hipEff);

        var stations = new List<(decimal Y, decimal HalfWidth)>
        {
            (0m, waistHalf),
            (input.WaistToHip, hipHalf),
            (input.SkirtLength, hipHalf)
        };

        var front = PanelBuilder.BuildSymmetricPanel(
            "skirt_front",
            "Saia frente",
            PieceSide.Front,
            stations,
            input.SeamAllowance,
            input.HemAllowance,
            input.SkirtLength,
            input.LengthIncludesHem);

        var back = PanelBuilder.BuildSymmetricPanel(
            "skirt_back",
            "Saia costas",
            PieceSide.Back,
            stations,
            input.SeamAllowance,
            input.HemAllowance,
            input.SkirtLength,
            input.LengthIncludesHem);

        var resolved = new Dictionary<string, decimal>(StringComparer.Ordinal)
        {
            ["waist_circ"] = input.WaistCirc,
            ["hip_circ"] = input.HipCirc,
            ["skirt_length"] = input.SkirtLength,
            ["ease_waist"] = input.EaseWaist,
            ["ease_hip"] = input.EaseHip,
            ["waist_to_hip"] = input.WaistToHip,
            ["seam_allowance"] = input.SeamAllowance,
            ["hem_allowance"] = input.HemAllowance,
            ["waistband_height"] = input.WaistbandHeight,
            ["waist_effective"] = waistEff,
            ["hip_effective"] = hipEff
        };

        return new PatternDocument
        {
            BaseId = BaseId,
            BaseVersion = BaseVersion,
            Pieces = [front, back],
            ResolvedParametersCm = resolved,
            Limitations = Limitations
        };
    }

    public static ValidationResult Validate(StraightSkirtInput input)
    {
        var result = new ValidationResult();

        MeasurementRanges.RequireInRange(result, "waist_circ", input.WaistCirc, MeasurementRanges.WaistCirc);
        MeasurementRanges.RequireInRange(result, "hip_circ", input.HipCirc, MeasurementRanges.HipCirc);
        MeasurementRanges.RequireInRange(result, "skirt_length", input.SkirtLength, MeasurementRanges.SkirtLength);
        MeasurementRanges.RequireInRange(result, "ease_waist", input.EaseWaist, MeasurementRanges.EaseWaist);
        MeasurementRanges.RequireInRange(result, "ease_hip", input.EaseHip, MeasurementRanges.EaseHip);
        MeasurementRanges.RequireInRange(result, "waist_to_hip", input.WaistToHip, MeasurementRanges.WaistToHip);
        MeasurementRanges.RequireInRange(result, "seam_allowance", input.SeamAllowance, MeasurementRanges.SeamAllowance);
        MeasurementRanges.RequireInRange(result, "hem_allowance", input.HemAllowance, MeasurementRanges.HemAllowance);
        MeasurementRanges.RequireInRange(result, "waistband_height", input.WaistbandHeight, MeasurementRanges.WaistbandHeight);

        if (input.WaistbandHeight != 0m)
        {
            result.AddError(
                "waistband_not_in_base_v1",
                "waistband_height must be 0 for straight_skirt.v1 (separate waistband out of scope).");
        }

        var waistEff = input.WaistCirc + input.EaseWaist;
        var hipEff = input.HipCirc + input.EaseHip;
        if (hipEff < waistEff)
        {
            result.AddError(
                "hip_lt_waist_effective",
                $"hip_circ+ease_hip ({hipEff}) must be ≥ waist_circ+ease_waist ({waistEff}).");
        }

        if (input.HipCirc < input.WaistCirc - 2m)
        {
            result.AddWarning(
                "hip_waist_anatomy",
                "hip_circ is more than 2 cm below waist_circ; confirm anatomy/marking.");
        }

        if (input.SkirtLength < input.WaistToHip + 8m)
        {
            result.AddError(
                "skirt_too_short",
                $"skirt_length ({input.SkirtLength}) must be ≥ waist_to_hip+8 ({input.WaistToHip + 8m}).");
        }

        return result;
    }
}
