namespace ModelaFlow.PatternCore.Diagnostics;

/// <summary>
/// Basic observability for calculation failures (no sensitive raw dumps required).
/// Callers may attach this to structured logging.
/// </summary>
public sealed record PatternCalculationFailure(
    string BaseId,
    string Code,
    string Message,
    IReadOnlyList<string> Details,
    DateTimeOffset OccurredAtUtc)
{
    public static PatternCalculationFailure FromException(
        string baseId,
        Exception exception,
        DateTimeOffset? occurredAtUtc = null)
    {
        if (exception is Validation.PatternValidationException pve)
        {
            return new PatternCalculationFailure(
                baseId,
                pve.Code,
                pve.Message,
                pve.Details,
                occurredAtUtc ?? DateTimeOffset.UtcNow);
        }

        return new PatternCalculationFailure(
            baseId,
            "calculation_error",
            exception.Message,
            Array.Empty<string>(),
            occurredAtUtc ?? DateTimeOffset.UtcNow);
    }
}
