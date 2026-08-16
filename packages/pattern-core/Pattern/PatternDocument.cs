namespace ModelaFlow.PatternCore.Pattern;

/// <summary>
/// Versioned geometric pattern document produced by a deterministic base.
/// </summary>
public sealed class PatternDocument
{
    public const string SchemaId = "pattern.v1";
    public const int SchemaVersion = 1;
    public const string Unit = "cm";

    public required string BaseId { get; init; }
    public required string BaseVersion { get; init; }
    public required IReadOnlyList<PatternPiece> Pieces { get; init; }
    public required IReadOnlyDictionary<string, decimal> ResolvedParametersCm { get; init; }
    public IReadOnlyList<string> Limitations { get; init; } = Array.Empty<string>();
}
