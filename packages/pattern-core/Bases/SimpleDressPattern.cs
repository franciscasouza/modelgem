namespace ModelaFlow.PatternCore.Bases;

using ModelaFlow.PatternCore.Pattern;
using ModelaFlow.PatternCore.Validation;

/// <summary>
/// Input for simple dress base v1 (cm). Silhouette: straight / lightly waisted; no complex sleeves.
/// </summary>
public sealed record SimpleDressInput
{
    public required decimal BustCirc { get; init; }
    public required decimal WaistCirc { get; init; }
    public required decimal HipCirc { get; init; }
    public required decimal DressLength { get; init; }
    public decimal EaseBust { get; init; } = 4m;
    public decimal EaseWaist { get; init; } = 2m;
    public decimal EaseHip { get; init; } = 4m;
    public decimal ShoulderToBust { get; init; } = 26m;
    public decimal BustToWaist { get; init; } = 20m;
    public decimal WaistToHip { get; init; } = 20m;
    public decimal SeamAllowance { get; init; } = 1.0m;
    public decimal HemAllowance { get; init; } = 3.0m;
    public bool LengthIncludesHem { get; init; }
    /// <summary>MVP: only shoulder origin is supported for length consistency checks.</summary>
    public string LengthFrom { get; init; } = "shoulder";
}

/// <summary>
/// Deterministic simple dress (vestido simples) — base simple_dress.v1.
/// 2 parts (front + back), slightly waisted tube; no complex sleeves/armholes in v1.
/// </summary>
public static class SimpleDressPattern
{
    public const string BaseId = "simple_dress";
    public const string BaseVersion = "v1";

    public static readonly IReadOnlyList<string> Limitations =
    [
        "MVP: 2 parts (front + back), symmetric; no sleeves, godês, or structured lining.",
        "Neckline is a straight shoulder/top line (no shaped neck opening in v1).",
        "length_from must be shoulder for consistency rule in v1.",
        "Light waist shaping only; not a fitted princess-seam dress."
    ];

    public static PatternDocument Generate(SimpleDressInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        Validate(input).ThrowIfInvalid();

        var bustEff = input.BustCirc + input.EaseBust;
        var waistEff = input.WaistCirc + input.EaseWaist;
        var hipEff = input.HipCirc + input.EaseHip;

        var bustHalf = PanelBuilder.HalfWidthFromCircumference(bustEff);
        var waistHalf = PanelBuilder.HalfWidthFromCircumference(waistEff);
        var hipHalf = PanelBuilder.HalfWidthFromCircumference(hipEff);

        var yBust = input.ShoulderToBust;
        var yWaist = yBust + input.BustToWaist;
        var yHip = yWaist + input.WaistToHip;

        var stations = new List<(decimal Y, decimal HalfWidth)>
        {
            (0m, bustHalf),           // shoulder / top
            (yBust, bustHalf),
            (yWaist, waistHalf),
            (yHip, hipHalf),
            (input.DressLength, hipHalf)
        };

        var front = PanelBuilder.BuildSymmetricPanel(
            "dress_front",
            "Vestido frente",
            PieceSide.Front,
            stations,
            input.SeamAllowance,
            input.HemAllowance,
            input.DressLength,
            input.LengthIncludesHem);

        var back = PanelBuilder.BuildSymmetricPanel(
            "dress_back",
            "Vestido costas",
            PieceSide.Back,
            stations,
            input.SeamAllowance,
            input.HemAllowance,
            input.DressLength,
            input.LengthIncludesHem);

        var resolved = new Dictionary<string, decimal>(StringComparer.Ordinal)
        {
            ["bust_circ"] = input.BustCirc,
            ["waist_circ"] = input.WaistCirc,
            ["hip_circ"] = input.HipCirc,
            ["dress_length"] = input.DressLength,
            ["ease_bust"] = input.EaseBust,
            ["ease_waist"] = input.EaseWaist,
            ["ease_hip"] = input.EaseHip,
            ["shoulder_to_bust"] = input.ShoulderToBust,
            ["bust_to_waist"] = input.BustToWaist,
            ["waist_to_hip"] = input.WaistToHip,
            ["seam_allowance"] = input.SeamAllowance,
            ["hem_allowance"] = input.HemAllowance,
            ["bust_effective"] = bustEff,
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

    public static ValidationResult Validate(SimpleDressInput input)
    {
        var result = new ValidationResult();

        MeasurementRanges.RequireInRange(result, "bust_circ", input.BustCirc, MeasurementRanges.BustCirc);
        MeasurementRanges.RequireInRange(result, "waist_circ", input.WaistCirc, MeasurementRanges.WaistCirc);
        MeasurementRanges.RequireInRange(result, "hip_circ", input.HipCirc, MeasurementRanges.HipCirc);
        MeasurementRanges.RequireInRange(result, "dress_length", input.DressLength, MeasurementRanges.DressLength);
        MeasurementRanges.RequireInRange(result, "ease_bust", input.EaseBust, MeasurementRanges.EaseBust);
        MeasurementRanges.RequireInRange(result, "ease_waist", input.EaseWaist, MeasurementRanges.EaseWaist);
        MeasurementRanges.RequireInRange(result, "ease_hip", input.EaseHip, MeasurementRanges.EaseHip);
        MeasurementRanges.RequireInRange(result, "shoulder_to_bust", input.ShoulderToBust, MeasurementRanges.ShoulderToBust);
        MeasurementRanges.RequireInRange(result, "bust_to_waist", input.BustToWaist, MeasurementRanges.BustToWaist);
        MeasurementRanges.RequireInRange(result, "waist_to_hip", input.WaistToHip, MeasurementRanges.WaistToHip);
        MeasurementRanges.RequireInRange(result, "seam_allowance", input.SeamAllowance, MeasurementRanges.SeamAllowance);
        MeasurementRanges.RequireInRange(result, "hem_allowance", input.HemAllowance, MeasurementRanges.HemAllowance);

        if (!string.Equals(input.LengthFrom, "shoulder", StringComparison.OrdinalIgnoreCase))
        {
            result.AddError(
                "length_from_unsupported",
                "simple_dress.v1 only supports length_from=shoulder.");
        }

        var bustEff = input.BustCirc + input.EaseBust;
        var waistEff = input.WaistCirc + input.EaseWaist;
        var hipEff = input.HipCirc + input.EaseHip;

        if (bustEff < waistEff)
        {
            result.AddWarning(
                "bust_lt_waist_effective",
                $"bust_circ+ease_bust ({bustEff}) < waist_circ+ease_waist ({waistEff}); tight-bust silhouette.");
        }

        if (hipEff < waistEff)
        {
            result.AddError(
                "hip_lt_waist_effective",
                $"hip_circ+ease_hip ({hipEff}) must be ≥ waist_circ+ease_waist ({waistEff}).");
        }

        var minLength = input.ShoulderToBust + input.BustToWaist + input.WaistToHip + 10m;
        if (input.DressLength < minLength)
        {
            result.AddError(
                "dress_too_short",
                $"dress_length ({input.DressLength}) must be ≥ shoulder_to_bust+bust_to_waist+waist_to_hip+10 ({minLength}).");
        }

        return result;
    }
}
