using ModelaFlow.PatternCore.Bases;
using ModelaFlow.PatternCore.Pattern;
using ModelaFlow.PatternCore.Quality;
using ModelaFlow.PatternCore.Validation;

namespace ModelaFlow.PatternCore.Tests;

public class StraightSkirtPatternTests
{
    private static StraightSkirtInput Fixture() => new()
    {
        WaistCirc = 70m,
        HipCirc = 96m,
        SkirtLength = 60m,
        EaseWaist = 2m,
        EaseHip = 4m,
        WaistToHip = 20m,
        SeamAllowance = 1.0m,
        HemAllowance = 3.0m,
        WaistbandHeight = 0m,
        LengthIncludesHem = false
    };

    [Fact]
    public void Generate_IsDeterministic()
    {
        var input = Fixture();
        var a = StraightSkirtPattern.Generate(input);
        var b = StraightSkirtPattern.Generate(input);

        Assert.Equal(a.BaseId, b.BaseId);
        Assert.Equal(a.BaseVersion, b.BaseVersion);
        Assert.Equal(a.Pieces.Count, b.Pieces.Count);

        for (var i = 0; i < a.Pieces.Count; i++)
        {
            AssertPieceGeometryEqual(a.Pieces[i], b.Pieces[i]);
        }
    }

    [Fact]
    public void Generate_Regression_KeyWidthsAndLength()
    {
        var doc = StraightSkirtPattern.Generate(Fixture());
        Assert.Equal(2, doc.Pieces.Count);

        // waist_eff=72 → panel width = 36; hip_eff=100 → panel width = 50
        foreach (var piece in doc.Pieces)
        {
            Assert.Equal(36m, PatternQualityChecks.StitchWidthAtY(piece, 0m));
            Assert.Equal(50m, PatternQualityChecks.StitchWidthAtY(piece, 20m));
            Assert.Equal(50m, PatternQualityChecks.StitchWidthAtY(piece, 60m));
            Assert.Equal(60m, PatternQualityChecks.StitchLength(piece));
            Assert.Equal(1.0m, piece.Margins.SeamAllowanceCm);
            Assert.Equal(3.0m, piece.Margins.HemAllowanceCm);
            Assert.Equal(GrainlinePolicies.ParallelCenter, piece.Grainline.Policy);
        }

        Assert.Contains(doc.Pieces, p => p.Side == PieceSide.Front && p.Id == "skirt_front");
        Assert.Contains(doc.Pieces, p => p.Side == PieceSide.Back && p.Id == "skirt_back");
        Assert.Equal(0m, doc.ResolvedParametersCm["waistband_height"]);
    }

    [Fact]
    public void Generate_MeetsMinimumQualityCriteria()
    {
        var doc = StraightSkirtPattern.Generate(Fixture());
        var failures = PatternQualityChecks.Evaluate(doc);
        Assert.Empty(failures);
    }

    [Fact]
    public void Generate_RejectsOutOfRangeWaist()
    {
        var input = Fixture() with { WaistCirc = 40m };
        var ex = Assert.Throws<PatternValidationException>(() => StraightSkirtPattern.Generate(input));
        Assert.Equal("validation_failed", ex.Code);
        Assert.Contains(ex.Details, d => d.Contains("out_of_range", StringComparison.Ordinal));
    }

    [Fact]
    public void Generate_RejectsHipSmallerThanWaistEffective()
    {
        var input = Fixture() with { HipCirc = 70m, EaseHip = 0m, WaistCirc = 80m, EaseWaist = 2m };
        var ex = Assert.Throws<PatternValidationException>(() => StraightSkirtPattern.Generate(input));
        Assert.Contains(ex.Details, d => d.Contains("hip_lt_waist_effective", StringComparison.Ordinal));
    }

    [Fact]
    public void Generate_RejectsNonZeroWaistband()
    {
        var input = Fixture() with { WaistbandHeight = 3m };
        var ex = Assert.Throws<PatternValidationException>(() => StraightSkirtPattern.Generate(input));
        Assert.Contains(ex.Details, d => d.Contains("waistband_not_in_base_v1", StringComparison.Ordinal));
    }

    [Fact]
    public void Generate_RejectsTooShortSkirt()
    {
        var input = Fixture() with { SkirtLength = 25m, WaistToHip = 20m };
        // 25 is also out of skirt_length min 30 — use 30 with waist_to_hip 25 → need ≥ 33
        input = Fixture() with { SkirtLength = 30m, WaistToHip = 25m };
        var ex = Assert.Throws<PatternValidationException>(() => StraightSkirtPattern.Generate(input));
        Assert.Contains(ex.Details, d => d.Contains("skirt_too_short", StringComparison.Ordinal));
    }

    private static void AssertPieceGeometryEqual(PatternPiece a, PatternPiece b)
    {
        Assert.Equal(a.Id, b.Id);
        Assert.Equal(a.StitchContour.Edges.Count, b.StitchContour.Edges.Count);
        for (var i = 0; i < a.StitchContour.Edges.Count; i++)
        {
            var ea = a.StitchContour.Edges[i];
            var eb = b.StitchContour.Edges[i];
            Assert.Equal(ea.Kind, eb.Kind);
            Assert.Equal(ea.Start, eb.Start);
            Assert.Equal(ea.End, eb.End);
            if (ea.Curve is not null && eb.Curve is not null)
            {
                Assert.Equal(ea.Curve.P1, eb.Curve.P1);
                Assert.Equal(ea.Curve.P2, eb.Curve.P2);
            }
        }

        Assert.Equal(a.Grainline.Start, b.Grainline.Start);
        Assert.Equal(a.Grainline.End, b.Grainline.End);
        Assert.Equal(a.Notches.Count, b.Notches.Count);
        for (var i = 0; i < a.Notches.Count; i++)
            Assert.Equal(a.Notches[i].Position, b.Notches[i].Position);
    }
}
