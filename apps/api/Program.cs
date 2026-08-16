using Microsoft.EntityFrameworkCore;
using ModelaFlow.Api.Data;
using ModelaFlow.Api.Endpoints;
using ModelaFlow.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Host=localhost;Port=5432;Database=modelaflow;Username=modelaflow;Password=CHANGE_ME";

builder.Services.AddDbContext<ModelaFlowDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<PlatformService>();
builder.Services.AddScoped<DesignService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebDev", policy =>
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Stable demo tenant for Next.js (tenantId = 11111111-1111-1111-1111-111111111111).
    // Prefer PostgreSQL; fall back to EnsureCreated when migrations cannot run (local without DB still needs API tests).
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ModelaFlowDbContext>();
    try
    {
        await db.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "MigrateAsync failed in Development — continuing without applying migrations.");
    }

    var design = scope.ServiceProvider.GetRequiredService<DesignService>();
    try
    {
        await design.EnsureDevTenantSeededAsync();
        app.Logger.LogInformation(
            "Dev tenant ready: {TenantId}. Also available via POST /api/v1/dev/bootstrap",
            DesignService.DevTenantId);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Dev tenant seed skipped (database unavailable?). Use POST /api/v1/dev/bootstrap when DB is up.");
    }
}

app.UseCors("WebDev");
app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "ModelaFlow.Api",
    // Redis / S3 deferred; export jobs run in-process — see ADR-0002 / ADR-0003
    pending = new[] { "redis", "s3-storage", "job-queue-provider" }
}));

app.MapPlatformEndpoints();
app.MapDesignEndpoints();

app.Run();

public partial class Program;
