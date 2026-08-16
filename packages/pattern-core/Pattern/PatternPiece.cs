namespace ModelaFlow.PatternCore.Pattern;

using ModelaFlow.PatternCore.Geometry;

public enum PieceSide
{
    Front,
    Back
}

/// <summary>
/// One cuttable pattern part with stitch contour, cut contour, grainline, and notches.
/// </summary>
public sealed class PatternPiece
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required PieceSide Side { get; init; }
    public required int QuantityToCut { get; init; }
    public required bool OnFold { get; init; }
    public required Contour2D StitchContour { get; init; }
    public required Contour2D CutContour { get; init; }
    public required Grainline Grainline { get; init; }
    public required IReadOnlyList<Notch> Notches { get; init; }
    public required MarginSpec Margins { get; init; }
    public string? Notes { get; init; }
}
