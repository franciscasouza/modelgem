using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ModelaFlow.Api.Domain.Audit;
using ModelaFlow.Api.Domain.Customer;
using ModelaFlow.Api.Domain.Identity;

namespace ModelaFlow.Api.Data;

public class ModelaFlowDbContext : DbContext
{
    public ModelaFlowDbContext(DbContextOptions<ModelaFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<MeasurementSet> MeasurementSets => Set<MeasurementSet>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Organization>(e =>
        {
            e.ToTable("organizations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.TenantId).IsUnique();
            e.HasMany(x => x.Users).WithOne(x => x.Organization).HasForeignKey(x => x.OrganizationId);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).HasMaxLength(320).IsRequired();
            e.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(32);
            e.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
            e.HasIndex(x => x.TenantId);
        });

        modelBuilder.Entity<Customer>(e =>
        {
            e.ToTable("customers");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(2000);
            e.HasIndex(x => x.TenantId);
            e.HasMany(x => x.MeasurementSets).WithOne(x => x.Customer).HasForeignKey(x => x.CustomerId);
        });

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var valuesConverter = new ValueConverter<Dictionary<string, decimal>, string>(
            v => JsonSerializer.Serialize(v, jsonOptions),
            v => JsonSerializer.Deserialize<Dictionary<string, decimal>>(v, jsonOptions)
                 ?? new Dictionary<string, decimal>(StringComparer.Ordinal));

        var valuesComparer = new ValueComparer<Dictionary<string, decimal>>(
            (a, b) => JsonSerializer.Serialize(a, jsonOptions) == JsonSerializer.Serialize(b, jsonOptions),
            v => JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
            v => new Dictionary<string, decimal>(v, StringComparer.Ordinal));

        modelBuilder.Entity<MeasurementSet>(e =>
        {
            e.ToTable("measurement_sets");
            e.HasKey(x => x.Id);
            e.Property(x => x.ValuesCm)
                .HasColumnName("values_cm_json")
                .HasConversion(valuesConverter)
                .Metadata.SetValueComparer(valuesComparer);
            e.HasIndex(x => new { x.TenantId, x.CustomerId, x.Version }).IsUnique();
            e.HasIndex(x => x.TenantId);
        });

        modelBuilder.Entity<AuditEvent>(e =>
        {
            e.ToTable("audit_events");
            e.HasKey(x => x.Id);
            e.Property(x => x.Action).HasMaxLength(100).IsRequired();
            e.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
            e.Property(x => x.MetadataJson).HasMaxLength(4000);
            e.HasIndex(x => x.TenantId);
            e.HasIndex(x => x.OccurredAt);
        });
    }
}
