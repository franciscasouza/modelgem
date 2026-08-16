namespace ModelaFlow.PatternCore.Pattern;

using ModelaFlow.PatternCore.Geometry;

/// <summary>
/// Grainline (fio) — MVP policy: parallel to longitudinal center.
/// </summary>
public sealed record Grainline(
    Point2D Start,
    Point2D End,
    string Policy = GrainlinePolicies.ParallelCenter)
{
    public Segment2D AsSegment() => new(Start, End);
}

public static class GrainlinePolicies
{
    public const string ParallelCenter = "parallel_center";
}
