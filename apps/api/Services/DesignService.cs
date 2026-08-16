using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelaFlow.Api.Data;
using ModelaFlow.Api.Domain.Audit;
using ModelaFlow.Api.Domain.Design;
using ModelaFlow.PatternCore.Bases;
using ModelaFlow.PatternCore.Measurements;
using ModelaFlow.PatternCore.Pattern;
using ModelaFlow.PatternCore.Quality;
using ModelaFlow.PatternCore.Serialization;
using ModelaFlow.PatternCore.Validation;
using ModelaFlow.PatternExport;

namespace ModelaFlow.Api.Services;

public sealed class DesignService
{
    public static readonly Guid DevTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly JsonSerializerOptions ApiJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly ModelaFlowDbContext _db;
    private readonly ILogger<DesignService> _logger;

    public DesignService(ModelaFlowDbContext db, ILogger<DesignService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(Guid TenantId, Guid OrganizationId)> BootstrapDevTenantAsync(CancellationToken ct = default)
    {
        var existing = await _db.Organizations.AsNoTracking()
            .FirstOrDefaultAsync(o => o.TenantId == DevTenantId, ct);
        if (existing is not null)
            return (existing.TenantId, existing.Id);

        var any = await _db.Organizations.AsNoTracking().OrderBy(o => o.CreatedAt).FirstOrDefaultAsync(ct);
        if (any is not null)
            return (any.TenantId, any.Id);

        var org = new Domain.Identity.Organization
        {
            Id = DevTenantId,
            TenantId = DevTenantId,
            Name = "Atelier Demo (Dev)"
        };
        _db.Organizations.Add(org);

        var user = new Domain.Identity.User
        {
            TenantId = DevTenantId,
            OrganizationId = DevTenantId,
            Email = "demo@modelaflow.local",
            DisplayName = "Demo Owner",
            Role = Domain.Identity.UserRole.Owner
        };
        _db.Users.Add(user);

        await AddAuditAsync(DevTenantId, user.Id, "dev.bootstrap", nameof(Domain.Identity.Organization), org.Id,
            "{\"source\":\"dev_bootstrap\"}", ct);
        await _db.SaveChangesAsync(ct);
        return (org.TenantId, org.Id);
    }

    public async Task EnsureDevTenantSeededAsync(CancellationToken ct = default)
    {
        var exists = await _db.Organizations.AnyAsync(o => o.TenantId == DevTenantId, ct);
        if (exists) return;
        await BootstrapDevTenantAsync(ct);
    }

    public async Task<(int CustomerCount, int PatternCount)> GetOverviewAsync(Guid tenantId, CancellationToken ct = default)
    {
        await EnsureOrganizationAsync(tenantId, ct);
        var customers = await _db.Customers.CountAsync(c => c.TenantId == tenantId, ct);
        var patterns = await _db.PatternModels.CountAsync(p => p.TenantId == tenantId, ct);
        return (customers, patterns);
    }

    public async Task<IReadOnlyList<PatternModel>> ListPatternsAsync(Guid tenantId, int take = 20, CancellationToken ct = default)
    {
        await EnsureOrganizationAsync(tenantId, ct);
        return await _db.PatternModels.AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.UpdatedAt)
            .Take(Math.Clamp(take, 1, 100))
            .ToListAsync(ct);
    }

    public async Task<PatternModel> CreatePatternAsync(
        Guid tenantId,
        string name,
        PatternBaseKind baseKind,
        Guid? customerId,
        Guid? actorUserId,
        CancellationToken ct = default)
    {
        await EnsureOrganizationAsync(tenantId, ct);
        if (customerId is { } cid)
        {
            var customerOk = await _db.Customers.AnyAsync(c => c.TenantId == tenantId && c.Id == cid, ct);
            if (!customerOk)
                throw new InvalidOperationException("Customer not found for tenant.");
        }

        var now = DateTimeOffset.UtcNow;
        var model = new PatternModel
        {
            TenantId = tenantId,
            Name = name.Trim(),
            ReferenceCode = $"MF-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
            BaseKind = baseKind,
            CustomerId = customerId,
            Status = PatternModelStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.PatternModels.Add(model);

        _db.TechnicalSheets.Add(new TechnicalSheet
        {
            TenantId = tenantId,
            PatternModelId = model.Id,
            UpdatedAt = now
        });

        await AddAuditAsync(tenantId, actorUserId, "pattern.created", nameof(PatternModel), model.Id,
            $"{{\"baseKind\":\"{ToBaseKindString(baseKind)}\"}}", ct);
        await _db.SaveChangesAsync(ct);
        return model;
    }

    public async Task<PatternModel?> GetPatternAsync(Guid tenantId, Guid patternId, CancellationToken ct = default) =>
        await _db.PatternModels.AsNoTracking()
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == patternId, ct);

