using ModelaFlow.PatternCore.Geometry;
using ModelaFlow.PatternCore.Measurements;

namespace ModelaFlow.PatternCore.Tests;

public class GeometryAndSchemaTests
{
    [Fact]
    public void MeasurementSchema_IsV1()
    {
        Assert.Equal("measurements.v1", MeasurementSchema.SchemaId);
        Assert.Equal(1, MeasurementSchema.SchemaVersion);
        Assert.Equal("cm", MeasurementSchema.Unit);
    }

    [Fact]
    public void PatternDocument_Schema_IsV1()
    {
        Assert.Equal("pattern.v1", Pattern.PatternDocument.SchemaId);
        Assert.Equal(1, Pattern.PatternDocument.SchemaVersion);
        Assert.Equal("cm", Pattern.PatternDocument.Unit);
    }

    [Fact]
    public void CubicBezier_Evaluate_IsDeterministic()
    {
        var curve = new CubicBezier2D(
            new Point2D(0m, 0m),
            new Point2D(1m, 0m),
            new Point2D(2m, 0m),
            new Point2D(3m, 0m));

        var a = curve.Evaluate(0.5m);
        var b = curve.Evaluate(0.5m);
        Assert.Equal(a, b);
        Assert.Equal(1.5m, a.X);
        Assert.Equal(0m, a.Y);
    }

    [Fact]
    public void Point2D_Distance_UsesDecimal()
    {
        var d = new Point2D(0m, 0m).DistanceTo(new Point2D(3m, 4m));
        Assert.Equal(5m, d);
    }
}
