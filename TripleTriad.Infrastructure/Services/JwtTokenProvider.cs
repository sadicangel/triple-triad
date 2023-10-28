using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TripleTriad.Users;

namespace TripleTriad.Services;

internal sealed class JwtTokenProvider : IUserTwoFactorTokenProvider<User>
{
    private readonly TokenValidationParameters _validationParameters;
    private readonly SigningCredentials _signingCredentials;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly TimeSpan _accessTokenExpireTime;
    private readonly TimeSpan _refreshTokenExpireTime;

    public JwtTokenProvider(SecurityKey securityKey)
    {
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
            ValidateIssuer = false,
            ValidateAudience = false,
        };
        _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
        _tokenHandler = new JwtSecurityTokenHandler();
        _accessTokenExpireTime = TimeSpan.FromHours(1);
        _refreshTokenExpireTime = TimeSpan.FromDays(1);
    }

    public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<User> manager, User user) => Task.FromResult(false);

    private static ClaimsIdentity GetIdentity(User user)
    {
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
        identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName!));
        foreach (var claim in user.Claims)
            identity.AddClaim(claim.ToClaim());
        foreach (var role in user.Roles)
            identity.AddClaim(new Claim(JwtClaimTypes.Role, role));
        return identity;
    }

    private Task<string> GenerateAccessToken(User user)
    {
        var issuedDateTime = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = GetIdentity(user),
            Expires = issuedDateTime.Add(_accessTokenExpireTime),
            SigningCredentials = _signingCredentials,
        };
        var securityToken = _tokenHandler.CreateToken(tokenDescriptor);
        return Task.FromResult(_tokenHandler.WriteToken(securityToken));
    }

    private Task<string> GenerateRefreshToken(User user)
    {
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
        var issuedDateTime = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = identity,
            Expires = issuedDateTime.Add(_refreshTokenExpireTime),
            SigningCredentials = _signingCredentials,
        };
        var securityToken = _tokenHandler.CreateToken(tokenDescriptor);
        return Task.FromResult(_tokenHandler.WriteToken(securityToken));
    }

    public Task<string> GenerateAsync(string purpose, UserManager<User> manager, User user)
    {
        switch (purpose)
        {
            case "AccessToken":
                return GenerateAccessToken(user);
            case "RefreshToken":
                return GenerateRefreshToken(user);
            default:
                throw new InvalidOperationException($"Invalid token purpose");
        }
    }

    public Task<bool> ValidateAsync(string purpose, string token, UserManager<User> manager, User user)
    {
        switch (purpose)
        {
            case "AccessToken":
            case "RefreshToken":
                _tokenHandler.ValidateToken(token, _validationParameters, out var jwt);
                var subject = ((JwtSecurityToken)jwt)?.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject);
                return Task.FromResult(user.Id == subject?.Value);
            default:
                throw new InvalidOperationException($"Invalid token purpose");
        }
    }
}