    public async Task<PatternVersion?> GetLatestVersionAsync(Guid tenantId, Guid patternId, CancellationToken ct = default) =>
        await _db.PatternVersions.AsNoTracking()
            .Where(v => v.TenantId == tenantId && v.PatternModelId == patternId)
            .OrderByDescending(v => v.Version)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<PatternVersion>> ListVersionsAsync(Guid tenantId, Guid patternId, CancellationToken ct = default) =>
        await _db.PatternVersions.AsNoTracking()
            .Where(v => v.TenantId == tenantId && v.PatternModelId == patternId)
            .OrderByDescending(v => v.Version)
            .ToListAsync(ct);

    public async Task<PatternVersion?> GetVersionAsync(
        Guid tenantId,
        Guid patternId,
        Guid versionId,
        CancellationToken ct = default) =>
        await _db.PatternVersions.AsNoTracking()
            .FirstOrDefaultAsync(v => v.TenantId == tenantId && v.PatternModelId == patternId && v.Id == versionId, ct);

    public async Task<(PatternVersion Version, PatternDocument? Document, IReadOnlyList<string> Issues)> GenerateAsync(
        Guid tenantId,
        Guid patternId,
        GeneratePatternRequest request,
        CancellationToken ct = default)
    {
        var model = await _db.PatternModels
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == patternId, ct)
            ?? throw new InvalidOperationException("Pattern not found for tenant.");

        if (model.BaseKind == PatternBaseKind.Blank)
            throw new PatternValidationException("blank_base", "Blank canvas cannot be generated from a parametric base.");

