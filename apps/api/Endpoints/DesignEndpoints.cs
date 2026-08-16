using System.Text.Json;
using ModelaFlow.Api.Domain.Design;
using ModelaFlow.Api.Services;
using ModelaFlow.PatternCore.Serialization;
using ModelaFlow.PatternCore.Validation;

namespace ModelaFlow.Api.Endpoints;

public static class DesignEndpoints
{
    public static RouteGroupBuilder MapDesignEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/v1").WithTags("Design");

        api.MapPost("/dev/bootstrap", async (DesignService service, IHostEnvironment env, CancellationToken ct) =>
        {
            if (!env.IsDevelopment())
                return Results.NotFound();

            var (tenantId, organizationId) = await service.BootstrapDevTenantAsync(ct);
            return Results.Ok(new DevBootstrapResponse(tenantId, organizationId));
        });

        api.MapGet("/tenants/{tenantId:guid}/overview", async (Guid tenantId, DesignService service, CancellationToken ct) =>
        {
            try
            {
                var (customers, patterns) = await service.GetOverviewAsync(tenantId, ct);
                return Results.Ok(new OverviewResponse(customers, patterns));
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        });

        api.MapGet("/tenants/{tenantId:guid}/patterns", async (Guid tenantId, DesignService service, CancellationToken ct) =>
        {
            try
            {
                var patterns = await service.ListPatternsAsync(tenantId, ct: ct);
                return Results.Ok(patterns.Select(ToPatternSummary));
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        });

        api.MapPost("/tenants/{tenantId:guid}/patterns", async (
            Guid tenantId,
            CreatePatternRequest request,
            DesignService service,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["name"] = ["Name is required."]
                });
            }

            PatternBaseKind baseKind;
            try
            {
                baseKind = DesignService.ParseBaseKind(request.BaseKind);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["baseKind"] = [ex.Message]
                });
            }

            try
            {
                var model = await service.CreatePatternAsync(tenantId, request.Name, baseKind, request.CustomerId, request.ActorUserId, ct);
                return Results.Created(
                    $"/api/v1/tenants/{tenantId}/patterns/{model.Id}",
                    ToPatternSummary(model));
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        });

        api.MapGet("/tenants/{tenantId:guid}/patterns/{patternId:guid}", async (
            Guid tenantId,
            Guid patternId,
            DesignService service,
            CancellationToken ct) =>
        {
            var model = await service.GetPatternAsync(tenantId, patternId, ct);
            if (model is null)
                return Results.NotFound(new { error = "Pattern not found for tenant." });

            var latest = await service.GetLatestVersionAsync(tenantId, patternId, ct);
            return Results.Ok(new PatternDetailResponse(
                ToPatternSummary(model),
                latest is null ? null : ToVersionSummary(latest)));
        });

        api.MapPost("/tenants/{tenantId:guid}/patterns/{patternId:guid}/generate", async (
            Guid tenantId,
            Guid patternId,
            GeneratePatternRequest request,
            DesignService service,
            CancellationToken ct) =>
        {
            try
            {
                var (version, document, issues) = await service.GenerateAsync(tenantId, patternId, request, ct);
                object? geometry = document is null
                    ? null
                    : JsonSerializer.Deserialize<object>(PatternDocumentJson.Serialize(document), PatternDocumentJson.Options);

                return Results.Ok(new GeneratePatternResponse(
                    ToVersionSummary(version),
                    geometry,
                    issues));
            }
            catch (PatternValidationException ex)
            {
                return Results.BadRequest(new
                {
                    error = ex.Code,
                    message = ex.Message,
                    details = ex.Details
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        });

        api.MapPost("/tenants/{tenantId:guid}/patterns/{patternId:guid}/versions", async (
            Guid tenantId,
            Guid patternId,
            CreateVersionRequest request,
            DesignService service,
            CancellationToken ct) =>
        {
            try
            {
                var version = await service.CreateBlankOrCopyVersionAsync(tenantId, patternId, request, ct);
                return Results.Created(
                    $"/api/v1/tenants/{tenantId}/patterns/{patternId}/versions/{version.Id}",
                    ToVersionSummary(version));
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        });

        api.MapGet("/tenants/{tenantId:guid}/patterns/{patternId:guid}/versions", async (
            Guid tenantId,
            Guid patternId,
            DesignService service,
            CancellationToken ct) =>
        {
            var model = await service.GetPatternAsync(tenantId, patternId, ct);
            if (model is null)
                return Results.NotFound(new { error = "Pattern not found for tenant." });

            var versions = await service.ListVersionsAsync(tenantId, patternId, ct);
            return Results.Ok(versions.Select(ToVersionSummary));
        });

        api.MapGet("/tenants/{tenantId:guid}/patterns/{patternId:guid}/versions/{versionId:guid}", async (
            Guid tenantId,
            Guid patternId,
            Guid versionId,
            DesignService service,
            CancellationToken ct) =>
        {
            var version = await service.GetVersionAsync(tenantId, patternId, versionId, ct);
            if (version is null)
                return Results.NotFound(new { error = "Version not found for tenant." });

            object? geometry = null;
            if (!string.IsNullOrWhiteSpace(version.GeometryJson))
                geometry = JsonSerializer.Deserialize<object>(version.GeometryJson, PatternDocumentJson.Options);

            IReadOnlyList<string>? issues = null;
            if (!string.IsNullOrWhiteSpace(version.QualityIssuesJson))
                issues = JsonSerializer.Deserialize<List<string>>(version.QualityIssuesJson);

            return Results.Ok(new PatternVersionDetailResponse(
                ToVersionSummary(version),
                version.ParametersJson,
                geometry,
                issues ?? Array.Empty<string>()));
        });

        api.MapGet("/tenants/{tenantId:guid}/patterns/{patternId:guid}/technical-sheet", async (
            Guid tenantId,
            Guid patternId,
            DesignService service,
            CancellationToken ct) =>
        {
            var sheet = await service.GetTechnicalSheetAsync(tenantId, patternId, ct);
            if (sheet is null)
                return Results.NotFound(new { error = "Technical sheet not found for tenant." });
            return Results.Ok(ToSheetResponse(sheet));
        });

        api.MapPut("/tenants/{tenantId:guid}/patterns/{patternId:guid}/technical-sheet", async (
            Guid tenantId,
            Guid patternId,
            UpdateTechnicalSheetRequest request,
            DesignService service,
            CancellationToken ct) =>
        {
            try
            {
                var sheet = await service.UpsertTechnicalSheetAsync(
                    tenantId, patternId, request.MaterialsNotes, request.ConstructionNotes, request.ActorUserId, ct);
                return Results.Ok(ToSheetResponse(sheet));
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        });

        api.MapPost("/tenants/{tenantId:guid}/patterns/{patternId:guid}/exports", async (
            Guid tenantId,
            Guid patternId,
            CreateExportRequest? request,
            DesignService service,
            CancellationToken ct) =>
        {
            try
            {
                var job = await service.CreateExportJobAsync(
                    tenantId, patternId, request?.VersionId, request?.ActorUserId, ct);
                return Results.Accepted(
                    $"/api/v1/tenants/{tenantId}/exports/{job.Id}",
                    ToExportResponse(tenantId, job));
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        });

        api.MapGet("/tenants/{tenantId:guid}/exports/{jobId:guid}", async (
            Guid tenantId,
            Guid jobId,
            DesignService service,
            CancellationToken ct) =>
        {
            var job = await service.GetExportJobAsync(tenantId, jobId, ct);
            if (job is null)
                return Results.NotFound(new { error = "Export job not found for tenant." });
            return Results.Ok(ToExportResponse(tenantId, job));
        });

        api.MapGet("/tenants/{tenantId:guid}/exports/{jobId:guid}/download", async (
            Guid tenantId,
            Guid jobId,
            DesignService service,
            CancellationToken ct) =>
        {
            var job = await service.GetExportJobAsync(tenantId, jobId, ct);
            if (job is null)
                return Results.NotFound(new { error = "Export job not found for tenant." });
            if (job.Status != ExportJobStatus.Succeeded || job.ResultBytes is null)
                return Results.Conflict(new { error = "Export is not ready.", status = job.Status.ToString().ToLowerInvariant() });

            return Results.File(job.ResultBytes, "application/pdf", $"pattern-{job.PatternModelId:N}.pdf");
        });

        return api;
    }

    private static PatternSummaryResponse ToPatternSummary(PatternModel model) =>
        new(
            model.Id,
            model.TenantId,
            model.CustomerId,
            model.Name,
            model.ReferenceCode,
            DesignService.ToBaseKindString(model.BaseKind),
            DesignService.ToStatusString(model.Status),
            model.CreatedAt,
            model.UpdatedAt);

    private static PatternVersionSummaryResponse ToVersionSummary(PatternVersion version) =>
        new(
            version.Id,
            version.PatternModelId,
            version.Version,
            version.GeometryJson is not null,
            version.CreatedAt,
            version.CreatedByUserId);

    private static TechnicalSheetResponse ToSheetResponse(TechnicalSheet sheet) =>
        new(sheet.Id, sheet.PatternModelId, sheet.MaterialsNotes, sheet.ConstructionNotes, sheet.UpdatedAt);

    private static ExportJobResponse ToExportResponse(Guid tenantId, ExportJob job)
    {
        var status = job.Status switch
        {
            ExportJobStatus.Queued => "queued",
            ExportJobStatus.Running => "running",
            ExportJobStatus.Succeeded => "succeeded",
            ExportJobStatus.Failed => "failed",
            _ => job.Status.ToString().ToLowerInvariant()
        };

        string? downloadUrl = job.Status == ExportJobStatus.Succeeded
            ? $"/api/v1/tenants/{tenantId}/exports/{job.Id}/download"
            : null;

        return new ExportJobResponse(
            job.Id,
            job.PatternModelId,
            job.PatternVersionId,
            status,
            job.Format,
            downloadUrl,
            job.ResultBytes?.Length,
            job.ErrorMessage,
            job.CreatedAt,
            job.CompletedAt);
    }
}

public sealed record DevBootstrapResponse(Guid TenantId, Guid OrganizationId);

public sealed record OverviewResponse(int CustomerCount, int PatternCount);

public sealed record CreatePatternRequest(string Name, string BaseKind, Guid? CustomerId = null, Guid? ActorUserId = null);

public sealed record UpdateTechnicalSheetRequest(
    string? MaterialsNotes = null,
    string? ConstructionNotes = null,
    Guid? ActorUserId = null);

public sealed record CreateExportRequest(Guid? VersionId = null, Guid? ActorUserId = null);

public sealed record PatternSummaryResponse(
    Guid Id,
    Guid TenantId,
    Guid? CustomerId,
    string Name,
    string ReferenceCode,
    string BaseKind,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record PatternVersionSummaryResponse(
    Guid Id,
    Guid PatternModelId,
    int Version,
    bool HasGeometry,
    DateTimeOffset CreatedAt,
    Guid? CreatedByUserId);

public sealed record PatternDetailResponse(
    PatternSummaryResponse Pattern,
    PatternVersionSummaryResponse? LatestVersion);

public sealed record GeneratePatternResponse(
    PatternVersionSummaryResponse Version,
    object? Geometry,
    IReadOnlyList<string> QualityIssues);

public sealed record PatternVersionDetailResponse(
    PatternVersionSummaryResponse Summary,
    string ParametersJson,
    object? Geometry,
    IReadOnlyList<string> QualityIssues);

public sealed record TechnicalSheetResponse(
    Guid Id,
    Guid PatternModelId,
    string? MaterialsNotes,
    string? ConstructionNotes,
    DateTimeOffset UpdatedAt);

public sealed record ExportJobResponse(
    Guid Id,
    Guid PatternModelId,
    Guid? PatternVersionId,
    string Status,
    string Format,
    string? DownloadUrl,
    int? ByteLength,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);
