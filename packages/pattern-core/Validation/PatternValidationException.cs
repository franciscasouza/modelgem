namespace ModelaFlow.PatternCore.Validation;

/// <summary>
/// Explicit validation failure — never silently compute invalid geometry.
/// </summary>
public sealed class PatternValidationException : Exception
{
    public PatternValidationException(string code, string message, IReadOnlyList<string>? details = null)
        : base(message)
    {
        Code = code;
        Details = details ?? Array.Empty<string>();
    }

    public string Code { get; }

    public IReadOnlyList<string> Details { get; }
}

public sealed record ValidationIssue(string Code, string Message, bool IsError);

public sealed class ValidationResult
{
    private readonly List<ValidationIssue> _issues = new();

    public IReadOnlyList<ValidationIssue> Issues => _issues;

    public bool HasErrors => _issues.Exists(i => i.IsError);

    public void AddError(string code, string message) =>
        _issues.Add(new ValidationIssue(code, message, IsError: true));

    public void AddWarning(string code, string message) =>
        _issues.Add(new ValidationIssue(code, message, IsError: false));

    public void ThrowIfInvalid()
    {
        if (!HasErrors)
            return;

        var details = _issues.Where(i => i.IsError).Select(i => $"{i.Code}: {i.Message}").ToList();
        throw new PatternValidationException(
            "validation_failed",
            "Measurement or parameter validation failed; geometry was not calculated.",
            details);
    }
}
