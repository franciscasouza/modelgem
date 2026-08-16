using Microsoft.Extensions.Options;

namespace ModelaFlow.Api.Auth;

public static class AuthCookieHelper
{
    public static void SetAuthCookie(HttpContext http, string token, AuthOptions options, IHostEnvironment env)
    {
        http.Response.Cookies.Append(options.CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = env.IsProduction(),
            SameSite = SameSiteMode.Lax,
            Path = "/",
            IsEssential = true,
            MaxAge = TimeSpan.FromMinutes(options.TokenLifetimeMinutes)
        });
    }

    public static void ClearAuthCookie(HttpContext http, AuthOptions options, IHostEnvironment env)
    {
        http.Response.Cookies.Delete(options.CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = env.IsProduction(),
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });
    }

    public static AuthOptions Resolve(IOptions<AuthOptions> options) => options.Value;
}