        Dictionary<string, decimal>? measurementPrefill = null;
        if (request.MeasurementSetId is { } setId)
        {
            var set = await _db.MeasurementSets.AsNoTracking()
                .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.Id == setId, ct)
                ?? throw new InvalidOperationException("Measurement set not found for tenant.");
            measurementPrefill = set.ValuesCm;
        }

        PatternDocument document;
        object resolvedInput;
        try
        {
            (document, resolvedInput) = model.BaseKind switch
            {
                PatternBaseKind.StraightSkirt => GenerateSkirt(request, measurementPrefill),
                PatternBaseKind.SimpleDress => GenerateDress(request, measurementPrefill),
                _ => throw new PatternValidationException("unsupported_base", $"Unsupported base kind: {model.BaseKind}")
            };
        }
        catch (PatternValidationException)
        {
            _logger.LogInformation("Pattern generate validation failed for pattern {PatternId} tenant {TenantId}", patternId, tenantId);
            throw;
        }

        var issues = PatternQualityChecks.Evaluate(document);
        var parametersJson = JsonSerializer.Serialize(resolvedInput, ApiJson);
        var geometryJson = PatternDocumentJson.Serialize(document);
        var issuesJson = JsonSerializer.Serialize(issues, ApiJson);

        var nextVersion = await _db.PatternVersions
            .Where(v => v.TenantId == tenantId && v.PatternModelId == patternId)
            .Select(v => (int?)v.Version)
            .MaxAsync(ct) ?? 0;

        var version = new PatternVersion
        {
            TenantId = tenantId,
            PatternModelId = patternId,
            Version = nextVersion + 1,
            ParametersJson = parametersJson,
            GeometryJson = geometryJson,
            QualityIssuesJson = issuesJson,
            CreatedByUserId = request.ActorUserId
        };
        _db.PatternVersions.Add(version);

        model.Status = PatternModelStatus.Ready;
        model.UpdatedAt = DateTimeOffset.UtcNow;

        await AddAuditAsync(tenantId, request.ActorUserId, "pattern.version.generated", nameof(PatternVersion), version.Id,
            $"{{\"patternId\":\"{patternId}\",\"version\":{version.Version}}}", ct);
        await _db.SaveChangesAsync(ct);
        return (version, document, issues);
    }

    public async Task<PatternVersion> CreateBlankOrCopyVersionAsync(
        Guid tenantId,
        Guid patternId,
        CreateVersionRequest request,
        CancellationToken ct = default)
    {
        var model = await _db.PatternModels
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == patternId, ct)
            ?? throw new InvalidOperationException("Pattern not found for tenant.");

        var nextVersion = await _db.PatternVersions
            .Where(v => v.TenantId == tenantId && v.PatternModelId == patternId)
            .Select(v => (int?)v.Version)
            .MaxAsync(ct) ?? 0;

        string parametersJson = request.ParametersJson ?? "{}";
        string? geometryJson = null;
        string? issuesJson = null;

        if (request.CopyFromVersionId is { } copyId)
        {
            var source = await _db.PatternVersions.AsNoTracking()
                .FirstOrDefaultAsync(v => v.TenantId == tenantId && v.PatternModelId == patternId && v.Id == copyId, ct)
                ?? throw new InvalidOperationException("Source version not found for tenant.");
            parametersJson = source.ParametersJson;
            geometryJson = source.GeometryJson;
            issuesJson = source.QualityIssuesJson;
        }
        else if (!string.IsNullOrWhiteSpace(request.ParametersJson))
        {
            parametersJson = request.ParametersJson!;
        }

        var version = new PatternVersion
        {
            TenantId = tenantId,
            PatternModelId = patternId,
            Version = nextVersion + 1,
            ParametersJson = parametersJson,
            GeometryJson = geometryJson,
            QualityIssuesJson = issuesJson,
            CreatedByUserId = request.ActorUserId
        };
        _db.PatternVersions.Add(version);
        model.UpdatedAt = DateTimeOffset.UtcNow;

        await AddAuditAsync(tenantId, request.ActorUserId, "pattern.version.created", nameof(PatternVersion), version.Id,
            $"{{\"patternId\":\"{patternId}\",\"version\":{version.Version}}}", ct);
        await _db.SaveChangesAsync(ct);
        return version;
    }

    public async Task<TechnicalSheet?> GetTechnicalSheetAsync(Guid tenantId, Guid patternId, CancellationToken ct = default) =>
        await _db.TechnicalSheets.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.PatternModelId == patternId, ct);

    public async Task<TechnicalSheet> UpsertTechnicalSheetAsync(
        Guid tenantId,
        Guid patternId,
        string? materialsNotes,
        string? constructionNotes,
        Guid? actorUserId,
        CancellationToken ct = default)
    {
        _ = await _db.PatternModels.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == patternId, ct)
            ?? throw new InvalidOperationException("Pattern not found for tenant.");

        var sheet = await _db.TechnicalSheets
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.PatternModelId == patternId, ct);

        if (sheet is null)
        {
            sheet = new TechnicalSheet
            {
                TenantId = tenantId,
                PatternModelId = patternId
            };
            _db.TechnicalSheets.Add(sheet);
        }

        sheet.MaterialsNotes = string.IsNullOrWhiteSpace(materialsNotes) ? null : materialsNotes.Trim();
        sheet.ConstructionNotes = string.IsNullOrWhiteSpace(constructionNotes) ? null : constructionNotes.Trim();
        sheet.UpdatedAt = DateTimeOffset.UtcNow;

        await AddAuditAsync(tenantId, actorUserId, "technical_sheet.updated", nameof(TechnicalSheet), sheet.Id,
            $"{{\"patternId\":\"{patternId}\"}}", ct);
        await _db.SaveChangesAsync(ct);
        return sheet;
    }

    public async Task<ExportJob> CreateExportJobAsync(
        Guid tenantId,
        Guid patternId,
        Guid? versionId,
        Guid? actorUserId,
        CancellationToken ct = default)
    {
        var model = await _db.PatternModels.AsNoTracking()
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == patternId, ct)
            ?? throw new InvalidOperationException("Pattern not found for tenant.");

        PatternVersion? version;
        if (versionId is { } vid)
        {
            version = await _db.PatternVersions.AsNoTracking()
                .FirstOrDefaultAsync(v => v.TenantId == tenantId && v.PatternModelId == patternId && v.Id == vid, ct)
                ?? throw new InvalidOperationException("Pattern version not found for tenant.");
        }
        else
        {
            version = await GetLatestVersionAsync(tenantId, patternId, ct);
        }

        if (version?.GeometryJson is null)
            throw new InvalidOperationException("No geometry available to export. Generate a version first.");

        var job = new ExportJob
        {
            TenantId = tenantId,
            PatternModelId = patternId,
            PatternVersionId = version.Id,
            Status = ExportJobStatus.Queued,
            Format = "pdf_a4"
        };
        _db.ExportJobs.Add(job);
        await AddAuditAsync(tenantId, actorUserId, "export.queued", nameof(ExportJob), job.Id,
            $"{{\"patternId\":\"{patternId}\",\"format\":\"pdf_a4\"}}", ct);
        await _db.SaveChangesAsync(ct);

        // In-process job (Redis deferred): run synchronously for MVP reliability.
        await ProcessExportJobAsync(job.Id, model.Name, ct);
        return (await _db.ExportJobs.AsNoTracking().FirstAsync(j => j.Id == job.Id, ct))!;
    }

    public async Task<ExportJob?> GetExportJobAsync(Guid tenantId, Guid jobId, CancellationToken ct = default) =>
        await _db.ExportJobs.AsNoTracking()
            .FirstOrDefaultAsync(j => j.TenantId == tenantId && j.Id == jobId, ct);

    private async Task ProcessExportJobAsync(Guid jobId, string patternName, CancellationToken ct)
    {
        var job = await _db.ExportJobs.FirstAsync(j => j.Id == jobId, ct);
        job.Status = ExportJobStatus.Running;
        job.StartedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        try
        {
            var version = await _db.PatternVersions.AsNoTracking()
                .FirstAsync(v => v.Id == job.PatternVersionId, ct);
            var document = PatternDocumentJson.Deserialize(version.GeometryJson!);
            var bytes = PatternPdfExporter.ExportA4(document, patternName);

            job.ResultBytes = bytes;
            job.Status = ExportJobStatus.Succeeded;
            job.CompletedAt = DateTimeOffset.UtcNow;
            job.ErrorMessage = null;
            _logger.LogInformation("Export job {JobId} succeeded ({Bytes} bytes)", jobId, bytes.Length);
        }
        catch (Exception ex)
        {
            job.Status = ExportJobStatus.Failed;
            job.CompletedAt = DateTimeOffset.UtcNow;
            job.ErrorMessage = "export_failed";
            _logger.LogError(ex, "Export job {JobId} failed", jobId);
        }

        await _db.SaveChangesAsync(ct);
    }

    private static (PatternDocument Document, object Input) GenerateSkirt(
        GeneratePatternRequest request,
        Dictionary<string, decimal>? prefill)
    {
        prefill ??= new Dictionary<string, decimal>(StringComparer.Ordinal);

        decimal? Pref(string key) => prefill.TryGetValue(key, out var v) ? v : null;

        var input = new StraightSkirtInput
        {
            WaistCirc = Require("waistCirc", request.WaistCircCm, Pref(MeasurementKeys.WaistCirc)),
            HipCirc = Require("hipCirc", request.HipCircCm, Pref(MeasurementKeys.HipCirc)),
            SkirtLength = Require("skirtLength", request.SkirtLengthCm, Pref(MeasurementKeys.SkirtLength)),
            EaseWaist = request.EaseWaistCm ?? Pref(MeasurementKeys.EaseWaist) ?? 2m,
            EaseHip = request.EaseHipCm ?? Pref(MeasurementKeys.EaseHip) ?? 4m,
            WaistToHip = request.WaistToHipCm ?? Pref(MeasurementKeys.WaistToHip) ?? 20m,
            SeamAllowance = request.SeamAllowanceCm ?? 1.0m,
            HemAllowance = request.HemAllowanceCm ?? 3.0m,
            WaistbandHeight = request.WaistbandHeightCm ?? 0m,
            LengthIncludesHem = request.LengthIncludesHem ?? false
        };

        return (StraightSkirtPattern.Generate(input), input);

        decimal Require(string key, decimal? body, decimal? fromPrefill) =>
            body ?? fromPrefill ?? throw new PatternValidationException(
                "missing_parameter",
                $"Required parameter '{key}' was not provided.",
                [$"missing:{key}"]);
    }

    private static (PatternDocument Document, object Input) GenerateDress(
        GeneratePatternRequest request,
        Dictionary<string, decimal>? prefill)
    {
        decimal Require(string key, decimal? body, decimal? fromPrefill) =>
            body ?? fromPrefill ?? throw new PatternValidationException(
                "missing_parameter",
                $"Required parameter '{key}' was not provided.",
                [$"missing:{key}"]);

        prefill ??= new Dictionary<string, decimal>(StringComparer.Ordinal);

        decimal? Pref(string key) => prefill.TryGetValue(key, out var v) ? v : null;

        var input = new SimpleDressInput
        {
            BustCirc = Require("bustCirc", request.BustCircCm, Pref(MeasurementKeys.BustCirc)),
            WaistCirc = Require("waistCirc", request.WaistCircCm, Pref(MeasurementKeys.WaistCirc)),
            HipCirc = Require("hipCirc", request.HipCircCm, Pref(MeasurementKeys.HipCirc)),
            DressLength = Require("dressLength", request.DressLengthCm, Pref(MeasurementKeys.DressLength)),
            EaseBust = request.EaseBustCm ?? Pref(MeasurementKeys.EaseBust) ?? 4m,
            EaseWaist = request.EaseWaistCm ?? Pref(MeasurementKeys.EaseWaist) ?? 2m,
            EaseHip = request.EaseHipCm ?? Pref(MeasurementKeys.EaseHip) ?? 4m,
            ShoulderToBust = request.ShoulderToBustCm ?? 26m,
            BustToWaist = request.BustToWaistCm ?? 20m,
            WaistToHip = request.WaistToHipCm ?? Pref(MeasurementKeys.WaistToHip) ?? 20m,
            SeamAllowance = request.SeamAllowanceCm ?? 1.0m,
            HemAllowance = request.HemAllowanceCm ?? 3.0m,
            LengthIncludesHem = request.LengthIncludesHem ?? false
        };

        return (SimpleDressPattern.Generate(input), input);
    }

    public static string ToBaseKindString(PatternBaseKind kind) => kind switch
    {
        PatternBaseKind.StraightSkirt => "straight_skirt",
        PatternBaseKind.SimpleDress => "simple_dress",
        PatternBaseKind.Blank => "blank",
        _ => kind.ToString()
    };

    public static PatternBaseKind ParseBaseKind(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "straight_skirt" => PatternBaseKind.StraightSkirt,
        "simple_dress" => PatternBaseKind.SimpleDress,
        "blank" => PatternBaseKind.Blank,
        _ => throw new ArgumentException("baseKind must be straight_skirt, simple_dress, or blank.")
    };

    public static string ToStatusString(PatternModelStatus status) => status switch
    {
        PatternModelStatus.Draft => "draft",
        PatternModelStatus.Ready => "ready",
        _ => status.ToString()
    };

    private async Task EnsureOrganizationAsync(Guid tenantId, CancellationToken ct)
    {
        var exists = await _db.Organizations.AnyAsync(o => o.TenantId == tenantId, ct);
        if (!exists)
            throw new InvalidOperationException("Organization (tenant) not found.");
    }

    private Task AddAuditAsync(
        Guid tenantId,
        Guid? actorUserId,
        string action,
        string entityType,
        Guid entityId,
        string? metadataJson,
        CancellationToken ct)
    {
        _db.AuditEvents.Add(new AuditEvent
        {
            TenantId = tenantId,
            ActorUserId = actorUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            MetadataJson = metadataJson,
            OccurredAt = DateTimeOffset.UtcNow
        });
        return Task.CompletedTask;
    }
}

public sealed record GeneratePatternRequest(
    Guid? MeasurementSetId = null,
    decimal? BustCircCm = null,
    decimal? WaistCircCm = null,
    decimal? HipCircCm = null,
    decimal? SkirtLengthCm = null,
    decimal? DressLengthCm = null,
    decimal? EaseBustCm = null,
    decimal? EaseWaistCm = null,
    decimal? EaseHipCm = null,
    decimal? WaistToHipCm = null,
    decimal? ShoulderToBustCm = null,
    decimal? BustToWaistCm = null,
    decimal? SeamAllowanceCm = null,
    decimal? HemAllowanceCm = null,
    decimal? WaistbandHeightCm = null,
    bool? LengthIncludesHem = null,
    Guid? ActorUserId = null);

public sealed record CreateVersionRequest(
    string? ParametersJson = null,
    Guid? CopyFromVersionId = null,
    Guid? ActorUserId = null);
