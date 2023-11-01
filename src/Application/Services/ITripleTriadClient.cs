using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Http.AutoClient;

namespace TripleTriad;

[AutoClient(nameof(ITripleTriadClient))]
public interface ITripleTriadClient
{
    [Post("register")]
    Task<string> Register([Body] RegisterRequest request, CancellationToken cancellationToken = default);

    [Post("login")]
    Task<AccessTokenResponse> Login([Body] LoginRequest request, CancellationToken cancellationToken = default);
}
