using Microsoft.EntityFrameworkCore;
using ModelaFlow.Api.Data;
using ModelaFlow.Api.Services;
using ModelaFlow.PatternCore.Measurements;

namespace ModelaFlow.Api.Tests;

public class MeasurementVersioningTests
{
    [Fact]
    public async Task CreateMeasurementSet_AppendsVersions_DoesNotOverwrite()
    {
        await using var db = CreateDb();
        var service = new PlatformService(db);

        var org = await service.CreateOrganizationAsync("Atelier");
        var customer = await service.CreateCustomerAsync(org.TenantId, "Maria", null, null);

        var v1 = await service.CreateMeasurementSetAsync(
            org.TenantId,
            customer.Id,
            new BodyMeasurementsCm(WaistCirc: 68m, HipCirc: 92m, SkirtLength: 58m),
            null);

        var v2 = await service.CreateMeasurementSetAsync(
            org.TenantId,
            customer.Id,
            new BodyMeasurementsCm(WaistCirc: 70m, HipCirc: 94m, SkirtLength: 60m),
            null);

        Assert.Equal(1, v1.Version);
        Assert.Equal(2, v2.Version);
        Assert.NotEqual(v1.Id, v2.Id);

        var versions = await service.ListMeasurementVersionsAsync(org.TenantId, customer.Id);
        Assert.Equal(2, versions.Count);
        Assert.Equal(new[] { 2, 1 }, versions.Select(v => v.Version).ToArray());

        var storedV1 = versions.Single(v => v.Version == 1);
        Assert.Equal(68m, storedV1.ValuesCm[MeasurementKeys.WaistCirc]);
        Assert.Equal(92m, storedV1.ValuesCm[MeasurementKeys.HipCirc]);
        Assert.Equal(58m, storedV1.ValuesCm[MeasurementKeys.SkirtLength]);

        var storedV2 = versions.Single(v => v.Version == 2);
        Assert.Equal(70m, storedV2.ValuesCm[MeasurementKeys.WaistCirc]);
        Assert.Equal(94m, storedV2.ValuesCm[MeasurementKeys.HipCirc]);
        Assert.Equal(60m, storedV2.ValuesCm[MeasurementKeys.SkirtLength]);

        Assert.Equal(2, await db.MeasurementSets.CountAsync(m =>
            m.TenantId == org.TenantId && m.CustomerId == customer.Id));
    }

    [Fact]
    public async Task ListMeasurementVersions_FiltersByTenant()
    {
        await using var db = CreateDb();
        var service = new PlatformService(db);

        var orgA = await service.CreateOrganizationAsync("A");
        var orgB = await service.CreateOrganizationAsync("B");
        var customerA = await service.CreateCustomerAsync(orgA.TenantId, "Cliente", null, null);

        await service.CreateMeasurementSetAsync(
            orgA.TenantId,
            customerA.Id,
            new BodyMeasurementsCm(WaistCirc: 70m),
            null);

        var fromB = await service.ListMeasurementVersionsAsync(orgB.TenantId, customerA.Id);
        Assert.Empty(fromB);
    }

    private static ModelaFlowDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ModelaFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new ModelaFlowDbContext(options);
    }
}
