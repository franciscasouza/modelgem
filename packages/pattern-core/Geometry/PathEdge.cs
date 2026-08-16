namespace ModelaFlow.PatternCore.Geometry;

public enum EdgeRole
{
    Stitch,
    Cut,
    Construction
}

public enum EdgeKind
{
    Segment,
    CubicBezier
}

/// <summary>
/// Single edge of a pattern contour. Exactly one of Segment/Curve is set based on Kind.
/// </summary>
public sealed record PathEdge
{
    public required EdgeKind Kind { get; init; }
    public required EdgeRole Role { get; init; }
    public Segment2D? Segment { get; init; }
    public CubicBezier2D? Curve { get; init; }
    public string? Label { get; init; }

    public static PathEdge FromSegment(Segment2D segment, EdgeRole role, string? label = null) =>
        new()
        {
            Kind = EdgeKind.Segment,
            Role = role,
            Segment = segment,
            Label = label
        };

    public static PathEdge FromCurve(CubicBezier2D curve, EdgeRole role, string? label = null) =>
        new()
        {
            Kind = EdgeKind.CubicBezier,
            Role = role,
            Curve = curve,
            Label = label
        };

    public Point2D Start => Kind switch
    {
        EdgeKind.Segment => Segment!.Start,
        EdgeKind.CubicBezier => Curve!.P0,
        _ => throw new InvalidOperationException($"Unknown edge kind: {Kind}")
    };

    public Point2D End => Kind switch
    {
        EdgeKind.Segment => Segment!.End,
        EdgeKind.CubicBezier => Curve!.P3,
        _ => throw new InvalidOperationException($"Unknown edge kind: {Kind}")
    };
}
