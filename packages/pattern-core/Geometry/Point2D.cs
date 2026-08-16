namespace ModelaFlow.PatternCore.Geometry;

/// <summary>
/// 2D point in centimeters. Domain unit is always cm.
/// </summary>
public readonly record struct Point2D(decimal X, decimal Y)
{
    public static Point2D Origin { get; } = new(0m, 0m);

    public Point2D Translate(decimal dx, decimal dy) => new(X + dx, Y + dy);

    public decimal DistanceTo(Point2D other)
    {
        var dx = other.X - X;
        var dy = other.Y - Y;
        return Sqrt(dx * dx + dy * dy);
    }

    public override string ToString() => $"({X}, {Y}) cm";

    private static decimal Sqrt(decimal value)
    {
        if (value < 0m)
            throw new ArgumentOutOfRangeException(nameof(value));
        if (value == 0m)
            return 0m;

        // Newton-Raphson on decimal for determinism (no double).
        decimal x = value;
        for (var i = 0; i < 32; i++)
        {
            var next = (x + value / x) / 2m;
            if (next == x)
                break;
            x = next;
        }

        return x;
    }
}
