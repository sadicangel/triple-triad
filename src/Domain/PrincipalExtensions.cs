using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;

namespace TripleTriad;
public static class PrincipalExtensions
{
    private static string ThrowMissing(string claimType) => throw new($"{claimType} claim is missing");

    //[DebuggerStepThrough]
    //public static DateTime GetAuthenticationTime(this IPrincipal principal)
    //{
    //    return DateTimeOffset.FromUnixTimeSeconds(principal.GetAuthenticationTimeEpoch()).UtcDateTime;
    //}

    //[DebuggerStepThrough]
    //public static long GetAuthenticationTimeEpoch(this IPrincipal principal)
    //{
    //    return principal.Identity!.GetAuthenticationTimeEpoch();
    //}

    //[DebuggerStepThrough]
    //public static long GetAuthenticationTimeEpoch(this IIdentity identity)
    //{
    //    var claim = ((ClaimsIdentity)identity).FindFirst(JwtClaimTypes.AuthenticationTime);

    //    return claim is not null
    //        ? long.Parse(claim.Value)
    //        : throw ThrowMissingClaim(JwtClaimTypes.AuthenticationTime);
    //}

    [DebuggerStepThrough]
    public static string GetSubjectId(this ClaimsPrincipal? principal)
    {
        return principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? ThrowMissing(ClaimTypes.NameIdentifier);
    }

    [DebuggerStepThrough]
    public static string GetDisplayName(this ClaimsPrincipal? principal)
    {
        return principal?.FindFirstValue(ClaimTypes.Name) ?? ThrowMissing(ClaimTypes.Name);
    }

    [DebuggerStepThrough]
    public static string GetEmail(this ClaimsPrincipal? principal)
    {
        return principal?.FindFirstValue(ClaimTypes.Email) ?? ThrowMissing(ClaimTypes.Email);
    }

    [DebuggerStepThrough]
    public static bool IsAuthenticated(this IPrincipal principal)
    {
        return principal != null && principal.Identity != null && principal.Identity.IsAuthenticated;
    }

    //[DebuggerStepThrough]
    //public static string GetAuthenticationMethod(this IPrincipal principal)
    //{
    //    return principal.Identity!.GetAuthenticationMethod();
    //}

    //[DebuggerStepThrough]
    //public static IEnumerable<Claim> GetAuthenticationMethods(this IPrincipal principal)
    //{
    //    return principal.Identity!.GetAuthenticationMethods();
    //}

    //[DebuggerStepThrough]
    //public static string GetAuthenticationMethod(this IIdentity identity)
    //{
    //    var claim = ((ClaimsIdentity)identity).FindFirst(JwtClaimTypes.AuthenticationMethod);

    //    return claim is not null
    //        ? claim.Value
    //        : throw ThrowMissingClaim(JwtClaimTypes.AuthenticationMethod);
    //}

    //[DebuggerStepThrough]
    //public static IEnumerable<Claim> GetAuthenticationMethods(this IIdentity identity)
    //{
    //    return ((ClaimsIdentity)identity).FindAll(JwtClaimTypes.AuthenticationMethod);
    //}

    //[DebuggerStepThrough]
    //public static string GetIdentityProvider(this IPrincipal principal)
    //{
    //    return principal.Identity!.GetIdentityProvider();
    //}

    //[DebuggerStepThrough]
    //public static string GetIdentityProvider(this IIdentity identity)
    //{
    //    var claim = ((ClaimsIdentity)identity).FindFirst(JwtClaimTypes.IdentityProvider);

    //    return claim is not null
    //        ? claim.Value
    //        : throw ThrowMissingClaim(JwtClaimTypes.IdentityProvider);
    //}

    //[DebuggerStepThrough]
    //public static string? GetTenant(this ClaimsPrincipal principal)
    //{
    //    return principal.FindFirst("tenant")?.Value;
    //}

    //[DebuggerStepThrough]
    //public static IEnumerable<string> GetRoles(this IPrincipal principal)
    //{
    //    return principal.Identity!.GetRoles();
    //}

    //[DebuggerStepThrough]
    //public static IEnumerable<string> GetRoles(this IIdentity identity)
    //{
    //    return ((ClaimsIdentity)identity).FindAll(JwtClaimTypes.Role).Select(c => c.Value);
    //}

    //[DebuggerStepThrough]
    //public static bool GetEmailVerified(this IPrincipal principal)
    //{
    //    return principal.Identity!.GetEmailVerified();
    //}

    //[DebuggerStepThrough]
    //public static bool GetEmailVerified(this IIdentity identity)
    //{
    //    var claim = ((ClaimsIdentity)identity).FindFirst(JwtClaimTypes.Email);

    //    return claim is not null
    //        ? claim.Value == "true"
    //        : throw ThrowMissingClaim(JwtClaimTypes.EmailVerified);
    //}

    //[DebuggerStepThrough]
    //public static string GetPicture(this IPrincipal principal)
    //{
    //    return principal.Identity!.GetPicture();
    //}

    //[DebuggerStepThrough]
    //public static string GetPicture(this IIdentity identity)
    //{
    //    var claim = ((ClaimsIdentity)identity).FindFirst(JwtClaimTypes.Picture);

    //    return claim is not null
    //        ? claim.Value
    //        : throw ThrowMissingClaim(JwtClaimTypes.Picture);
    //}

    //[DebuggerStepThrough]
    //public static string GetLogoutUrl(this IPrincipal principal)
    //{
    //    return principal.Identity!.GetLogoutUrl();
    //}

    //[DebuggerStepThrough]
    //public static string GetLogoutUrl(this IIdentity identity)
    //{
    //    const string claimType = "bff:logout_url";
    //    var claim = ((ClaimsIdentity)identity).FindFirst(claimType);

    //    return claim is not null
    //        ? claim.Value
    //        : throw ThrowThrowMissingClaim(claimType);
    //}
}
