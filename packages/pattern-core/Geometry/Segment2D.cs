namespace ModelaFlow.PatternCore.Geometry;

/// <summary>
/// Straight segment between two points (cm).
/// </summary>
public sealed record Segment2D(Point2D Start, Point2D End)
{
    public decimal Length => Start.DistanceTo(End);

    public Point2D Midpoint => new((Start.X + End.X) / 2m, (Start.Y + End.Y) / 2m);
}
