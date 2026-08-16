using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModelaFlow.Api.Auth;
using ModelaFlow.Api.Data;
using ModelaFlow.Api.Domain.Identity;
using ModelaFlow.Api.Endpoints;
using ModelaFlow.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Host=localhost;Port=5432;Database=modelaflow;Username=modelaflow;Password=CHANGE_ME";

builder.Services.AddDbContext<ModelaFlowDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.SectionName));
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<PlatformService>();
builder.Services.AddScoped<DesignService>();
builder.Services.AddScoped<AuthService>();

var authOptions = builder.Configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
var jwtService = new JwtTokenService(Microsoft.Extensions.Options.Options.Create(authOptions));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = jwtService.CreateValidationParameters();
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (!string.IsNullOrEmpty(context.Token))
                    return Task.CompletedTask;

                var cookieName = authOptions.CookieName;
                if (context.Request.Cookies.TryGetValue(cookieName, out var cookieToken)
                    && !string.IsNullOrWhiteSpace(cookieToken))
                {
                    context.Token = cookieToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var corsOrigins = builder.Configuration["Cors:Origins"]
        ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? ["http://localhost:3000"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebApp", policy =>
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

var applyMigrations = app.Configuration.GetValue("Database:ApplyMigrations", app.Environment.IsDevelopment());
if (applyMigrations)
{
    using var migrateScope = app.Services.CreateScope();
    var db = migrateScope.ServiceProvider.GetRequiredService<ModelaFlowDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        app.Logger.LogInformation("Database migrations applied.");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "MigrateAsync failed — continuing without applying migrations.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var design = scope.ServiceProvider.GetRequiredService<DesignService>();
    var auth = scope.ServiceProvider.GetRequiredService<AuthService>();
    try
    {
        await design.EnsureDevTenantSeededAsync();
        await auth.EnsureDevDemoCredentialsAsync();
        app.Logger.LogInformation(
            "Dev tenant ready: {TenantId}. Login: {Email} (see README). Also POST /api/v1/dev/bootstrap",
            DesignService.DevTenantId,
            AuthOptions.DevDemoEmail);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Dev tenant seed skipped (database unavailable?). Use POST /api/v1/dev/bootstrap when DB is up.");
    }
}

app.UseCors("WebApp");

var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? string.Empty;
if (urls.Contains("https://", StringComparison.OrdinalIgnoreCase))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseMiddleware<TenantAccessMiddleware>();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "ModelaFlow.Api",
    // Redis / S3 deferred; export jobs run in-process — see ADR-0002 / ADR-0003
    pending = new[] { "redis", "s3-storage", "job-queue-provider" }
}));

app.MapAuthEndpoints();
app.MapPlatformEndpoints();
app.MapDesignEndpoints();

app.Run();

public partial class Program;
