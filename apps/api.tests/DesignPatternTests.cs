using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ModelaFlow.Api.Data;
using ModelaFlow.Api.Domain.Design;
using ModelaFlow.Api.Services;
using ModelaFlow.PatternCore.Validation;

namespace ModelaFlow.Api.Tests;

public class DesignPatternTests
{
    [Fact]
    public async Task ListPatterns_FiltersByTenantId()
    {
        await using var db = CreateDb();
        var platform = new PlatformService(db);
        var service = CreateService(db);

        var orgA = await platform.CreateOrganizationAsync("A");
        var orgB = await platform.CreateOrganizationAsync("B");

        await service.CreatePatternAsync(orgA.TenantId, "Saia A", PatternBaseKind.StraightSkirt, null, null);
        await service.CreatePatternAsync(orgA.TenantId, "Vestido A", PatternBaseKind.SimpleDress, null, null);
        await service.CreatePatternAsync(orgB.TenantId, "Saia B", PatternBaseKind.StraightSkirt, null, null);

        var listA = await service.ListPatternsAsync(orgA.TenantId);
        var listB = await service.ListPatternsAsync(orgB.TenantId);

        Assert.Equal(2, listA.Count);
        Assert.All(listA, p => Assert.Equal(orgA.TenantId, p.TenantId));
        Assert.DoesNotContain(listA, p => p.Name == "Saia B");
        Assert.Single(listB);
        Assert.Equal("Saia B", listB[0].Name);
    }

    [Fact]
    public async Task Generate_StraightSkirt_CreatesVersionWithGeometry()
    {
        await using var db = CreateDb();
        var platform = new PlatformService(db);
        var service = CreateService(db);
        var org = await platform.CreateOrganizationAsync("Atelier");
        var pattern = await service.CreatePatternAsync(org.TenantId, "Saia reta", PatternBaseKind.StraightSkirt, null, null);

        var (version, document, issues) = await service.GenerateAsync(
            org.TenantId,
            pattern.Id,
            new GeneratePatternRequest(
                WaistCircCm: 70m,
                HipCircCm: 96m,
                SkirtLengthCm: 60m));

        Assert.Equal(1, version.Version);
        Assert.False(string.IsNullOrWhiteSpace(version.GeometryJson));
        Assert.NotNull(document);
        Assert.Equal(2, document!.Pieces.Count);
        Assert.Empty(issues);

        var versions = await service.ListVersionsAsync(org.TenantId, pattern.Id);
        Assert.Single(versions);

        var overview = await service.GetOverviewAsync(org.TenantId);
        Assert.Equal(1, overview.PatternCount);
    }

    [Fact]
    public async Task Generate_InvalidMeasures_ThrowsValidation()
    {
        await using var db = CreateDb();
        var platform = new PlatformService(db);
        var service = CreateService(db);
        var org = await platform.CreateOrganizationAsync("Atelier");
        var pattern = await service.CreatePatternAsync(org.TenantId, "Saia", PatternBaseKind.StraightSkirt, null, null);

        var ex = await Assert.ThrowsAsync<PatternValidationException>(() =>
            service.GenerateAsync(
                org.TenantId,
                pattern.Id,
                new GeneratePatternRequest(
                    WaistCircCm: 10m,
                    HipCircCm: 96m,
                    SkirtLengthCm: 60m)));

        Assert.Equal("validation_failed", ex.Code);
        Assert.NotEmpty(ex.Details);

        var versions = await service.ListVersionsAsync(org.TenantId, pattern.Id);
        Assert.Empty(versions);
    }

    [Fact]
    public async Task GetPattern_DoesNotReturnOtherTenant()
    {
        await using var db = CreateDb();
        var platform = new PlatformService(db);
        var service = CreateService(db);
        var orgA = await platform.CreateOrganizationAsync("A");
        var orgB = await platform.CreateOrganizationAsync("B");
        var pattern = await service.CreatePatternAsync(orgA.TenantId, "Privado", PatternBaseKind.Blank, null, null);

        Assert.Null(await service.GetPatternAsync(orgB.TenantId, pattern.Id));
        Assert.NotNull(await service.GetPatternAsync(orgA.TenantId, pattern.Id));
    }

    [Fact]
    public async Task BootstrapDevTenant_IsIdempotentAndStable()
    {
        await using var db = CreateDb();
        var service = CreateService(db);

        var first = await service.BootstrapDevTenantAsync();
        var second = await service.BootstrapDevTenantAsync();

        Assert.Equal(DesignService.DevTenantId, first.TenantId);
        Assert.Equal(first.TenantId, second.TenantId);
        Assert.Equal(first.OrganizationId, second.OrganizationId);
        Assert.Equal(1, await db.Organizations.CountAsync());
    }

    private static DesignService CreateService(ModelaFlowDbContext db) =>
        new(db, NullLogger<DesignService>.Instance);

    private static ModelaFlowDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ModelaFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new ModelaFlowDbContext(options);
    }
}
