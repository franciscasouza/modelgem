namespace ModelaFlow.PatternCore.Quality;

using ModelaFlow.PatternCore.Pattern;

/// <summary>
/// Testable subset of docs/discovery/quality-criteria.md §1 for pattern-core.
/// </summary>
public static class PatternQualityChecks
{
    public static IReadOnlyList<string> Evaluate(PatternDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var failures = new List<string>();

        if (document.Pieces.Count == 0)
            failures.Add("no_pieces");

        foreach (var piece in document.Pieces)
        {
            if (piece.Grainline.Policy != GrainlinePolicies.ParallelCenter)
                failures.Add($"{piece.Id}:grainline_policy");
            else if (piece.Grainline.Start.X != 0m || piece.Grainline.End.X != 0m)
                failures.Add($"{piece.Id}:grainline_not_parallel_center");

            if (piece.Margins.SeamAllowanceCm <= 0m)
                failures.Add($"{piece.Id}:missing_seam_allowance");
            if (piece.Margins.HemAllowanceCm <= 0m)
                failures.Add($"{piece.Id}:missing_hem_allowance");

            if (piece.StitchContour.Edges.Count == 0)
                failures.Add($"{piece.Id}:empty_stitch");
            if (piece.CutContour.Edges.Count == 0)
                failures.Add($"{piece.Id}:empty_cut");

            if (piece.Notches.Count == 0)
                failures.Add($"{piece.Id}:missing_notches");

            if (string.IsNullOrWhiteSpace(piece.Name))
                failures.Add($"{piece.Id}:missing_name");
            if (piece.QuantityToCut < 1)
                failures.Add($"{piece.Id}:invalid_quantity");
        }

        var hasFront = document.Pieces.Any(p => p.Side == PieceSide.Front);
        var hasBack = document.Pieces.Any(p => p.Side == PieceSide.Back);
        if (!hasFront || !hasBack)
            failures.Add("missing_front_or_back");

        return failures;
    }

    /// <summary>
    /// Horizontal width of stitch contour at a given Y (max X − min X).
    /// </summary>
    public static decimal StitchWidthAtY(PatternPiece piece, decimal y, decimal yTolerance = 0.01m)
    {
        var xs = piece.StitchContour.Vertices
            .Where(p => DecimalAbs(p.Y - y) <= yTolerance)
            .Select(p => p.X)
            .ToList();

        if (xs.Count < 2)
            throw new InvalidOperationException($"No stitch vertices at y={y} for piece {piece.Id}.");

        return xs.Max() - xs.Min();
    }

    /// <summary>
    /// Finished length along center (min Y to max Y of stitch vertices).
    /// </summary>
    public static decimal StitchLength(PatternPiece piece)
    {
        var ys = piece.StitchContour.Vertices.Select(p => p.Y).ToList();
        return ys.Max() - ys.Min();
    }

    private static decimal DecimalAbs(decimal value) => value < 0m ? -value : value;
}
