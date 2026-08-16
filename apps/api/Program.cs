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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "ModelaFlow.Api",
    // Redis / S3 / job queue intentionally deferred — see ADR-0002 and architecture.md
    pending = new[] { "redis", "s3-storage", "job-queue" }
}));

app.MapPlatformEndpoints();

app.Run();

public partial class Program;
