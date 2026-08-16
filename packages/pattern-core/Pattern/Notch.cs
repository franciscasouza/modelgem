namespace ModelaFlow.PatternCore.Pattern;

using ModelaFlow.PatternCore.Geometry;

public enum NotchKind
{
    SideSeam,
    Center,
    Construction
}

/// <summary>
/// Notch / pique marker on a piece (cm).
/// </summary>
public sealed record Notch(
    string Id,
    Point2D Position,
    NotchKind Kind,
    string? PairKey = null,
    string? Label = null);
