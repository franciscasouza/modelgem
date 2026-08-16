namespace ModelaFlow.PatternCore.Bases;

using ModelaFlow.PatternCore.Geometry;
using ModelaFlow.PatternCore.Pattern;

/// <summary>
/// Shared helpers for deterministic panel construction (cm).
/// Coordinate system: X=0 at center; +X toward right side seam; +Y toward hem.
/// </summary>
internal static class PanelBuilder
{
    /// <summary>
    /// Builds a full panel (left side → right side) with optional waist→hip shaping via cubic Bézier.
    /// Half-widths are from center to side seam (finished / stitch).
    /// </summary>
    public static PatternPiece BuildSymmetricPanel(
        string id,
        string name,
        PieceSide side,
        IReadOnlyList<(decimal Y, decimal HalfWidth)> stations,
        decimal seamAllowance,
        decimal hemAllowance,
        decimal finishedLength,
        bool lengthIncludesHem)
    {
        if (stations.Count < 2)
            throw new ArgumentException("At least two stations are required.", nameof(stations));

        var margins = new MarginSpec(seamAllowance, hemAllowance);
        var stitch = BuildStitchContour(stations);
        var cut = BuildCutContour(stations, seamAllowance, hemAllowance, lengthIncludesHem);
        var grain = new Grainline(
            new Point2D(0m, stations[0].Y + 2m),
            new Point2D(0m, finishedLength - 2m));

        var notches = BuildSideNotches(stations, side);

        return new PatternPiece
        {
            Id = id,
            Name = name,
            Side = side,
            QuantityToCut = 1,
            OnFold = false,
            StitchContour = stitch,
            CutContour = cut,
            Grainline = grain,
            Notches = notches,
            Margins = margins
        };
    }

    private static Contour2D BuildStitchContour(IReadOnlyList<(decimal Y, decimal HalfWidth)> stations)
    {
        var rightSide = BuildRightSide(stations, offsetX: 0m);
        var leftSide = BuildRightSide(stations, offsetX: 0m)
            .Select(p => new Point2D(-p.X, p.Y))
            .Reverse()
            .ToList();

        var topLeft = leftSide[^1];
        var topRight = rightSide[0];
        var bottomRight = rightSide[^1];
        var bottomLeft = leftSide[0];

        var edges = new List<PathEdge>
        {
            PathEdge.FromSegment(new Segment2D(topLeft, topRight), EdgeRole.Stitch, "waist_or_neck")
        };
        edges.AddRange(BuildSideEdges(rightSide, EdgeRole.Stitch, "side_right"));
        edges.Add(PathEdge.FromSegment(new Segment2D(bottomRight, bottomLeft), EdgeRole.Stitch, "hem"));
        edges.AddRange(BuildSideEdges(leftSide, EdgeRole.Stitch, "side_left"));

        return new Contour2D(edges, isClosed: true);
    }

    private static Contour2D BuildCutContour(
        IReadOnlyList<(decimal Y, decimal HalfWidth)> stations,
        decimal seam,
        decimal hem,
        bool lengthIncludesHem)
    {
        // Expand horizontally by seam; top by seam; bottom by hem (unless length already includes hem).
        var hemExtra = lengthIncludesHem ? 0m : hem;
        var expanded = stations
            .Select(s => (Y: s.Y, HalfWidth: s.HalfWidth + seam))
            .ToList();

        // Shift first station up by seam (negative Y) and last down by hemExtra.
        expanded[0] = (expanded[0].Y - seam, expanded[0].HalfWidth);
        var last = expanded.Count - 1;
        expanded[last] = (expanded[last].Y + hemExtra, expanded[last].HalfWidth);

        var rightSide = BuildRightSide(expanded, offsetX: 0m);
        var leftSide = BuildRightSide(expanded, offsetX: 0m)
            .Select(p => new Point2D(-p.X, p.Y))
            .Reverse()
            .ToList();

        var topLeft = leftSide[^1];
        var topRight = rightSide[0];
        var bottomRight = rightSide[^1];
        var bottomLeft = leftSide[0];

        var edges = new List<PathEdge>
        {
            PathEdge.FromSegment(new Segment2D(topLeft, topRight), EdgeRole.Cut, "cut_top")
        };
        edges.AddRange(BuildSideEdges(rightSide, EdgeRole.Cut, "cut_side_right"));
        edges.Add(PathEdge.FromSegment(new Segment2D(bottomRight, bottomLeft), EdgeRole.Cut, "cut_hem"));
        edges.AddRange(BuildSideEdges(leftSide, EdgeRole.Cut, "cut_side_left"));

        return new Contour2D(edges, isClosed: true);
    }

    private static List<Point2D> BuildRightSide(
        IReadOnlyList<(decimal Y, decimal HalfWidth)> stations,
        decimal offsetX)
    {
        return stations.Select(s => new Point2D(s.HalfWidth + offsetX, s.Y)).ToList();
    }

    private static IEnumerable<PathEdge> BuildSideEdges(
        IReadOnlyList<Point2D> points,
        EdgeRole role,
        string labelPrefix)
    {
        for (var i = 0; i < points.Count - 1; i++)
        {
            var a = points[i];
            var b = points[i + 1];
            // Slight shaping: cubic with controls at 1/3 and 2/3 along the chord, X nudged toward max width.
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            var c1 = new Point2D(a.X + dx / 3m, a.Y + dy / 3m);
            var c2 = new Point2D(a.X + 2m * dx / 3m, a.Y + 2m * dy / 3m);
            if (dx != 0m)
            {
                // Keep controls on the chord for deterministic mild curve (still Bézier, length ≈ segment).
                yield return PathEdge.FromCurve(
                    new CubicBezier2D(a, c1, c2, b),
                    role,
                    $"{labelPrefix}_{i}");
            }
            else
            {
                yield return PathEdge.FromSegment(new Segment2D(a, b), role, $"{labelPrefix}_{i}");
            }
        }
    }

    private static IReadOnlyList<Notch> BuildSideNotches(
        IReadOnlyList<(decimal Y, decimal HalfWidth)> stations,
        PieceSide side)
    {
        var notches = new List<Notch>();
        var prefix = side == PieceSide.Front ? "front" : "back";
        for (var i = 0; i < stations.Count; i++)
        {
            var (y, half) = stations[i];
            var pairKey = $"station_{i}";
            notches.Add(new Notch(
                $"{prefix}_right_{pairKey}",
                new Point2D(half, y),
                NotchKind.SideSeam,
                pairKey));
            notches.Add(new Notch(
                $"{prefix}_left_{pairKey}",
                new Point2D(-half, y),
                NotchKind.SideSeam,
                pairKey));
        }

        notches.Add(new Notch(
            $"{prefix}_center_top",
            new Point2D(0m, stations[0].Y),
            NotchKind.Center,
            "center"));

        return notches;
    }

    /// <summary>
    /// Half-width of one panel from center to side for a full circumference (2 panels = front+back).
    /// Panel covers half body → halfWidth = circumference / 4.
    /// </summary>
    public static decimal HalfWidthFromCircumference(decimal fullCircumferenceCm) =>
        fullCircumferenceCm / 4m;
}
