using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModelaFlow.Api.Auth;
using ModelaFlow.Api.Data;
using ModelaFlow.Api.Domain.Audit;
using ModelaFlow.Api.Domain.Identity;

namespace ModelaFlow.Api.Services;

public sealed class AuthService
{
    private readonly ModelaFlowDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtTokenService _jwt;

    public AuthService(
        ModelaFlowDbContext db,
        IPasswordHasher<User> passwordHasher,
        JwtTokenService jwt)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
    }

    public async Task<AuthSessionResult> RegisterAsync(
        string organizationName,
        string email,
        string? displayName,
        string password,
        CancellationToken ct = default)
    {
        ValidatePassword(password);

        var normalizedEmail = NormalizeEmail(email);
        if (string.IsNullOrWhiteSpace(organizationName))
            throw new AuthValidationException("organizationName", "Organization name is required.");
        if (string.IsNullOrWhiteSpace(normalizedEmail))
            throw new AuthValidationException("email", "Email is required.");

        var exists = await _db.Users.AnyAsync(u => u.Email == normalizedEmail, ct);
        if (exists)
            throw new AuthConflictException("Email is already registered.");

        var orgId = Guid.NewGuid();
        var org = new Organization
        {
            Id = orgId,
            TenantId = orgId,
            Name = organizationName.Trim()
        };

        var user = new User
        {
            TenantId = orgId,
            OrganizationId = orgId,
            Email = normalizedEmail,
            DisplayName = string.IsNullOrWhiteSpace(displayName)
                ? normalizedEmail.Split('@')[0]
                : displayName.Trim(),
            Role = UserRole.Owner,
            SecurityStamp = Guid.NewGuid().ToString("N")
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        _db.Organizations.Add(org);
        _db.Users.Add(user);
        await AddAuditAsync(org.TenantId, user.Id, "auth.register", nameof(User), user.Id,
            $"{{\"email\":\"{normalizedEmail}\"}}", ct);
        await _db.SaveChangesAsync(ct);

        var token = _jwt.CreateToken(user, org.Name);
        return new AuthSessionResult(user, org.Name, token);
    }

    public async Task<AuthSessionResult?> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await _db.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

        if (user is null || string.IsNullOrEmpty(user.PasswordHash))
            return null;

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
            return null;

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
        }

        var orgName = user.Organization?.Name ?? string.Empty;
        await AddAuditAsync(user.TenantId, user.Id, "auth.login", nameof(User), user.Id,
            $"{{\"email\":\"{normalizedEmail}\"}}", ct);
        await _db.SaveChangesAsync(ct);

        var token = _jwt.CreateToken(user, orgName);
        return new AuthSessionResult(user, orgName, token);
    }

    public async Task LogoutAsync(Guid? tenantId, Guid? userId, CancellationToken ct = default)
    {
        if (tenantId is null || userId is null)
            return;

        await AddAuditAsync(tenantId.Value, userId.Value, "auth.logout", nameof(User), userId.Value, null, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<AuthMeResult?> GetMeAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            return null;

        return new AuthMeResult(
            user.Id,
            user.Email,
            user.DisplayName,
            user.TenantId,
            user.Organization?.Name ?? string.Empty,
            user.Role.ToString());
    }

    /// <summary>
    /// Ensures the stable Dev tenant has a loginable Owner (demo@modelaflow.local / ChangeMe!).
    /// </summary>
    public async Task EnsureDevDemoCredentialsAsync(CancellationToken ct = default)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.TenantId == DesignService.DevTenantId && u.Email == AuthOptions.DevDemoEmail, ct);

        if (user is null)
            return;

        if (!string.IsNullOrEmpty(user.PasswordHash))
        {
            var check = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, AuthOptions.DevDemoPassword);
            if (check != PasswordVerificationResult.Failed)
                return;
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, AuthOptions.DevDemoPassword);
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        await _db.SaveChangesAsync(ct);
    }

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new AuthValidationException("password", "Password must be at least 8 characters.");
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

public sealed record AuthSessionResult(User User, string OrganizationName, string AccessToken);

public sealed record AuthMeResult(
    Guid UserId,
    string Email,
    string DisplayName,
    Guid TenantId,
    string OrganizationName,
    string Role);

public sealed class AuthValidationException : Exception
{
    public string Field { get; }

    public AuthValidationException(string field, string message) : base(message)
    {
        Field = field;
    }
}

public sealed class AuthConflictException : Exception
{
    public AuthConflictException(string message) : base(message)
    {
    }
}
