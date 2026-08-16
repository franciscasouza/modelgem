namespace ModelaFlow.PatternCore.Geometry;

/// <summary>
/// Cubic Bézier curve in centimeters. Evaluated with deterministic decimal arithmetic.
/// </summary>
public sealed record CubicBezier2D(Point2D P0, Point2D P1, Point2D P2, Point2D P3)
{
    public Point2D Evaluate(decimal t)
    {
        if (t < 0m || t > 1m)
            throw new ArgumentOutOfRangeException(nameof(t), "t must be in [0, 1].");

        var u = 1m - t;
        var uu = u * u;
        var tt = t * t;
        var uuu = uu * u;
        var ttt = tt * t;

        var x = uuu * P0.X + 3m * uu * t * P1.X + 3m * u * tt * P2.X + ttt * P3.X;
        var y = uuu * P0.Y + 3m * uu * t * P1.Y + 3m * u * tt * P2.Y + ttt * P3.Y;
        return new Point2D(x, y);
    }

    /// <summary>
    /// Approximate length via fixed sample count (deterministic).
    /// </summary>
    public decimal ApproximateLength(int samples = 16)
    {
        if (samples < 1)
            throw new ArgumentOutOfRangeException(nameof(samples));

        var length = 0m;
        var prev = P0;
        for (var i = 1; i <= samples; i++)
        {
            var t = (decimal)i / samples;
            var curr = Evaluate(t);
            length += prev.DistanceTo(curr);
            prev = curr;
        }

        return length;
    }
}
