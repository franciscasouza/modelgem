using ModelaFlow.PatternCore.Bases;
using ModelaFlow.PatternCore.Pattern;
using ModelaFlow.PatternCore.Quality;
using ModelaFlow.PatternCore.Validation;

namespace ModelaFlow.PatternCore.Tests;

public class SimpleDressPatternTests
{
    private static SimpleDressInput Fixture() => new()
    {
        BustCirc = 90m,
        WaistCirc = 72m,
        HipCirc = 98m,
        DressLength = 110m,
        EaseBust = 4m,
        EaseWaist = 2m,
        EaseHip = 4m,
        ShoulderToBust = 26m,
        BustToWaist = 20m,
        WaistToHip = 20m,
        SeamAllowance = 1.0m,
        HemAllowance = 3.0m,
        LengthIncludesHem = false,
        LengthFrom = "shoulder"
    };

    [Fact]
    public void Generate_IsDeterministic()
    {
        var input = Fixture();
        var a = SimpleDressPattern.Generate(input);
        var b = SimpleDressPattern.Generate(input);

        Assert.Equal(a.Pieces.Count, b.Pieces.Count);
        for (var i = 0; i < a.Pieces.Count; i++)
        {
            var pa = a.Pieces[i];
            var pb = b.Pieces[i];
            Assert.Equal(pa.StitchContour.Edges.Count, pb.StitchContour.Edges.Count);
            for (var e = 0; e < pa.StitchContour.Edges.Count; e++)
            {
                Assert.Equal(pa.StitchContour.Edges[e].Start, pb.StitchContour.Edges[e].Start);
                Assert.Equal(pa.StitchContour.Edges[e].End, pb.StitchContour.Edges[e].End);
            }
        }
    }

    [Fact]
    public void Generate_Regression_KeyStations()
    {
        var doc = SimpleDressPattern.Generate(Fixture());
        // bust_eff=94 → width 47; waist_eff=74 → 37; hip_eff=102 → 51
        foreach (var piece in doc.Pieces)
        {
            Assert.Equal(47m, PatternQualityChecks.StitchWidthAtY(piece, 0m));
            Assert.Equal(47m, PatternQualityChecks.StitchWidthAtY(piece, 26m));
            Assert.Equal(37m, PatternQualityChecks.StitchWidthAtY(piece, 46m)); // 26+20
            Assert.Equal(51m, PatternQualityChecks.StitchWidthAtY(piece, 66m)); // 46+20
            Assert.Equal(51m, PatternQualityChecks.StitchWidthAtY(piece, 110m));
            Assert.Equal(110m, PatternQualityChecks.StitchLength(piece));
        }
    }

    [Fact]
    public void Generate_MeetsMinimumQualityCriteria()
    {
        var failures = PatternQualityChecks.Evaluate(SimpleDressPattern.Generate(Fixture()));
        Assert.Empty(failures);
    }

    [Fact]
    public void Generate_RejectsOutOfRangeBust()
    {
        var ex = Assert.Throws<PatternValidationException>(
            () => SimpleDressPattern.Generate(Fixture() with { BustCirc = 50m }));
        Assert.Contains(ex.Details, d => d.Contains("out_of_range", StringComparison.Ordinal));
    }

    [Fact]
    public void Generate_RejectsDressTooShort()
    {
        // min = 26+20+20+10 = 76; use 75 which is in dress_length range
        var ex = Assert.Throws<PatternValidationException>(
            () => SimpleDressPattern.Generate(Fixture() with { DressLength = 75m }));
        Assert.Contains(ex.Details, d => d.Contains("dress_too_short", StringComparison.Ordinal));
    }

    [Fact]
    public void Generate_RejectsUnsupportedLengthFrom()
    {
        var ex = Assert.Throws<PatternValidationException>(
            () => SimpleDressPattern.Generate(Fixture() with { LengthFrom = "waist" }));
        Assert.Contains(ex.Details, d => d.Contains("length_from_unsupported", StringComparison.Ordinal));
    }
}
