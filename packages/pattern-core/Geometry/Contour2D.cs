namespace ModelaFlow.PatternCore.Geometry;

/// <summary>
/// Ordered closed or open contour of edges (cm).
/// </summary>
public sealed class Contour2D
{
    private readonly List<PathEdge> _edges;

    public Contour2D(IEnumerable<PathEdge> edges, bool isClosed)
    {
        ArgumentNullException.ThrowIfNull(edges);
        _edges = edges.ToList();
        if (_edges.Count == 0)
            throw new ArgumentException("Contour requires at least one edge.", nameof(edges));
        IsClosed = isClosed;
    }

    public bool IsClosed { get; }

    public IReadOnlyList<PathEdge> Edges => _edges;

    public IReadOnlyList<Point2D> Vertices
    {
        get
        {
            var points = new List<Point2D>(_edges.Count + 1) { _edges[0].Start };
            foreach (var edge in _edges)
                points.Add(edge.End);
            return points;
        }
    }
}
