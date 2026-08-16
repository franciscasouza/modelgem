namespace ModelaFlow.Api.Auth;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public string JwtIssuer { get; set; } = "ModelaFlow";
    public string JwtAudience { get; set; } = "ModelaFlow";
    public string JwtSigningKey { get; set; } = "DEV_ONLY_CHANGE_ME_ModelaFlow_Auth_Signing_Key_32+";
    public string CookieName { get; set; } = "mf_auth";
    public int TokenLifetimeMinutes { get; set; } = 480;

    public const string DevDemoEmail = "demo@modelaflow.local";
    public const string DevDemoPassword = "ChangeMe!";
}
