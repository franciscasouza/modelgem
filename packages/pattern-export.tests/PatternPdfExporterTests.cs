using ModelaFlow.PatternCore.Bases;
using ModelaFlow.PatternExport;

namespace ModelaFlow.PatternExport.Tests;

public class PatternPdfExporterTests
{
    [Fact]
    public void ExportA4_StraightSkirt_ProducesNonEmptyA4PdfWithScaleMarker()
    {
        var document = StraightSkirtPattern.Generate(new StraightSkirtInput
        {
            WaistCirc = 70m,
            HipCirc = 96m,
            SkirtLength = 60m
        });

        var bytes = PatternPdfExporter.ExportA4(document, "Teste saia reta");

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 500, $"PDF too small: {bytes.Length} bytes");
        Assert.Equal((byte)'%', bytes[0]);
        Assert.Equal((byte)'P', bytes[1]);
        Assert.Equal((byte)'D', bytes[2]);
        Assert.Equal((byte)'F', bytes[3]);

        // Content streams are compressed; scale intent is also written to PDF metadata (uncompressed).
        var ascii = System.Text.Encoding.ASCII.GetString(bytes);
        Assert.Contains(PatternPdfExporter.ScaleLabel, ascii, StringComparison.Ordinal);
        Assert.Contains("10 cm", ascii, StringComparison.Ordinal);

        // A4 media box in points (~595.28 x 841.89)
        Assert.Contains("/MediaBox", ascii, StringComparison.Ordinal);
        Assert.True(
            ascii.Contains("595", StringComparison.Ordinal) || ascii.Contains("841", StringComparison.Ordinal),
            "Expected A4 page dimensions in PDF.");
    }
}
