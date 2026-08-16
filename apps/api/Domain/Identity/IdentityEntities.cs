namespace ModelaFlow.Api.Domain.Identity;

public enum UserRole
{
    Owner = 0,
    Member = 1,
    Viewer = 2
}

public class Organization : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<User> Users { get; set; } = new List<User>();
}

public class User : TenantEntity
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Member;
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
}
