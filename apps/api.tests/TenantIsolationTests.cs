using Microsoft.EntityFrameworkCore;
using ModelaFlow.Api.Data;
using ModelaFlow.Api.Domain.Identity;
using ModelaFlow.Api.Services;
using ModelaFlow.PatternCore.Measurements;

namespace ModelaFlow.Api.Tests;

public class TenantIsolationTests
{
    [Fact]
    public async Task ListCustomers_FiltersByTenantId()
    {
        await using var db = CreateDb();
        var service = new PlatformService(db);

        var orgA = await service.CreateOrganizationAsync("Atelier A");
        var orgB = await service.CreateOrganizationAsync("Atelier B");

        await service.CreateCustomerAsync(orgA.TenantId, "Cliente A1", null, null);
        await service.CreateCustomerAsync(orgA.TenantId, "Cliente A2", null, null);
        await service.CreateCustomerAsync(orgB.TenantId, "Cliente B1", null, null);

        var listA = await service.ListCustomersAsync(orgA.TenantId);
        var listB = await service.ListCustomersAsync(orgB.TenantId);

        Assert.Equal(2, listA.Count);
        Assert.All(listA, c => Assert.Equal(orgA.TenantId, c.TenantId));
        Assert.DoesNotContain(listA, c => c.Name == "Cliente B1");

        Assert.Single(listB);
        Assert.Equal("Cliente B1", listB[0].Name);
        Assert.Equal(orgB.TenantId, listB[0].TenantId);
    }

    [Fact]
    public async Task GetCustomer_DoesNotReturnOtherTenant()
    {
        await using var db = CreateDb();
        var service = new PlatformService(db);

        var orgA = await service.CreateOrganizationAsync("Atelier A");
        var orgB = await service.CreateOrganizationAsync("Atelier B");
        var customerA = await service.CreateCustomerAsync(orgA.TenantId, "Somente A", null, null);

        var foundWithWrongTenant = await service.GetCustomerAsync(orgB.TenantId, customerA.Id);
        var foundWithRightTenant = await service.GetCustomerAsync(orgA.TenantId, customerA.Id);

        Assert.Null(foundWithWrongTenant);
        Assert.NotNull(foundWithRightTenant);
        Assert.Equal(customerA.Id, foundWithRightTenant!.Id);
    }

    [Fact]
    public async Task CreateMeasurementSet_RejectsCustomerFromOtherTenant()
    {
        await using var db = CreateDb();
        var service = new PlatformService(db);

        var orgA = await service.CreateOrganizationAsync("Atelier A");
        var orgB = await service.CreateOrganizationAsync("Atelier B");
        var customerA = await service.CreateCustomerAsync(orgA.TenantId, "Cliente A", null, null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateMeasurementSetAsync(
                orgB.TenantId,
                customerA.Id,
                new BodyMeasurementsCm(WaistCirc: 70m, HipCirc: 95m, SkirtLength: 60m),
                null));
    }

    private static ModelaFlowDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ModelaFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new ModelaFlowDbContext(options);
    }
}
