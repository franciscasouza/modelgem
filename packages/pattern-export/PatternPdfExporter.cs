using ModelaFlow.PatternCore.Geometry;
using ModelaFlow.PatternCore.Pattern;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ModelaFlow.PatternExport;

/// <summary>
/// Renders a given <see cref="PatternDocument"/> to A4 PDF bytes.
/// Does not recalculate pattern geometry — draws only what is provided.
/// Domain unit is cm; conversion to PDF points happens only at this boundary.
/// Library: QuestPDF (Community) — see ADR-0003.
/// </summary>
public static class PatternPdfExporter
{
    /// <summary>1 cm in PDF points at 72 dpi (1 in = 2.54 cm).</summary>
    public const float PointsPerCm = 72f / 2.54f;

    public const string ScaleLabel = "escala 100% / 1:1";
    public const float ScaleRulerCm = 10f;

    static PatternPdfExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] ExportA4(PatternDocument document, string? title = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text(title ?? $"ModelaFlow — {document.BaseId} {document.BaseVersion}")
                        .SemiBold().FontSize(12);
                    col.Item().Text(ScaleLabel).FontSize(10);
                    col.Item().Text($"Régua de escala: {ScaleRulerCm:0} cm (marcação abaixo)").FontSize(8)
                        .FontColor(Colors.Grey.Darken2);
                    col.Item().Height(22).Svg(size => BuildScaleRulerSvg(size.Width, size.Height));
                    col.Item().PaddingTop(2).Text(
                            $"unidade domínio: {PatternDocument.Unit} · peças: {document.Pieces.Count} · {ScaleLabel}")
                        .FontSize(8).FontColor(Colors.Grey.Darken2);
                });

                page.Content().PaddingTop(8).Column(col =>
                {
                    foreach (var piece in document.Pieces)
                    {
                        var bbox = ComputeBoundingBox(piece);
                        var svg = BuildPieceSvg(piece, bbox, maxWidthPt: PointsPerCm * 17f, maxHeightPt: PointsPerCm * 11f);

                        col.Item().PaddingBottom(10).Border(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(6).Column(pieceCol =>
                            {
                                pieceCol.Item().Text(
                                        $"{piece.Name} · {piece.Side} · cortar {piece.QuantityToCut}" +
                                        (piece.OnFold ? " · na dobra" : string.Empty))
                                    .SemiBold();
                                pieceCol.Item().Text(
                                        $"Margens: costura {piece.Margins.SeamAllowanceCm} cm · barra {piece.Margins.HemAllowanceCm} cm · piques: {piece.Notches.Count}")
                                    .FontSize(8).FontColor(Colors.Grey.Darken1);
                                pieceCol.Item().Height(svg.Height).Svg(_ => svg.Content);
                            });
                    }

                    if (document.Limitations.Count > 0)
                    {
                        col.Item().PaddingTop(8).Text("Limitações da base:").SemiBold().FontSize(8);
                        foreach (var limitation in document.Limitations)
                            col.Item().Text("• " + limitation).FontSize(7).FontColor(Colors.Grey.Darken1);
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("ModelaFlow export · A4 · ").FontSize(7).FontColor(Colors.Grey.Medium);
                    text.Span(ScaleLabel).FontSize(7).FontColor(Colors.Grey.Medium);
                    text.Span(" · página ").FontSize(7).FontColor(Colors.Grey.Medium);
                    text.CurrentPageNumber().FontSize(7).FontColor(Colors.Grey.Medium);
                });
            });
        });

        return pdf
            .WithMetadata(new DocumentMetadata
            {
                Title = title ?? $"ModelaFlow — {document.BaseId}",
                Subject = ScaleLabel,
                Keywords = $"ModelaFlow,PDF A4,{ScaleLabel},regua {ScaleRulerCm:0} cm",
                Creator = "ModelaFlow.PatternExport",
                Producer = "ModelaFlow / QuestPDF"
            })
            .GeneratePdf();
    }

    private static string BuildScaleRulerSvg(float width, float height)
    {
        var rulerLength = Math.Min(ScaleRulerCm * PointsPerCm, width - 40f);
        var y = height * 0.55f;
        var ticks = new System.Text.StringBuilder();
        for (var cm = 0; cm <= ScaleRulerCm; cm++)
        {
            var x = cm * PointsPerCm;
            if (x > rulerLength) break;
            var tick = cm == 0 || cm == ScaleRulerCm || cm % 5 == 0 ? 6f : 3f;
            ticks.Append($"<line x1=\"{x:0.##}\" y1=\"{y - tick:0.##}\" x2=\"{x:0.##}\" y2=\"{y + tick:0.##}\" stroke=\"black\" stroke-width=\"1\"/>");
        }

        return $"""
            <svg width="{width:0.##}" height="{height:0.##}" xmlns="http://www.w3.org/2000/svg">
              <line x1="0" y1="{y:0.##}" x2="{rulerLength:0.##}" y2="{y:0.##}" stroke="black" stroke-width="1.5"/>
              {ticks}
              <text x="{rulerLength + 6:0.##}" y="{y + 4:0.##}" font-size="9" fill="black">{ScaleRulerCm:0} cm</text>
            </svg>
            """;
    }

    private static (string Content, float Height) BuildPieceSvg(
        PatternPiece piece,
        BoundingBoxCm bbox,
        float maxWidthPt,
        float maxHeightPt)
    {
        var marginPt = PointsPerCm * 0.5f;
        var naturalW = Math.Max(0.1f, bbox.WidthCm) * PointsPerCm;
        var naturalH = Math.Max(0.1f, bbox.HeightCm) * PointsPerCm;
        var fit = Math.Min(1f, Math.Min(maxWidthPt / naturalW, maxHeightPt / naturalH));
        var width = naturalW * fit + marginPt * 2;
        var height = naturalH * fit + marginPt * 2 + 14f;

        string MapX(decimal xCm) =>
            (marginPt + (float)((double)(xCm - bbox.MinX) * PointsPerCm * fit)).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

        string MapY(decimal yCm) =>
            (marginPt + (float)((double)(yCm - bbox.MinY) * PointsPerCm * fit)).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

        var sb = new System.Text.StringBuilder();
        sb.Append(CultureInvariant($"<svg width=\"{width:0.##}\" height=\"{height:0.##}\" xmlns=\"http://www.w3.org/2000/svg\">"));

        AppendContour(sb, piece.CutContour, MapX, MapY, "#666666", 0.9f, dash: "3 2");
        AppendContour(sb, piece.StitchContour, MapX, MapY, "#000000", 1.2f, dash: null);

        var g0 = piece.Grainline.Start;
        var g1 = piece.Grainline.End;
        sb.Append(CultureInvariant(
            $"<line x1=\"{MapX(g0.X)}\" y1=\"{MapY(g0.Y)}\" x2=\"{MapX(g1.X)}\" y2=\"{MapY(g1.Y)}\" stroke=\"#1565C0\" stroke-width=\"1.2\"/>"));
        sb.Append(CultureInvariant(
            $"<text x=\"{(float.Parse(MapX(g1.X), System.Globalization.CultureInfo.InvariantCulture) + 4).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}\" y=\"{MapY(g1.Y)}\" font-size=\"8\" fill=\"#1565C0\">fio</text>"));

        foreach (var notch in piece.Notches)
        {
            var nx = MapX(notch.Position.X);
            var ny = MapY(notch.Position.Y);
            var nxf = float.Parse(nx, System.Globalization.CultureInfo.InvariantCulture);
            var nyf = float.Parse(ny, System.Globalization.CultureInfo.InvariantCulture);
            sb.Append(CultureInvariant(
                $"<line x1=\"{nxf - 3:0.##}\" y1=\"{nyf:0.##}\" x2=\"{nxf + 3:0.##}\" y2=\"{nyf:0.##}\" stroke=\"#C62828\" stroke-width=\"1\"/>"));
            sb.Append(CultureInvariant(
                $"<line x1=\"{nxf:0.##}\" y1=\"{nyf - 3:0.##}\" x2=\"{nxf:0.##}\" y2=\"{nyf + 3:0.##}\" stroke=\"#C62828\" stroke-width=\"1\"/>"));
        }

        if (fit < 0.999f)
        {
            sb.Append(CultureInvariant(
                $"<text x=\"4\" y=\"{height - 4:0.##}\" font-size=\"7\" fill=\"#666\">pré-visualização {(fit * 100f):0}% — régua do cabeçalho = 10 cm reais · {ScaleLabel}</text>"));
        }

        sb.Append("</svg>");
        return (sb.ToString(), height);
    }

    private static void AppendContour(
        System.Text.StringBuilder sb,
        Contour2D contour,
        Func<decimal, string> mapX,
        Func<decimal, string> mapY,
        string color,
        float strokeWidth,
        string? dash)
    {
        var dashAttr = dash is null ? string.Empty : $" stroke-dasharray=\"{dash}\"";
        foreach (var edge in contour.Edges)
        {
            switch (edge.Kind)
            {
                case EdgeKind.Segment when edge.Segment is { } seg:
                    sb.Append(CultureInvariant(
                        $"<line x1=\"{mapX(seg.Start.X)}\" y1=\"{mapY(seg.Start.Y)}\" x2=\"{mapX(seg.End.X)}\" y2=\"{mapY(seg.End.Y)}\" stroke=\"{color}\" stroke-width=\"{strokeWidth:0.##}\"{dashAttr}/>"));
                    break;
                case EdgeKind.CubicBezier when edge.Curve is { } curve:
                    var d = new System.Text.StringBuilder();
                    d.Append(CultureInvariant($"M {mapX(curve.P0.X)} {mapY(curve.P0.Y)}"));
                    const int samples = 12;
                    for (var i = 1; i <= samples; i++)
                    {
                        var t = i / (decimal)samples;
                        var p = curve.Evaluate(t);
                        d.Append(CultureInvariant($" L {mapX(p.X)} {mapY(p.Y)}"));
                    }

                    sb.Append(CultureInvariant(
                        $"<path d=\"{d}\" fill=\"none\" stroke=\"{color}\" stroke-width=\"{strokeWidth:0.##}\"{dashAttr}/>"));
                    break;
            }
        }
    }

    private static string CultureInvariant(FormattableString value) =>
        value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static BoundingBoxCm ComputeBoundingBox(PatternPiece piece)
    {
        var points = new List<Point2D>();
        foreach (var edge in piece.CutContour.Edges)
        {
            points.Add(edge.Start);
            points.Add(edge.End);
            if (edge.Curve is { } c)
            {
                points.Add(c.P1);
                points.Add(c.P2);
            }
        }

        points.Add(piece.Grainline.Start);
        points.Add(piece.Grainline.End);
        foreach (var n in piece.Notches)
            points.Add(n.Position);

        var minX = points.Min(p => p.X);
        var maxX = points.Max(p => p.X);
        var minY = points.Min(p => p.Y);
        var maxY = points.Max(p => p.Y);
        return new BoundingBoxCm(minX, minY, maxX, maxY);
    }

    private readonly record struct BoundingBoxCm(decimal MinX, decimal MinY, decimal MaxX, decimal MaxY)
    {
        public float WidthCm => (float)(MaxX - MinX);
        public float HeightCm => (float)(MaxY - MinY);
    }
}
